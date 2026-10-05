using System;
using SledSurfers.Progression;

namespace SledSurfers.UI.Lobby
{
    [UIView(typeof(LobbyWindowView), ViewType.Window)]
    public sealed class LobbyWindowController : UIController<LobbyWindowView, LobbyWindowArguments>
    {
        private Action _playRequested;
        private Action<PlayerUpgradeType> _upgradeRequested;

        public LobbyWindowController(LobbyWindowView view) : base(view) { }

        public override void Configure(LobbyWindowArguments arguments)
        {
            _playRequested = arguments.PlayRequested;
            _upgradeRequested = arguments.UpgradeRequested;
            TypedView.SetCoinBalance(arguments.CoinBalance);
            TypedView.SetUpgradeCards(arguments.SlingshotUpgrade, arguments.SkateUpgrade, arguments.IncomeUpgrade);
        }

        protected override void OnShow()
        {
            TypedView.PlayRequested += OnPlayRequested;
            TypedView.UpgradeRequested += OnUpgradeRequested;
        }

        protected override void OnHide()
        {
            TypedView.PlayRequested -= OnPlayRequested;
            TypedView.UpgradeRequested -= OnUpgradeRequested;
            _playRequested = null;
            _upgradeRequested = null;
        }

        private void OnPlayRequested()
        {
            _playRequested?.Invoke();
        }

        private void OnUpgradeRequested(PlayerUpgradeType upgradeType)
        {
            _upgradeRequested?.Invoke(upgradeType);
        }
    }
}
