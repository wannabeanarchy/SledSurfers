using System;
using SledSurfers.Progression;

namespace SledSurfers.UI.Lobby
{
    public readonly struct LobbyWindowArguments
    {
        public int CoinBalance { get; }
        public Action PlayRequested { get; }
        public LobbyUpgradeCardState SlingshotUpgrade { get; }
        public LobbyUpgradeCardState SkateUpgrade { get; }
        public LobbyUpgradeCardState IncomeUpgrade { get; }
        public Action<PlayerUpgradeType> UpgradeRequested { get; }

        public LobbyWindowArguments(
            int coinBalance,
            LobbyUpgradeCardState slingshotUpgrade,
            LobbyUpgradeCardState skateUpgrade,
            LobbyUpgradeCardState incomeUpgrade,
            Action<PlayerUpgradeType> upgradeRequested,
            Action playRequested)
        {
            CoinBalance = coinBalance;
            SlingshotUpgrade = slingshotUpgrade;
            SkateUpgrade = skateUpgrade;
            IncomeUpgrade = incomeUpgrade;
            UpgradeRequested = upgradeRequested ?? throw new ArgumentNullException(nameof(upgradeRequested));
            PlayRequested = playRequested ?? throw new ArgumentNullException(nameof(playRequested));
        }
    }
}
