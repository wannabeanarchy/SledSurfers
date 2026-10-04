using System;

namespace SledSurfers.UI.Lobby
{
    public readonly struct LobbyWindowArguments
    {
        public int CoinBalance { get; }
        public Action PlayRequested { get; }

        public LobbyWindowArguments(int coinBalance, Action playRequested)
        {
            CoinBalance = coinBalance;
            PlayRequested = playRequested ?? throw new ArgumentNullException(nameof(playRequested));
        }
    }
}
