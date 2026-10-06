using System;
using SledSurfers.Progression;

namespace SledSurfers.UI.Lobby
{
    [UIView(typeof(LobbyWindowView), ViewType.Window)]
    public sealed class LobbyWindowController : UIController<LobbyWindowView, LobbyWindowArguments>
    {
        private Action _playRequested;
        private Action _resetRequested;
        private Action<PlayerUpgradeType> _upgradeRequested;

        public LobbyWindowController(LobbyWindowView view) : base(view) { }

        public override void Configure(LobbyWindowArguments arguments)
        {
            _playRequested = arguments.PlayRequested;
            _resetRequested = arguments.ResetRequested;
            _upgradeRequested = arguments.UpgradeRequested;
            TypedView.SetCoinBalance(arguments.CoinBalance);
            TypedView.SetUpgradeCards(arguments.SlingshotUpgrade, arguments.SkateUpgrade, arguments.IncomeUpgrade);
        }

        protected override void OnShow()
        {
            TypedView.PlayRequested += OnPlayRequested;
            TypedView.ResetRequested += OnResetRequested;
            TypedView.UpgradeRequested += OnUpgradeRequested;
        }

        protected override void OnHide()
        {
            TypedView.PlayRequested -= OnPlayRequested;
            TypedView.ResetRequested -= OnResetRequested;
            TypedView.UpgradeRequested -= OnUpgradeRequested;
            _playRequested = null;
            _resetRequested = null;
            _upgradeRequested = null;
        }

        private void OnPlayRequested()
        {
            _playRequested?.Invoke();
        }

        private void OnResetRequested()
        {
            _resetRequested?.Invoke();
        }

        private void OnUpgradeRequested(PlayerUpgradeType upgradeType)
        {
            _upgradeRequested?.Invoke(upgradeType);
        }
    }
}
