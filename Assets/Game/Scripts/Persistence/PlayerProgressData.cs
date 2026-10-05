using System;

namespace SledSurfers.Persistence
{
    [Serializable]
    public sealed class PlayerProgressData
    {
        public int CoinBalance;
        public float BestDistanceMeters;
        public int SlingshotUpgradeLevel;
        public int SkateUpgradeLevel;
        public int IncomeUpgradeLevel;

        public bool ClampUpgradeLevels(int slingshotMaximum, int skateMaximum, int incomeMaximum)
        {
            var previousSlingshotLevel = SlingshotUpgradeLevel;
            var previousSkateLevel = SkateUpgradeLevel;
            var previousIncomeLevel = IncomeUpgradeLevel;

            SlingshotUpgradeLevel = ClampLevel(SlingshotUpgradeLevel, slingshotMaximum);
            SkateUpgradeLevel = ClampLevel(SkateUpgradeLevel, skateMaximum);
            IncomeUpgradeLevel = ClampLevel(IncomeUpgradeLevel, incomeMaximum);

            return previousSlingshotLevel != SlingshotUpgradeLevel ||
                   previousSkateLevel != SkateUpgradeLevel ||
                   previousIncomeLevel != IncomeUpgradeLevel;
        }

        private static int ClampLevel(int level, int maximum)
        {
            return Math.Min(Math.Max(0, level), Math.Max(0, maximum));
        }
    }
}
