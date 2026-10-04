using System;

namespace SledSurfers.UI.Lobby
{
    [UIView(typeof(LobbyWindowView), ViewType.Window)]
    public sealed class LobbyWindowController : UIController<LobbyWindowView, LobbyWindowArguments>
    {
        private Action _playRequested;

        public LobbyWindowController(LobbyWindowView view) : base(view) { }

        public override void Configure(LobbyWindowArguments arguments)
        {
            _playRequested = arguments.PlayRequested;
            TypedView.SetCoinBalance(arguments.CoinBalance);
        }

        protected override void OnShow()
        {
            TypedView.PlayRequested += OnPlayRequested;
        }

        protected override void OnHide()
        {
            TypedView.PlayRequested -= OnPlayRequested;
            _playRequested = null;
        }

        private void OnPlayRequested()
        {
            _playRequested?.Invoke();
        }
    }
}
