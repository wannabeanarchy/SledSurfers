namespace SledSurfers.UI.Lobby
{
    public readonly struct LobbyUpgradeCardState
    {
        public string CurrentValue { get; }
        public string Cost { get; }
        public int Level { get; }
        public bool CanPurchase { get; }

        public LobbyUpgradeCardState(string currentValue, string cost, int level, bool canPurchase)
        {
            CurrentValue = currentValue;
            Cost = cost;
            Level = level;
            CanPurchase = canPurchase;
        }
    }
}
