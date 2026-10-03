using System;

namespace SledSurfers.UI.Retry
{
    [UIView(typeof(RetryWidgetView), ViewType.Widget)]
    public sealed class RetryWidgetController : UIController<RetryWidgetView, Action>
    {
        private Action _retry;

        public RetryWidgetController(RetryWidgetView view) : base(view) { }

        public override void Configure(Action retry)
        {
            _retry = retry ?? throw new ArgumentNullException(nameof(retry));
        }

        protected override void OnShow()
        {
            TypedView.RetryRequested += OnRetryRequested;
        }

        protected override void OnHide()
        {
            TypedView.RetryRequested -= OnRetryRequested;
            _retry = null;
        }

        private void OnRetryRequested()
        {
            _retry?.Invoke();
        }
    }
}
