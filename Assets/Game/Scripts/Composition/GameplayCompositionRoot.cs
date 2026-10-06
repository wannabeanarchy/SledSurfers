using System.Collections;
using SledSurfers.Gameplay.Player;
using SledSurfers.Input;
using System;
using SledSurfers.UI;
using SledSurfers.UI.Results;
using SledSurfers.UI.Hud;
using SledSurfers.Gameplay.Run;
using UnityEngine;
using SledSurfers.Gameplay.Launch;
using SledSurfers.Gameplay.Collectibles;
using SledSurfers.Presentation;
using SledSurfers.Persistence;
using SledSurfers.Progression;
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
        private float _trackEndZ;
        private float _targetDistance;
        private float _bestDistanceMeters;
        private float[] _coinProgressPositions;
        private RunMetrics _metrics;
        private GameplayHud _hud;
        private UIManager _ui;
        private DragSteering _steering;
        private SteeringJoystickVisual _steeringJoystick;
        private PointerInput _input;
        private LaunchSession _session;
        private PlayerProgressStorage _progressStorage;
        private PlayerProgressData _playerProgress;
        private bool _hasFocus = true;
        private bool _isLoadingRun;
        private bool _isLoadingLobby;

        private void Start()
        {
            var skateboardGroundCollider = _animation != null ? _animation.SkateboardGroundCollider : null;
            if (_player == null || _surfaces == null || _surfaces.Length == 0 || _animation == null || skateboardGroundCollider == null || _progressionConfig == null || _minimumX >= _maximumX || _uiConfig == null || _windowRoot == null || _popupRoot == null || _widgetRoot == null || string.IsNullOrWhiteSpace(_lobbySceneName))
            {
                Debug.LogError("Gameplay references or track limits are invalid.", this);
                enabled = false;
                return;
            }

            var startZ = skateboardGroundCollider.bounds.center.z;
            _trackEndZ = startZ;
            var hasActiveTrackSurface = false;
            foreach (var surface in _surfaces)
            {
                if (surface != null && surface.enabled && surface.gameObject.activeInHierarchy)
                {
                    hasActiveTrackSurface = true;
                    _trackEndZ = Mathf.Max(_trackEndZ, surface.bounds.max.z);
                }
            }

            _targetDistance = _hudTargetDistance > 0 ? _hudTargetDistance : _trackEndZ - startZ;
            if (!hasActiveTrackSurface || _targetDistance <= 0)
            {
                Debug.LogError("Gameplay needs at least one active track collider and a positive target distance.", this);
                enabled = false;
                return;
            }

            if (_coins == null)
            {
                _coins = Array.Empty<CoinPickup>();
            }
            _progressStorage = new PlayerProgressStorage();
            _playerProgress = _progressStorage.Load();
            if (_playerProgress.ClampUpgradeLevels(
                    _progressionConfig.GetMaximumLevel(PlayerUpgradeType.Slingshot),
                    _progressionConfig.GetMaximumLevel(PlayerUpgradeType.Skate),
                    _progressionConfig.GetMaximumLevel(PlayerUpgradeType.Income)))
            {
                _progressStorage.Save(_playerProgress);
            }

            SessionWallet.Initialize(_playerProgress.CoinBalance);
            _bestDistanceMeters = _playerProgress.BestDistanceMeters;
            var factory = new UIFactory(_uiConfig, _windowRoot, _popupRoot, _widgetRoot);
            factory.Register<RunResultPopupController, RunResultPopupView, RunResultPopupArguments>(view => new RunResultPopupController(view));
            GameplayHud.Register(factory);
            _ui = new UIManager(factory);
            _metrics = new RunMetrics();
            _coinProgressPositions = new float[_coins.Length];
            for (var i = 0; i < _coins.Length; i++)
            {
                if (_coins[i] != null && _targetDistance > 0)
                {
                    _coinProgressPositions[i] = Mathf.Clamp01((_coins[i].transform.position.z - startZ) / _targetDistance);
                }
            }
            _hud = new GameplayHud(_ui, _coinProgressPositions);
            _steering = new DragSteering();
            _steeringJoystick = new SteeringJoystickVisual(_widgetRoot, _steeringJoystickRadius);
            _session = new LaunchSession(
                _progressionConfig.GetMaximumLaunchSpeed(_playerProgress.SlingshotUpgradeLevel),
                _maximumLaunchAngle,
                _pullScreenFraction);
            if (_slingshotRopeVisual != null)
            {
                _slingshotRopeVisual.Initialize(_session, _player.transform);
            }
            _input = new PointerInput();
            _input.Pressed += OnPressed;
            _input.Moved += OnMoved;
            _input.Released += OnReleased;
            _input.Canceled += OnCanceled;
            foreach (var coin in _coins)
            {
                if (coin != null)
                {
                    coin.Collected += OnCoinCollected;
                }
            }
            _animation.Initialize(_session);
            _player.Initialize(_surfaces, _minimumX, _maximumX, skateboardGroundCollider);
            _player.ConfigureHandling(
                _progressionConfig.GetGroundTurnRate(_playerProgress.SkateUpgradeLevel),
                _progressionConfig.GetAirTurnRate(_playerProgress.SkateUpgradeLevel),
                _progressionConfig.GetMaximumSteeringAngle(_playerProgress.SkateUpgradeLevel));
            _player.Prepare();
            _hud.Refresh(_session, _metrics, GetBestProgress(), _bestDistanceMeters > 0);
        }

        private void Update()
        {
            if (!_hasFocus)
            {
                return;
            }
            if (_ui.BlocksGameplayInput)
            {
                CancelInput();
                return;
            }
            if (_session.Phase == RunPhase.Stopped)
            {
                CancelInput();
                return;
            }
            _input.Tick();
            if (_session.Phase == RunPhase.Running)
            {
                _player.SetSteering(_steering.SteeringInput, _steering.IsDragging);
                _metrics.Update(_player.BoardPosition.z, _player.Velocity);
                var reachedEnd = _targetDistance > 0 && _metrics.Distance >= _targetDistance;
                if (!_player.IsRunning || reachedEnd)
                {
                    FinishRun(reachedEnd);
                }
            }
        }

        private void LateUpdate()
        {
            if (_hud == null)
            {
                return;
            }
            if (_session.Phase == RunPhase.Running)
            {
                _metrics.Update(_player.BoardPosition.z, _player.Velocity);
            }
            if (_session.Phase != RunPhase.Stopped)
            {
                _hud.Refresh(_session, _metrics, GetBestProgress(), _bestDistanceMeters > 0);
            }
        }

        private void FinishRun(bool isVictory)
        {
            if (_session.Phase != RunPhase.Running)
            {
                return;
            }

            _session.Stop();
            _player.Freeze();
            CancelInput();
            _hud.Hide();
            if (isVictory)
            {
                _animation.PlayVictory();
            }
            else
            {
                _animation.HoldCurrentPose();
            }
            RaiseFinishFlag();

            var coinsEarned = RunRewardCalculator.CalculateCoins(
                _metrics.Distance,
                _metrics.CollectedCoinCount,
                _progressionConfig.GetCoinsPerKilometer(_playerProgress.IncomeUpgradeLevel),
                _progressionConfig.GetCoinsPerCollectedCoin(_playerProgress.IncomeUpgradeLevel));
            SessionWallet.Add(coinsEarned);
            _bestDistanceMeters = Mathf.Max(_bestDistanceMeters, _metrics.Distance);
            _playerProgress.CoinBalance = SessionWallet.Balance;
            _playerProgress.BestDistanceMeters = _bestDistanceMeters;
            _progressStorage.Save(_playerProgress);
            StartCoroutine(ShowRunResultAfterFlag(coinsEarned));
        }

        private IEnumerator ShowRunResultAfterFlag(int coinsEarned)
        {
            if (_finishFlag != null)
            {
                yield return new WaitForSecondsRealtime(_finishFlag.RaiseDuration);
            }

            _ui.Show<RunResultPopupController, RunResultPopupView, RunResultPopupArguments>(
                new RunResultPopupArguments(coinsEarned, RetryRun, ContinueToLobby));
        }

        private float GetBestProgress()
        {
            return _targetDistance > 0 ? Mathf.Clamp01(_bestDistanceMeters / _targetDistance) : 0;
        }

        private void RaiseFinishFlag()
        {
            if (_finishFlag == null)
            {
                return;
            }

            var trackCenterX = (_minimumX + _maximumX) * .5f;
            _finishFlag.RaiseAt(_player.Position, _finishFlagSideOffset, trackCenterX);
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

        private void OnCoinCollected()
        {
            _metrics.RegisterCoinCollected();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            _hasFocus = hasFocus;
            if (!hasFocus)
            {
                CancelInput();
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                CancelInput();
            }
        }

        private void OnDisable()
        {
            CancelInput();
            if (_player != null)
            {
                _player.Stop();
            }
        }

        private void OnPressed(Vector2 position)
        {
            if (_session.Phase == RunPhase.Ready) { _session.Begin(position); }
            else if (_session.Phase == RunPhase.Running) { _steering.Begin(position.x); }
            if (_session.Phase == RunPhase.Running)
            {
                _steeringJoystick.ShowAt(position);
            }
        }

        private void OnMoved(Vector2 position)
        {
            if (_session.Phase == RunPhase.Pulling)
            {
                _session.Move(position, Screen.width, Screen.height);
                _player.SetLaunchPose(_session.Power, _session.AimOffset, _maximumBackwardPullDistance, _maximumLateralPullDistance);
            }
            else if (_session.Phase == RunPhase.Running)
            {
                _steering.Move(position.x, _steeringJoystick.SteeringRadiusPixels);
                _steeringJoystick.Move(position);
            }
        }

        private void OnReleased()
        {
            if (_session.Release(out var velocity))
            {
                var launchPosition = _player.BoardPosition;
                if (_hudTargetDistance <= 0)
                {
                    _targetDistance = Mathf.Max(0, _trackEndZ - launchPosition.z);
                }
                for (var i = 0; i < _coins.Length; i++)
                {
                    if (_coins[i] != null && _targetDistance > 0)
                    {
                        _coinProgressPositions[i] = Mathf.Clamp01((_coins[i].transform.position.z - launchPosition.z) / _targetDistance);
                    }
                }
                _metrics.Begin(launchPosition.z, _targetDistance);
                _player.Launch(velocity);
            }
            else if (_session.Phase == RunPhase.Ready)
            {
                _player.SetLaunchPose(0, 0, _maximumBackwardPullDistance, _maximumLateralPullDistance);
            }
            _steering.End();
            _steeringJoystick.Hide();
        }

        private void OnCanceled()
        {
            var wasPulling = _session.Phase == RunPhase.Pulling;
            _session.Cancel();
            if (wasPulling && _player != null)
            {
                _player.SetLaunchPose(0, 0, _maximumBackwardPullDistance, _maximumLateralPullDistance);
            }
            if (_player != null)
            {
                _steering.End();
                _player.SetSteering(0, false);
            }
            _steeringJoystick?.Hide();
        }

        private void OnDestroy()
        {
            _ui?.Dispose();
            _steeringJoystick?.Dispose();
            if (_input != null)
            {
                _input.Pressed -= OnPressed;
                _input.Moved -= OnMoved;
                _input.Released -= OnReleased;
                _input.Canceled -= OnCanceled;
            }
            foreach (var coin in _coins)
            {
                if (coin != null)
                {
                    coin.Collected -= OnCoinCollected;
                }
            }
        }

        private void CancelInput()
        {
            _steeringJoystick?.Hide();
            if (_input == null || _player == null)
            {
                return;
            }
            _input.Cancel();
            _steering.End();
            _player.SetSteering(0, false);
        }
    }
}
