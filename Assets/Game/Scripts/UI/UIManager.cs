using System;
using System.Collections.Generic;

namespace SledSurfers.UI
{
    public sealed class UIManager : IDisposable
    {
        private readonly UIFactory _factory;
        private readonly Dictionary<Type, UIController> _controllers = new Dictionary<Type, UIController>();
        private readonly Dictionary<Type, ViewType> _kinds = new Dictionary<Type, ViewType>();
        private UIController _window;
        private UIController _popup;
        private bool _disposed;

        public bool BlocksGameplayInput => _popup != null && _popup.IsVisible;

        public UIManager(UIFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public TController Show<TController, TView, TArguments>(TArguments arguments)
            where TController : UIController<TView, TArguments> where TView : UIView
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(UIManager));
            }
            var type = typeof(TController);
            if (!_controllers.TryGetValue(type, out var controller))
            {
                controller = _factory.Create(type, out var kind);
                _controllers.Add(type, controller);
                _kinds.Add(type, kind);
            }
            var typedController = (TController)controller;
            typedController.Configure(arguments);
            switch (_kinds[type])
            {
                case ViewType.Window:
                    if (_window != controller)
                    {
                        HidePopup();
                        _window?.Hide();
                        _window = controller;
                    }
                    break;
                case ViewType.Popup:
                    if (_popup != controller)
                    {
                        HidePopup();
                        _popup = controller;
                    }
                    break;
            }
            controller.Show();
            return typedController;
        }

        public void Hide<TController>() where TController : UIController
        {
            if (!_controllers.TryGetValue(typeof(TController), out var controller))
            {
                return;
            }
            if (_window == controller)
            {
                HidePopup();
                _window = null;
            }
            if (_popup == controller)
            {
                _popup = null;
            }
            controller.Hide();
        }

        public void HideAll()
        {
            foreach (var controller in _controllers.Values)
            {
                controller.Hide();
            }
            _window = null;
            _popup = null;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            HideAll();
            foreach (var controller in _controllers.Values)
            {
                controller.Dispose();
                if (controller.View != null)
                {
                    UnityEngine.Object.Destroy(controller.View.gameObject);
                }
            }
            _controllers.Clear();
            _kinds.Clear();
        }

        private void HidePopup()
        {
            _popup?.Hide();
            _popup = null;
        }
    }
}
