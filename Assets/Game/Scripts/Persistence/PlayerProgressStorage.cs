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
                CoinBalance = progress != null ? Mathf.Max(0, progress.CoinBalance) : 0
            };

            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(safeProgress));
            PlayerPrefs.Save();
        }
    }
}
