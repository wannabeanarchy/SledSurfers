using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SledSurfers.UI
{
    public sealed class UIFactory
    {
        private sealed class Registration
        {
            public UIView Prefab;
            public ViewType Kind;
            public Func<UIView, UIController> Create;
        }

        private readonly Dictionary<Type, UIView> _catalog;
        private readonly Dictionary<Type, Registration> _registrations = new Dictionary<Type, Registration>();
        private readonly Transform _windowRoot;
        private readonly Transform _popupRoot;
        private readonly Transform _widgetRoot;

        public UIFactory(UIConfig config, Transform windowRoot, Transform popupRoot, Transform widgetRoot)
        {
            if (config == null || windowRoot == null || popupRoot == null || widgetRoot == null)
            {
                throw new ArgumentException("UI catalog and layer roots are required.");
            }
            _catalog = config.CreateCatalog();
            _windowRoot = windowRoot;
            _popupRoot = popupRoot;
            _widgetRoot = widgetRoot;
        }

        public void Register<TController, TView, TArguments>(Func<TView, TController> create)
            where TController : UIController<TView, TArguments> where TView : UIView
        {
            var controllerType = typeof(TController);
            var attribute = controllerType.GetCustomAttribute<UIViewAttribute>();
            if (create == null || attribute == null || attribute.View != typeof(TView) ||
                !Enum.IsDefined(typeof(ViewType), attribute.Kind))
            {
                throw new ArgumentException($"Invalid UI registration: {controllerType.Name}.");
            }
            if (_registrations.ContainsKey(controllerType))
            {
                throw new InvalidOperationException($"UI controller already registered: {controllerType.Name}.");
            }
            if (!_catalog.TryGetValue(typeof(TView), out var prefab))
            {
                throw new InvalidOperationException($"UI prefab missing: {typeof(TView).Name}.");
            }
            _registrations.Add(controllerType, new Registration
            {
                Prefab = prefab,
                Kind = attribute.Kind,
                Create = view => create((TView)view)
            });
        }

        internal UIController Create(Type controllerType, out ViewType kind)
        {
            if (!_registrations.TryGetValue(controllerType, out var registration))
            {
                throw new InvalidOperationException($"UI controller not registered: {controllerType.Name}.");
            }
            kind = registration.Kind;
            var root = kind == ViewType.Window ? _windowRoot : kind == ViewType.Popup ? _popupRoot : _widgetRoot;
            var view = UnityEngine.Object.Instantiate(registration.Prefab, root, false);
            view.SetVisible(false);
            try
            {
                return registration.Create(view) ?? throw new InvalidOperationException("UI factory returned no controller.");
            }
            catch
            {
                UnityEngine.Object.Destroy(view.gameObject);
                throw;
            }
        }
    }
}
