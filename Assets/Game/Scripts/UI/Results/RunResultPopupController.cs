using System;

namespace SledSurfers.UI.Results
{
    [UIView(typeof(RunResultPopupView), ViewType.Popup)]
    public sealed class RunResultPopupController : UIController<RunResultPopupView, RunResultPopupArguments>
    {
        private Action _continueRequested;

        public RunResultPopupController(RunResultPopupView view) : base(view) { }

        public override void Configure(RunResultPopupArguments arguments)
        {
            _continueRequested = arguments.ContinueRequested;
            TypedView.SetCoinsEarned(arguments.CoinsEarned);
        }

        protected override void OnShow()
        {
            TypedView.ContinueRequested += OnContinueRequested;
        }

        protected override void OnHide()
        {
            TypedView.ContinueRequested -= OnContinueRequested;
            _continueRequested = null;
        }

        private void OnContinueRequested()
        {
            _continueRequested?.Invoke();
        }
    }
}
