using UnityEngine;

namespace SledSurfers.Persistence
{
    public sealed class PlayerProgressStorage
    {
        private const string SaveKey = "sled_surfer_player_progress";

        public PlayerProgressData Load(int defaultCoinBalance = 0)
        {
            var defaultProgress = new PlayerProgressData
            {
                CoinBalance = Mathf.Max(0, defaultCoinBalance)
            };

            if (!PlayerPrefs.HasKey(SaveKey))
            {
                return defaultProgress;
            }

            try
            {
                var progress = JsonUtility.FromJson<PlayerProgressData>(PlayerPrefs.GetString(SaveKey));
                if (progress == null)
                {
                    return defaultProgress;
                }

                progress.CoinBalance = Mathf.Max(0, progress.CoinBalance);
                progress.BestDistanceMeters = IsValidDistance(progress.BestDistanceMeters) ? Mathf.Max(0, progress.BestDistanceMeters) : 0;
                progress.SlingshotUpgradeLevel = Mathf.Max(0, progress.SlingshotUpgradeLevel);
                progress.SkateUpgradeLevel = Mathf.Max(0, progress.SkateUpgradeLevel);
                progress.IncomeUpgradeLevel = Mathf.Max(0, progress.IncomeUpgradeLevel);
                return progress;
            }
            catch (System.ArgumentException)
            {
                return defaultProgress;
            }
        }

        public void Save(PlayerProgressData progress)
        {
            var safeProgress = new PlayerProgressData
            {
                CoinBalance = progress != null ? Mathf.Max(0, progress.CoinBalance) : 0,
                BestDistanceMeters = progress != null && IsValidDistance(progress.BestDistanceMeters)
                    ? Mathf.Max(0, progress.BestDistanceMeters)
                    : 0,
                SlingshotUpgradeLevel = progress != null ? Mathf.Max(0, progress.SlingshotUpgradeLevel) : 0,
                SkateUpgradeLevel = progress != null ? Mathf.Max(0, progress.SkateUpgradeLevel) : 0,
                IncomeUpgradeLevel = progress != null ? Mathf.Max(0, progress.IncomeUpgradeLevel) : 0
            };

            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(safeProgress));
            PlayerPrefs.Save();
        }

        public void Reset()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
        }

        private static bool IsValidDistance(float distance)
        {
            return !float.IsNaN(distance) && !float.IsInfinity(distance);
        }
    }
}
