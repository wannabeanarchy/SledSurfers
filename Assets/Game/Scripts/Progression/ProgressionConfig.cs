using System;
using UnityEngine;

namespace SledSurfers.Progression
{
    [CreateAssetMenu(menuName = "Sled Surfers/Progression Config")]
    public sealed class ProgressionConfig : ScriptableObject
    {
        [SerializeField, Min(0), Tooltip("Base launch speed in meters per second.")]
        private float _baseLaunchSpeed = 40;
        [SerializeField, Min(0), Tooltip("Additional launch speed per upgrade level, in meters per second.")]
        private float _launchSpeedPerLevel = 4;
        [SerializeField] private UpgradeCostCurve _slingshotCosts = new UpgradeCostCurve(500, 1.5f, 10);

        [SerializeField, Min(0)] private float _baseGroundTurnRate = 25;
        [SerializeField, Min(1)] private float _groundTurnRateGrowthPerLevel = 1.2f;
        [SerializeField, Min(0)] private float _baseAirTurnRate = 10;
        [SerializeField, Min(1)] private float _airTurnRateGrowthPerLevel = 1.15f;
        [SerializeField, Range(0, 80)] private float _baseMaximumSteeringAngle = 15;
        [SerializeField, Min(0)] private float _maximumSteeringAnglePerLevel = 4;
        [SerializeField] private UpgradeCostCurve _skateCosts = new UpgradeCostCurve(600, 1.5f, 10);

        [SerializeField, Min(0)] private float _baseCoinsPerKilometer = 1000;
        [SerializeField, Min(0)] private float _incomeMultiplierPerLevel = .2f;
        [SerializeField] private UpgradeCostCurve _incomeCosts = new UpgradeCostCurve(1000, 1.6f, 10);
        [SerializeField, Min(0)] private int _coinsPerCollectedCoin = 100;

        public int CoinsPerCollectedCoin => _coinsPerCollectedCoin;

        public float GetMaximumLaunchSpeed(int level)
        {
            return Mathf.Max(0, _baseLaunchSpeed + Mathf.Max(0, level) * _launchSpeedPerLevel);
        }

        public float GetGroundTurnRate(int level)
        {
            return Mathf.Max(0, _baseGroundTurnRate * Mathf.Pow(Mathf.Max(1, _groundTurnRateGrowthPerLevel), Mathf.Max(0, level)));
        }

        public float GetAirTurnRate(int level)
        {
            return Mathf.Max(0, _baseAirTurnRate * Mathf.Pow(Mathf.Max(1, _airTurnRateGrowthPerLevel), Mathf.Max(0, level)));
        }

        public float GetMaximumSteeringAngle(int level)
        {
            return Mathf.Clamp(_baseMaximumSteeringAngle + Mathf.Max(0, level) * _maximumSteeringAnglePerLevel, 0, 80);
        }

        public float GetCoinsPerKilometer(int level)
        {
            return Mathf.Max(0, _baseCoinsPerKilometer * (1 + Mathf.Max(0, level) * _incomeMultiplierPerLevel));
        }

        public int GetUpgradeCost(PlayerUpgradeType upgradeType, int currentLevel)
        {
            return GetCostCurve(upgradeType).GetCost(currentLevel);
        }

        public int GetMaximumLevel(PlayerUpgradeType upgradeType)
        {
            return GetCostCurve(upgradeType).MaximumLevel;
        }

        private UpgradeCostCurve GetCostCurve(PlayerUpgradeType upgradeType)
        {
            switch (upgradeType)
            {
                case PlayerUpgradeType.Slingshot:
                    return _slingshotCosts;
                case PlayerUpgradeType.Skate:
                    return _skateCosts;
                case PlayerUpgradeType.Income:
                    return _incomeCosts;
                default:
                    throw new ArgumentOutOfRangeException(nameof(upgradeType), upgradeType, null);
            }
        }

        [Serializable]
        private sealed class UpgradeCostCurve
        {
            [SerializeField, Min(0)] private int _startingCost;
            [SerializeField, Min(1)] private float _growthPerLevel = 1.5f;
            [SerializeField, Min(0)] private int _maximumLevel = 10;

            public int MaximumLevel => Mathf.Max(0, _maximumLevel);

            public UpgradeCostCurve(int startingCost, float growthPerLevel, int maximumLevel)
            {
                _startingCost = startingCost;
                _growthPerLevel = growthPerLevel;
                _maximumLevel = maximumLevel;
            }

            public int GetCost(int currentLevel)
            {
                if (currentLevel < 0 || currentLevel >= MaximumLevel)
                {
                    return 0;
                }

                var growth = Mathf.Max(1, _growthPerLevel);
                var cost = _startingCost * Mathf.Pow(growth, currentLevel);
                return Mathf.Clamp(Mathf.CeilToInt(cost), 0, int.MaxValue);
            }
        }
    }
}
