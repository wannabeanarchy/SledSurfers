using SledSurfers.UI;
using SledSurfers.UI.Lobby;
using SledSurfers.Persistence;
using SledSurfers.Progression;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SledSurfers.Composition
{
    public sealed class LobbyCompositionRoot : MonoBehaviour
    {
        [SerializeField] private UIConfig _uiConfig;
        [SerializeField] private Transform _windowRoot;
        [SerializeField] private Transform _popupRoot;
        [SerializeField] private Transform _widgetRoot;
        [SerializeField] private ProgressionConfig _progressionConfig;
        [SerializeField, Min(0)] private int _coinBalance;
        [SerializeField] private string _gameplaySceneName = "GameScene";

        private UIManager _ui;
        private PlayerProgressStorage _progressStorage;
        private PlayerProgressData _progress;
        private bool _isLoadingGame;

        private void Start()
        {
            if (_uiConfig == null || _windowRoot == null || _popupRoot == null || _widgetRoot == null || _progressionConfig == null || string.IsNullOrWhiteSpace(_gameplaySceneName))
            {
                Debug.LogError("Lobby UI references or gameplay scene name are invalid.", this);
                enabled = false;
                return;
            }

            var factory = new UIFactory(_uiConfig, _windowRoot, _popupRoot, _widgetRoot);
            factory.Register<LobbyWindowController, LobbyWindowView, LobbyWindowArguments>(view => new LobbyWindowController(view));
            _ui = new UIManager(factory);
            _progressStorage = new PlayerProgressStorage();
            _progress = _progressStorage.Load(_coinBalance);
            if (_progress.ClampUpgradeLevels(
                    _progressionConfig.GetMaximumLevel(PlayerUpgradeType.Slingshot),
                    _progressionConfig.GetMaximumLevel(PlayerUpgradeType.Skate),
                    _progressionConfig.GetMaximumLevel(PlayerUpgradeType.Income)))
            {
                _progressStorage.Save(_progress);
            }

            RefreshLobby();
        }

        private void OnDestroy()
        {
            _ui?.Dispose();
        }

        private void StartGame()
        {
            if (_isLoadingGame)
            {
                return;
            }

            _isLoadingGame = true;
            SceneManager.LoadScene(_gameplaySceneName);
        }

        private void OnUpgradeRequested(PlayerUpgradeType upgradeType)
        {
            var currentLevel = GetUpgradeLevel(upgradeType);
            var cost = _progressionConfig.GetUpgradeCost(upgradeType, currentLevel);
            if (currentLevel >= _progressionConfig.GetMaximumLevel(upgradeType) || _progress.CoinBalance < cost)
            {
                RefreshLobby();
                return;
            }

            _progress.CoinBalance -= cost;
            SetUpgradeLevel(upgradeType, currentLevel + 1);
            _progressStorage.Save(_progress);
            RefreshLobby();
        }

        private void OnResetRequested()
        {
            _progressStorage.Reset();
            _progress = _progressStorage.Load(_coinBalance);
            RefreshLobby();
        }

        private void RefreshLobby()
        {
            var balance = _progress.CoinBalance;
            var arguments = new LobbyWindowArguments(
                balance,
                GetUpgradeCardState(PlayerUpgradeType.Slingshot, balance),
                GetUpgradeCardState(PlayerUpgradeType.Skate, balance),
                GetUpgradeCardState(PlayerUpgradeType.Income, balance),
                OnUpgradeRequested,
                StartGame,
                OnResetRequested);
            _ui.Show<LobbyWindowController, LobbyWindowView, LobbyWindowArguments>(arguments);
        }

        private LobbyUpgradeCardState GetUpgradeCardState(PlayerUpgradeType upgradeType, int balance)
        {
            var level = GetUpgradeLevel(upgradeType);
            var maximumLevel = _progressionConfig.GetMaximumLevel(upgradeType);
            var isMaxLevel = level >= maximumLevel;
            var nextUpgradeCost = isMaxLevel ? 0 : _progressionConfig.GetUpgradeCost(upgradeType, level);
            var costLabel = isMaxLevel ? "MAX" : nextUpgradeCost.ToString("N0");
            return new LobbyUpgradeCardState(GetCurrentUpgradeValue(upgradeType, level), costLabel, level, !isMaxLevel && balance >= nextUpgradeCost);
        }

        private string GetCurrentUpgradeValue(PlayerUpgradeType upgradeType, int level)
        {
            switch (upgradeType)
            {
                case PlayerUpgradeType.Slingshot:
                    return $"{_progressionConfig.GetMaximumLaunchSpeed(level):0.#} m/s";
                case PlayerUpgradeType.Skate:
                    return $"{_progressionConfig.GetGroundTurnRate(level):0.#}/{_progressionConfig.GetAirTurnRate(level):0.#}";
                case PlayerUpgradeType.Income:
                    return $"{_progressionConfig.GetCoinsPerKilometer(level) / 1000f:0.##}/m · {_progressionConfig.GetCoinsPerCollectedCoin(level):N0}/coin";
                default:
                    return string.Empty;
            }
        }

        private int GetUpgradeLevel(PlayerUpgradeType upgradeType)
        {
            switch (upgradeType)
            {
                case PlayerUpgradeType.Slingshot:
                    return _progress.SlingshotUpgradeLevel;
                case PlayerUpgradeType.Skate:
                    return _progress.SkateUpgradeLevel;
                case PlayerUpgradeType.Income:
                    return _progress.IncomeUpgradeLevel;
                default:
                    return 0;
            }
        }

        private void SetUpgradeLevel(PlayerUpgradeType upgradeType, int level)
        {
            switch (upgradeType)
            {
                case PlayerUpgradeType.Slingshot:
                    _progress.SlingshotUpgradeLevel = level;
                    break;
                case PlayerUpgradeType.Skate:
                    _progress.SkateUpgradeLevel = level;
                    break;
                case PlayerUpgradeType.Income:
                    _progress.IncomeUpgradeLevel = level;
                    break;
            }
        }
    }
}
