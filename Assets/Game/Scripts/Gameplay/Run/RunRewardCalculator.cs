using UnityEngine;

namespace SledSurfers.Gameplay.Run
{
    public static class RunRewardCalculator
    {
        private const int CoinsPerPickup = 100;

        public static int CalculateCoins(float distanceMeters, int collectedCoinCount, float coinsPerMeter)
        {
            var distanceReward = Mathf.FloorToInt(Mathf.Max(0, distanceMeters) * Mathf.Max(0, coinsPerMeter));
            var pickupReward = Mathf.Max(0, collectedCoinCount) * CoinsPerPickup;
            return distanceReward + pickupReward;
        }
    }
}
