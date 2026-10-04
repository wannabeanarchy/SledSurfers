using SledSurfers.UI;
using SledSurfers.UI.Lobby;
using SledSurfers.Gameplay.Run;
using SledSurfers.Persistence;
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
        [SerializeField, Min(0)] private int _coinBalance;
        [SerializeField] private string _gameplaySceneName = "GameScene";

        private UIManager _ui;
        private bool _isLoadingGame;

        private void Awake()
        {
            Screen.orientation = ScreenOrientation.Portrait;
        }

        private void Start()
        {
            if (_uiConfig == null || _windowRoot == null || _popupRoot == null || _widgetRoot == null || string.IsNullOrWhiteSpace(_gameplaySceneName))
            {
                Debug.LogError("Lobby UI references or gameplay scene name are invalid.", this);
                enabled = false;
                return;
            }

            var factory = new UIFactory(_uiConfig, _windowRoot, _popupRoot, _widgetRoot);
            factory.Register<LobbyWindowController, LobbyWindowView, LobbyWindowArguments>(view => new LobbyWindowController(view));
            _ui = new UIManager(factory);
            var progressStorage = new PlayerProgressStorage();
            var progress = progressStorage.Load(_coinBalance);
            SessionWallet.Initialize(progress.CoinBalance);
            _ui.Show<LobbyWindowController, LobbyWindowView, LobbyWindowArguments>(new LobbyWindowArguments(SessionWallet.Balance, StartGame));
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
    }
}
