using SledSurfers.Gameplay.Player;
using SledSurfers.Input;
using System;
using SledSurfers.UI;
using SledSurfers.UI.Retry;
using SledSurfers.UI.Hud;
using SledSurfers.Gameplay.Run;
using UnityEngine;
using SledSurfers.Gameplay.Launch;
using SledSurfers.Presentation;

namespace SledSurfers.Composition
{
    public sealed class GameplayCompositionRoot : MonoBehaviour
    {
        [SerializeField] private PlayerMotor _player;
        [SerializeField] private Collider[] _surfaces;
        [SerializeField, Min(0)] private float _maximumLaunchSpeed = 30;
        [SerializeField, Range(0, 30)] private float _maximumLaunchAngle = 12;
        [SerializeField, Range(.05f, .8f)] private float _pullScreenFraction = .25f;
        [SerializeField] private PlayerAnimation _animation;
        [SerializeField, Min(0)] private float _maximumPullbackDistance = 2;
        [SerializeField] private float _minimumX = -18;
        [SerializeField] private float _maximumX = 18;
        [SerializeField, Min(.01f)] private float _dragRange = 40;
        [SerializeField] private UIConfig _uiConfig;
        [SerializeField] private Transform _windowRoot;
        [SerializeField] private Transform _popupRoot;
        [SerializeField] private Transform _widgetRoot;
        [SerializeField] private CameraFollow _camera;
        [SerializeField, Min(0), Tooltip("Distance goal in meters. Zero uses the end of the active road colliders.")]
        private float _hudTargetDistance;
        private float _trackEndZ;
        private RunMetrics _metrics;
        private GameplayHud _hud;
        private UIManager _ui;
        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private DragSteering _steering;
        private PointerInput _input;
        private LaunchSession _session;
        private bool _hasFocus = true;

        private void Start()
        {
            if (_player == null || _surfaces == null || _surfaces.Length == 0 || _animation == null || _minimumX >= _maximumX || _uiConfig == null || _windowRoot == null || _popupRoot == null || _widgetRoot == null || _camera == null)
            {
                Debug.LogError("Gameplay references or track limits are invalid.", this);
                enabled = false;
                return;
            }
            var factory = new UIFactory(_uiConfig, _windowRoot, _popupRoot, _widgetRoot);
            factory.Register<RetryWidgetController, RetryWidgetView, Action>(view => new RetryWidgetController(view));
            GameplayHud.Register(factory);
            _ui = new UIManager(factory);
            _hud = new GameplayHud(_ui);
            _metrics = new RunMetrics();
            _trackEndZ = _player.Position.z;
            foreach (var surface in _surfaces)
            {
                if (surface != null && surface.enabled && surface.gameObject.activeInHierarchy)
                {
                    _trackEndZ = Mathf.Max(_trackEndZ, surface.bounds.max.z);
                }
            }
            _startPosition = _player.transform.position;
            _startRotation = _player.transform.rotation;
            _steering = new DragSteering();
            _session = new LaunchSession(_maximumLaunchSpeed, _maximumLaunchAngle, _pullScreenFraction);
            _input = new PointerInput();
            _input.Pressed += OnPressed;
            _input.Moved += OnMoved;
            _input.Released += OnReleased;
            _input.Canceled += OnCanceled;
            _animation.Initialize(_session);
            _player.Initialize(_surfaces, _minimumX, _maximumX);
            _player.Prepare();
            _hud.Refresh(_session, _metrics);
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
            _input.Tick();
            if (_session.Phase == RunPhase.Running)
            {
                _player.SetSteering(_steering.TargetX, _steering.IsDragging);
                if (!_player.IsRunning)
                {
                    _session.Stop();
                    CancelInput();
                    _ui.Show<RetryWidgetController, RetryWidgetView, Action>(Retry);
                }
            }
        }

        private void LateUpdate()
        {
            if (_hud == null)
            {
                return;
            }
            if (_session.Phase == RunPhase.Running || _session.Phase == RunPhase.Stopped)
            {
                _metrics.Update(_player.Position.z, _player.Velocity);
            }
            _hud.Refresh(_session, _metrics);
        }

        private void Retry()
        {
            if (_session.Phase != RunPhase.Stopped)
            {
                return;
            }
            CancelInput();
            _steering.End(_startPosition.x);
            _session.Reset();
            _metrics.Reset();
            _player.ResetRun(_startPosition, _startRotation);
            _camera.ResetRun();
            _animation.ResetRun();
            _ui.Hide<RetryWidgetController>();
            _hud.Refresh(_session, _metrics);
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
            else if (_session.Phase == RunPhase.Running) { _steering.Begin(position.x, _player.transform.position.x); }
        }

        private void OnMoved(Vector2 position)
        {
            if (_session.Phase == RunPhase.Pulling)
            {
                _session.Move(position, Screen.height);
                _player.SetPullback(_session.Power * _maximumPullbackDistance);
            }
            else if (_session.Phase == RunPhase.Running) { _steering.Move(position.x, Screen.width, _dragRange, _minimumX, _maximumX); }
        }

        private void OnReleased()
        {
            if (_session.Release(out var velocity))
            {
                var targetDistance = _hudTargetDistance > 0 ? _hudTargetDistance : _trackEndZ - _player.Position.z;
                _metrics.Begin(_player.Position.z, targetDistance);
                _player.Launch(velocity);
            }
            else if (_session.Phase == RunPhase.Ready)
            {
                _player.SetPullback(0);
            }
            _steering.End(_player.transform.position.x);
        }

        private void OnCanceled()
        {
            var wasPulling = _session.Phase == RunPhase.Pulling;
            _session.Cancel();
            if (wasPulling && _player != null)
            {
                _player.SetPullback(0);
            }
            if (_player != null) { _steering.End(_player.transform.position.x); }
        }

        private void OnDestroy()
        {
            _ui?.Dispose();
            if (_input == null) { return; }
            _input.Pressed -= OnPressed;
            _input.Moved -= OnMoved;
            _input.Released -= OnReleased;
            _input.Canceled -= OnCanceled;
        }

        private void CancelInput()
        {
            if (_input == null || _player == null)
            {
                return;
            }
            _input.Cancel();
            _player.SetSteering(_player.transform.position.x, false);
        }
    }
}
