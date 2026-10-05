using UnityEngine;

namespace SledSurfers.Gameplay.Run
{
    public static class RunRewardCalculator
    {
        public static int CalculateCoins(float distanceMeters, int collectedCoinCount, float coinsPerKilometer, int coinsPerPickup)
        {
            var distanceReward = Mathf.FloorToInt(Mathf.Max(0, distanceMeters) * Mathf.Max(0, coinsPerKilometer) / 1000f);
            var pickupReward = Mathf.Max(0, collectedCoinCount) * Mathf.Max(0, coinsPerPickup);
            return distanceReward + pickupReward;
        }
    }
}
