using System;

namespace SledSurfers.UI.Results
{
    [UIView(typeof(RunResultPopupView), ViewType.Popup)]
    public sealed class RunResultPopupController : UIController<RunResultPopupView, RunResultPopupArguments>
    {
        private Action _retryRequested;
        private Action _continueRequested;

        public RunResultPopupController(RunResultPopupView view) : base(view) { }

        public override void Configure(RunResultPopupArguments arguments)
        {
            _retryRequested = arguments.RetryRequested;
            _continueRequested = arguments.ContinueRequested;
            TypedView.SetCoinsEarned(arguments.CoinsEarned);
        }

        protected override void OnShow()
        {
            TypedView.RetryRequested += OnRetryRequested;
            TypedView.ContinueRequested += OnContinueRequested;
        }

        protected override void OnHide()
        {
            TypedView.RetryRequested -= OnRetryRequested;
            TypedView.ContinueRequested -= OnContinueRequested;
            _retryRequested = null;
            _continueRequested = null;
        }

        private void OnRetryRequested()
        {
            _retryRequested?.Invoke();
        }

        private void OnContinueRequested()
        {
            _continueRequested?.Invoke();
        }
    }
}
