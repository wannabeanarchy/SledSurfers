using System;
using System.Collections;
using SledSurfers.Gameplay.Collectibles;
using SledSurfers.Gameplay.Launch;
using SledSurfers.Gameplay.Player;
using SledSurfers.Persistence;
using SledSurfers.Presentation;
using SledSurfers.Progression;
using SledSurfers.UI;
using SledSurfers.UI.Hud;
using SledSurfers.UI.Results;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SledSurfers.Composition
{
    public sealed class GameplayCompositionRoot : MonoBehaviour
    {
        [SerializeField] private PlayerMotor _player;
        [SerializeField] private Collider[] _surfaces;
        [SerializeField, Range(0, 30)] private float _maximumLaunchAngle = 12;
        [SerializeField, Range(.05f, .8f)] private float _pullScreenFraction = .25f;
        [SerializeField] private PlayerAnimation _animation;
        [SerializeField] private SlingshotRopeVisual _slingshotRopeVisual;
        [SerializeField, Min(0)] private float _maximumLateralPullDistance = 2.5f;
        [SerializeField, Min(0)] private float _maximumBackwardPullDistance = 4f;
        [SerializeField] private float _minimumX = -18;
        [SerializeField] private float _maximumX = 18;
        [SerializeField, Min(24)] private float _steeringJoystickRadius = 90;
        [SerializeField] private UIConfig _uiConfig;
        [SerializeField] private Transform _windowRoot;
        [SerializeField] private Transform _popupRoot;
        [SerializeField] private Transform _widgetRoot;
        [SerializeField] private ProgressionConfig _progressionConfig;
        [SerializeField, Min(0), Tooltip("Distance goal in meters. Zero uses the end of the active road colliders.")]
        private float _hudTargetDistance;
        [SerializeField] private string _lobbySceneName = "LobbyScene";
        [SerializeField] private FinishFlagVisual _finishFlag;
        [SerializeField, Min(0)] private float _finishFlagSideOffset = 1.5f;
        [SerializeField] private CoinPickup[] _coins = Array.Empty<CoinPickup>();

        private GameplayRunCoordinator _runCoordinator;
        private bool _isLoadingRun;
        private bool _isLoadingLobby;

        private void Start()
        {
            var skateboardGroundCollider = _animation != null ? _animation.SkateboardGroundCollider : null;
            if (_player == null || _surfaces == null || _surfaces.Length == 0 || _animation == null ||
                skateboardGroundCollider == null || _progressionConfig == null || _minimumX >= _maximumX ||
                _uiConfig == null || _windowRoot == null || _popupRoot == null || _widgetRoot == null ||
                string.IsNullOrWhiteSpace(_lobbySceneName))
            {
                Debug.LogError("Gameplay references or track limits are invalid.", this);
                enabled = false;
                return;
            }

            var startZ = skateboardGroundCollider.bounds.center.z;
            var trackEndZ = startZ;
            var hasActiveTrackSurface = false;
            foreach (var surface in _surfaces)
            {
                if (surface != null && surface.enabled && surface.gameObject.activeInHierarchy)
                {
                    hasActiveTrackSurface = true;
                    trackEndZ = Mathf.Max(trackEndZ, surface.bounds.max.z);
                }
            }

            var targetDistance = _hudTargetDistance > 0 ? _hudTargetDistance : trackEndZ - startZ;
            if (!hasActiveTrackSurface || targetDistance <= 0)
            {
                Debug.LogError("Gameplay needs at least one active track collider and a positive target distance.", this);
                enabled = false;
                return;
            }

            _coins ??= Array.Empty<CoinPickup>();
            var progressStorage = new PlayerProgressStorage();
            var playerProgress = progressStorage.Load();
            if (playerProgress.ClampUpgradeLevels(
                    _progressionConfig.GetMaximumLevel(PlayerUpgradeType.Slingshot),
                    _progressionConfig.GetMaximumLevel(PlayerUpgradeType.Skate),
                    _progressionConfig.GetMaximumLevel(PlayerUpgradeType.Income)))
            {
                progressStorage.Save(playerProgress);
            }

            var factory = new UIFactory(_uiConfig, _windowRoot, _popupRoot, _widgetRoot);
            factory.Register<RunResultPopupController, RunResultPopupView, RunResultPopupArguments>(view => new RunResultPopupController(view));
            GameplayHud.Register(factory);
            var ui = new UIManager(factory);
            var session = new LaunchSession(
                _progressionConfig.GetMaximumLaunchSpeed(playerProgress.SlingshotUpgradeLevel),
                _maximumLaunchAngle,
                _pullScreenFraction);

            if (_slingshotRopeVisual != null)
            {
                _slingshotRopeVisual.Initialize(session, _player.transform);
            }
            _animation.Initialize(session);
            _player.Initialize(_surfaces, _minimumX, _maximumX, skateboardGroundCollider);
            _player.ConfigureHandling(
                _progressionConfig.GetGroundTurnRate(playerProgress.SkateUpgradeLevel),
                _progressionConfig.GetAirTurnRate(playerProgress.SkateUpgradeLevel),
                _progressionConfig.GetMaximumSteeringAngle(playerProgress.SkateUpgradeLevel));
            _player.Prepare();

            _runCoordinator = new GameplayRunCoordinator(new GameplayRunCoordinator.Dependencies
            {
                Player = _player,
                Animation = _animation,
                Session = session,
                UI = ui,
                ProgressionConfig = _progressionConfig,
                PlayerProgress = playerProgress,
                ProgressStorage = progressStorage,
                FinishFlag = _finishFlag,
                Coins = _coins,
                WidgetRoot = _widgetRoot,
                InitialStartZ = startZ,
                TrackEndZ = trackEndZ,
                TargetDistance = targetDistance,
                HUDTargetDistance = _hudTargetDistance,
                MinimumX = _minimumX,
                MaximumX = _maximumX,
                MaximumLateralPullDistance = _maximumLateralPullDistance,
                MaximumBackwardPullDistance = _maximumBackwardPullDistance,
                SteeringJoystickRadius = _steeringJoystickRadius,
                FinishFlagSideOffset = _finishFlagSideOffset,
                StartCoroutine = routine => StartCoroutine(routine),
                RetryRequested = RetryRun,
                ContinueRequested = ContinueToLobby
            });
        }

        private void Update()
        {
            _runCoordinator?.Tick();
        }

        private void LateUpdate()
        {
            _runCoordinator?.LateTick();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            _runCoordinator?.SetApplicationFocus(hasFocus);
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                _runCoordinator?.CancelInput();
            }
        }

        private void OnDisable()
        {
            _runCoordinator?.OnHostDisabled();
        }

        private void OnDestroy()
        {
            _runCoordinator?.Dispose();
        }

        private void ContinueToLobby()
        {
            if (_isLoadingLobby || _isLoadingRun)
            {
                return;
            }

            _isLoadingLobby = true;
            SceneManager.LoadScene(_lobbySceneName);
        }

        private void RetryRun()
        {
            if (_isLoadingRun || _isLoadingLobby)
            {
                return;
            }

            _isLoadingRun = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
