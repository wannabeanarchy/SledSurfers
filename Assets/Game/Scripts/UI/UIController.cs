using System;

namespace SledSurfers.UI
{
    public abstract class UIController : IDisposable
    {
        public bool IsVisible { get; private set; }
        public UIView View { get; }

        protected UIController(UIView view)
        {
            View = view != null ? view : throw new ArgumentNullException(nameof(view));
            View.SetVisible(false);
        }

        internal void Show()
        {
            if (IsVisible)
            {
                return;
            }
            IsVisible = true;
            View.SetVisible(true);
            OnShow();
        }

        internal void Hide()
        {
            if (!IsVisible)
            {
                return;
            }
            IsVisible = false;
            OnHide();
            if (View != null)
            {
                View.SetVisible(false);
            }
        }

        protected virtual void OnShow() { }
        protected virtual void OnHide() { }

        public void Dispose()
        {
            Hide();
            OnDispose();
        }

        protected virtual void OnDispose() { }
    }

    public abstract class UIController<TView, TArguments> : UIController where TView : UIView
    {
        protected TView TypedView { get; }

        protected UIController(TView view) : base(view)
        {
            TypedView = view;
        }

        public abstract void Configure(TArguments arguments);
    }
}
