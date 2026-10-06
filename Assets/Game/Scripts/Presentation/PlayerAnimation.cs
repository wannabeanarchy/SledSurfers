using SledSurfers.Gameplay.Player;
using UnityEngine;
using SledSurfers.Gameplay.Launch;

namespace SledSurfers.Presentation
{
    public sealed class PlayerAnimation : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private PlayerMotor _motor;
        [SerializeField] private Transform _skateboard;
        [SerializeField, Min(0)] private float _slopeAlignmentSpeed = 12;
        [SerializeField, Min(0)] private float _movingSkateboardClearance = .5f;
        private Quaternion _initialRotation;
        private Transform _skateboardVisual;
        private Vector3 _initialSkateboardVisualPosition;
        private bool _runEnded;
        private static readonly int Running = Animator.StringToHash("IsRunning");
        private static readonly int Grounded = Animator.StringToHash("IsGrounded");

        private LaunchSession _session;
        private bool _wasRunning;
        private static readonly int Pulling = Animator.StringToHash("IsPulling");
        private static readonly int Launch = Animator.StringToHash("Launch");
        private static readonly int Victory = Animator.StringToHash("IsVictory");

        public BoxCollider SkateboardGroundCollider
        {
            get
            {
                var contact = _skateboard != null ? _skateboard.Find("BoardContactCollider") : null;
                return contact != null ? contact.GetComponent<BoxCollider>() : null;
            }
        }

        private void Awake()
        {
            _initialRotation = transform.localRotation;
            _skateboardVisual = _skateboard != null ? _skateboard.Find("SkateBoard_V1") : null;
            if (_skateboardVisual != null)
            {
                _initialSkateboardVisualPosition = _skateboardVisual.localPosition;
            }
        }

        public void Initialize(LaunchSession session) { _session = session; }

        public void ResetRun()
        {
            _runEnded = false;
            _animator.speed = 1;
            _wasRunning = false;
            transform.localRotation = _initialRotation;
            _animator.Rebind();
            _animator.ResetTrigger(Launch);
            _animator.SetBool(Running, false);
            _animator.SetBool(Pulling, false);
            _animator.SetBool(Grounded, false);
            _animator.SetBool(Victory, false);
            _animator.Update(0);
        }

        public void PlayVictory()
        {
            _runEnded = true;
            _animator.speed = 1;
            _animator.ResetTrigger(Launch);
            _animator.SetBool(Pulling, false);
            _animator.SetBool(Victory, true);
        }

        public void HoldCurrentPose()
        {
            _runEnded = true;
            _animator.SetBool(Running, false);
            _animator.SetBool(Pulling, false);
            _animator.speed = 0;
        }

        private void LateUpdate()
        {
            var normal = transform.parent != null ? transform.parent.InverseTransformDirection(_motor.GroundNormal) : _motor.GroundNormal;
            var target = _initialRotation;
            if (_runEnded)
            {
                target = transform.localRotation;
            }
            else if (_motor.IsRunning)
            {
                var slopeAlignment = _motor.IsGrounded ? Quaternion.FromToRotation(Vector3.up, normal) : Quaternion.identity;
                var heading = Quaternion.AngleAxis(_motor.HeadingAngle, normal);
                target = heading * slopeAlignment * _initialRotation;
            }
            transform.localRotation = Quaternion.Slerp(transform.localRotation, target, 1 - Mathf.Exp(-_slopeAlignmentSpeed * Time.deltaTime));
            if (_skateboardVisual != null)
            {
                _skateboardVisual.rotation = transform.rotation;
                var offset = (_motor.IsRunning || _runEnded)
                    ? _skateboardVisual.parent.InverseTransformVector(Vector3.up * _movingSkateboardClearance)
                    : Vector3.zero;
                _skateboardVisual.localPosition = _initialSkateboardVisualPosition + offset;
            }
        }

        private void Update()
        {
            if (_session == null || _runEnded) { return; }
            _animator.SetBool(Pulling, _session.Phase == RunPhase.Pulling);
            if (_motor.IsRunning && !_wasRunning) { _animator.SetTrigger(Launch); }
            _wasRunning = _motor.IsRunning;
            _animator.SetBool(Running, _motor.IsRunning);
            _animator.SetBool(Grounded, _motor.IsGrounded);
        }
    }
}
