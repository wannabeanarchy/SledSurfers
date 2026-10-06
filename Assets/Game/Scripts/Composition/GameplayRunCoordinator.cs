using System;
using System.Collections;
using SledSurfers.Gameplay.Collectibles;
using SledSurfers.Gameplay.Launch;
using SledSurfers.Gameplay.Player;
using SledSurfers.Gameplay.Run;
using SledSurfers.Input;
using SledSurfers.Persistence;
using SledSurfers.Presentation;
using SledSurfers.Progression;
using SledSurfers.UI;
using SledSurfers.UI.Hud;
using SledSurfers.UI.Results;
using UnityEngine;

namespace SledSurfers.Composition
{
    public sealed class GameplayRunCoordinator : IDisposable
    {
        public sealed class Dependencies
        {
            public PlayerMotor Player { get; internal set; }
            public PlayerAnimation Animation { get; internal set; }
            public LaunchSession Session { get; internal set; }
            public UIManager UI { get; internal set; }
            public ProgressionConfig ProgressionConfig { get; internal set; }
            public PlayerProgressData PlayerProgress { get; internal set; }
            public PlayerProgressStorage ProgressStorage { get; internal set; }
            public FinishFlagVisual FinishFlag { get; internal set; }
            public CoinPickup[] Coins { get; internal set; }
            public Transform WidgetRoot { get; internal set; }
            public float InitialStartZ { get; internal set; }
            public float TrackEndZ { get; internal set; }
            public float TargetDistance { get; internal set; }
            public float HUDTargetDistance { get; internal set; }
            public float MinimumX { get; internal set; }
            public float MaximumX { get; internal set; }
            public float MaximumLateralPullDistance { get; internal set; }
            public float MaximumBackwardPullDistance { get; internal set; }
            public float SteeringJoystickRadius { get; internal set; }
            public float FinishFlagSideOffset { get; internal set; }
            public Action<IEnumerator> StartCoroutine { get; internal set; }
            public Action RetryRequested { get; internal set; }
            public Action ContinueRequested { get; internal set; }
        }

        private readonly PlayerMotor _player;
        private readonly PlayerAnimation _animation;
        private readonly LaunchSession _session;
        private readonly UIManager _ui;
        private readonly ProgressionConfig _progressionConfig;
        private readonly PlayerProgressData _playerProgress;
        private readonly PlayerProgressStorage _progressStorage;
        private readonly FinishFlagVisual _finishFlag;
        private readonly CoinPickup[] _coins;
        private readonly float[] _coinProgressPositions;
        private readonly float _trackEndZ;
        private readonly float _hudTargetDistance;
        private readonly float _minimumX;
        private readonly float _maximumX;
        private readonly float _maximumLateralPullDistance;
        private readonly float _maximumBackwardPullDistance;
        private readonly float _finishFlagSideOffset;
        private readonly Action<IEnumerator> _startCoroutine;
        private readonly Action _retryRequested;
        private readonly Action _continueRequested;
        private readonly RunMetrics _metrics;
        private readonly GameplayHud _hud;
        private readonly DragSteering _steering;
        private readonly SteeringJoystickVisual _steeringJoystick;
        private readonly PointerInput _input;
        private float _targetDistance;
        private float _bestDistanceMeters;
        private bool _hasFocus = true;
        private bool _disposed;

        public GameplayRunCoordinator(Dependencies dependencies)
        {
            if (dependencies == null || dependencies.Player == null || dependencies.Animation == null ||
                dependencies.Session == null || dependencies.UI == null || dependencies.ProgressionConfig == null ||
                dependencies.PlayerProgress == null || dependencies.ProgressStorage == null ||
                dependencies.WidgetRoot == null || dependencies.StartCoroutine == null ||
                dependencies.RetryRequested == null || dependencies.ContinueRequested == null)
            {
                throw new ArgumentException("Gameplay run dependencies are incomplete.", nameof(dependencies));
            }

            _player = dependencies.Player;
            _animation = dependencies.Animation;
            _session = dependencies.Session;
            _ui = dependencies.UI;
            _progressionConfig = dependencies.ProgressionConfig;
            _playerProgress = dependencies.PlayerProgress;
            _progressStorage = dependencies.ProgressStorage;
            _finishFlag = dependencies.FinishFlag;
            _coins = dependencies.Coins ?? Array.Empty<CoinPickup>();
            _trackEndZ = dependencies.TrackEndZ;
            _targetDistance = dependencies.TargetDistance;
            _hudTargetDistance = dependencies.HUDTargetDistance;
            _minimumX = dependencies.MinimumX;
            _maximumX = dependencies.MaximumX;
            _maximumLateralPullDistance = dependencies.MaximumLateralPullDistance;
            _maximumBackwardPullDistance = dependencies.MaximumBackwardPullDistance;
            _finishFlagSideOffset = dependencies.FinishFlagSideOffset;
            _startCoroutine = dependencies.StartCoroutine;
            _retryRequested = dependencies.RetryRequested;
            _continueRequested = dependencies.ContinueRequested;
            _bestDistanceMeters = _playerProgress.BestDistanceMeters;
            _coinProgressPositions = new float[_coins.Length];
            for (var i = 0; i < _coins.Length; i++)
            {
                if (_coins[i] != null && _targetDistance > 0)
                {
                    _coinProgressPositions[i] = Mathf.Clamp01((_coins[i].transform.position.z - dependencies.InitialStartZ) / _targetDistance);
                }
            }

            _metrics = new RunMetrics();
            _hud = new GameplayHud(_ui, _coinProgressPositions);
            _steering = new DragSteering();
            _steeringJoystick = new SteeringJoystickVisual(dependencies.WidgetRoot, dependencies.SteeringJoystickRadius);
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

            _hud.Refresh(_session, _metrics, GetBestProgress(), _bestDistanceMeters > 0);
        }

        public void Tick()
        {
            if (_disposed || !_hasFocus)
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

        public void LateTick()
        {
            if (_disposed)
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

        public void SetApplicationFocus(bool hasFocus)
        {
            _hasFocus = hasFocus;
            if (!hasFocus)
            {
                CancelInput();
            }
        }

        public void CancelInput()
        {
            _steeringJoystick?.Hide();
            if (_input == null)
            {
                return;
            }
            _input.Cancel();
            _steering.End();
            _player.SetSteering(0, false);
        }

        public void OnHostDisabled()
        {
            CancelInput();
            _player.Stop();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            CancelInput();
            _ui.Dispose();
            _steeringJoystick.Dispose();
            _input.Pressed -= OnPressed;
            _input.Moved -= OnMoved;
            _input.Released -= OnReleased;
            _input.Canceled -= OnCanceled;
            foreach (var coin in _coins)
            {
                if (coin != null)
                {
                    coin.Collected -= OnCoinCollected;
                }
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
            _playerProgress.CoinBalance += coinsEarned;
            _bestDistanceMeters = Mathf.Max(_bestDistanceMeters, _metrics.Distance);
            _playerProgress.BestDistanceMeters = _bestDistanceMeters;
            _progressStorage.Save(_playerProgress);
            _startCoroutine(ShowRunResultAfterFlag(coinsEarned));
        }

        private IEnumerator ShowRunResultAfterFlag(int coinsEarned)
        {
            if (_finishFlag != null)
            {
                yield return new WaitForSecondsRealtime(_finishFlag.RaiseDuration);
            }

            _ui.Show<RunResultPopupController, RunResultPopupView, RunResultPopupArguments>(
                new RunResultPopupArguments(coinsEarned, _retryRequested, _continueRequested));
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

        private void OnCoinCollected()
        {
            _metrics.RegisterCoinCollected();
        }

        private void OnPressed(Vector2 position)
        {
            if (_session.Phase == RunPhase.Ready)
            {
                _session.Begin(position);
            }
            else if (_session.Phase == RunPhase.Running)
            {
                _steering.Begin(position.x);
            }

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
            if (wasPulling)
            {
                _player.SetLaunchPose(0, 0, _maximumBackwardPullDistance, _maximumLateralPullDistance);
            }

            _steering.End();
            _player.SetSteering(0, false);
            _steeringJoystick.Hide();
        }
    }
}
