using System.Collections.Generic;
using UnityEngine;

namespace SledSurfers.Gameplay.Player
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField, Min(0)] private float _steeringResponse = 6;
        [SerializeField, Min(0)] private float _lateralSpeed = 12;
        [SerializeField, Range(0, 80)] private float _maximumSteeringAngle = 35;
        [SerializeField, Min(0)] private float _groundAcceleration = 60;
        [SerializeField, Min(0)] private float _airAcceleration = 20;
        [SerializeField, Min(0)] private float _groundResistance = 2f;
        [SerializeField, Min(1)] private float _slopeGravityMultiplier = 2;
        [SerializeField, Min(.01f)] private float _groundProbeDistance = .2f;
        [SerializeField, Min(0)] private float _stopSpeed = .3f;
        private readonly Dictionary<Collider, float> _slowdownZones = new Dictionary<Collider, float>();
        private Rigidbody _body;
        private Collider[] _surfaces;
        private float _minimumX;
        private float _maximumX;
        private float _targetX;
        private bool _isSteering;
        private bool _hasSteered;
        private Vector3 _launchOrigin;
        private bool _hasGroundContact;
        private Vector3 _contactNormal;
        public bool IsRunning { get; private set; }
        public bool IsGrounded { get; private set; }
        public Vector3 GroundNormal { get; private set; } = Vector3.up;
        public Vector3 Position => _body != null ? _body.position : transform.position;
        public Vector3 Velocity => _body != null ? _body.velocity : Vector3.zero;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
        }

        public void Initialize(Collider[] surfaces, float minimumX, float maximumX)
        {
            _surfaces = surfaces;
            _minimumX = minimumX;
            _maximumX = maximumX;
        }

        public void Prepare()
        {
            Stop();
            _body.isKinematic = true;
            _launchOrigin = _body.position;
        }

        public void ResetRun(Vector3 position, Quaternion rotation)
        {
            Stop();
            _body.isKinematic = true;
            _body.position = position;
            _body.rotation = rotation;
            transform.SetPositionAndRotation(position, rotation);
            _launchOrigin = position;
            _targetX = position.x;
            _hasSteered = false;
            IsGrounded = false;
            GroundNormal = Vector3.up;
        }

        public void SetPullback(float distance)
        {
            if (_body.isKinematic && !IsRunning)
            {
                _body.position = _launchOrigin - Vector3.forward * Mathf.Max(0, distance);
            }
        }

        public void Launch(Vector3 velocity)
        {
            _slowdownZones.Clear();
            _hasGroundContact = false;
            _hasSteered = false;
            _targetX = _body.position.x;
            _body.isKinematic = false;
            _body.velocity = velocity;
            IsRunning = true;
        }

        public void SetSteering(float targetX, bool isSteering)
        {
            _targetX = Mathf.Clamp(targetX, _minimumX, _maximumX);
            _isSteering = isSteering;
            _hasSteered |= isSteering;
        }

        public void Stop()
        {
            _slowdownZones.Clear();
            _hasGroundContact = false;
            IsRunning = false;
            _isSteering = false;
            if (_body != null && !_body.isKinematic)
            {
                _body.velocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
            }
        }

        public void EnterSlowdownZone(Collider zone, float additionalResistance)
        {
            if (zone != null)
            {
                _slowdownZones[zone] = Mathf.Max(0, additionalResistance);
            }
        }

        public void ExitSlowdownZone(Collider zone)
        {
            if (zone != null)
            {
                _slowdownZones.Remove(zone);
            }
        }

        public void Crash()
        {
            if (!IsRunning)
            {
                return;
            }
            Stop();
            _body.isKinematic = true;
        }

        private void OnCollisionEnter(Collision collision)
        {
            ResolveSurfaceContact(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            ResolveSurfaceContact(collision);
        }

        private void ResolveSurfaceContact(Collision collision)
        {
            if (!IsRunning || _surfaces == null)
            {
                return;
            }
            var isSurface = false;
            foreach (var surface in _surfaces)
            {
                if (surface == collision.collider)
                {
                    isSurface = true;
                    break;
                }
            }
            if (!isSurface)
            {
                return;
            }
            for (var i = 0; i < collision.contactCount; i++)
            {
                var normal = collision.GetContact(i).normal;
                if (normal.y < .5f)
                {
                    continue;
                }
                _contactNormal = normal;
                _hasGroundContact = true;
                var velocity = _body.velocity;
                var separatingSpeed = Vector3.Dot(velocity, normal);
                if (separatingSpeed > 0)
                {
                    // Remove collision-induced separation, retaining motion along the slope.
                    _body.velocity = velocity - normal * separatingSpeed;
                }
            }
        }

        private void FixedUpdate()
        {
            if (!IsRunning || _surfaces == null)
            {
                return;
            }
            var ray = new Ray(_body.position + Vector3.up * .3f, Vector3.down);
            var hasSurface = false;
            var hit = new RaycastHit();
            var closestDistance = .3f + _groundProbeDistance;
            foreach (var surface in _surfaces)
            {
                if (surface != null && surface.enabled && surface.gameObject.activeInHierarchy && surface.Raycast(ray, out var candidate, closestDistance))
                {
                    hasSurface = true;
                    hit = candidate;
                    closestDistance = candidate.distance;
                }
            }
            if (_hasGroundContact)
            {
                hasSurface = true;
                hit.normal = _contactNormal;
            }
            _hasGroundContact = false;
            var velocity = _body.velocity;
            // Ascending a slope is grounded motion; only velocity away from the surface indicates takeoff.
            IsGrounded = hasSurface && Vector3.Dot(velocity, hit.normal) <= .5f;
            GroundNormal = IsGrounded ? hit.normal : Vector3.up;
            var horizontalSpeed = new Vector2(velocity.x, velocity.z).magnitude;
            var lateralLimit = Mathf.Min(_lateralSpeed, horizontalSpeed * Mathf.Sin(_maximumSteeringAngle * Mathf.Deg2Rad));
            var desiredLateralSpeed = _isSteering ? Mathf.Clamp((_targetX - _body.position.x) * _steeringResponse, -lateralLimit, lateralLimit) : 0;
            if (_isSteering || _hasSteered)
            {
                var surfaceSpeed = IsGrounded ? Vector3.ProjectOnPlane(velocity, hit.normal).magnitude : 0;
                var normalSpeed = IsGrounded ? Vector3.Dot(velocity, hit.normal) : 0;
                velocity.x = Mathf.MoveTowards(velocity.x, desiredLateralSpeed, (IsGrounded ? _groundAcceleration : _airAcceleration) * Time.fixedDeltaTime);
                velocity.x = Mathf.Clamp(velocity.x, -horizontalSpeed, horizontalSpeed);
                // Steering redirects horizontal momentum without adding speed.
                velocity.z = Mathf.Sqrt(Mathf.Max(0, horizontalSpeed * horizontalSpeed - velocity.x * velocity.x)) * Mathf.Sign(velocity.z);
                if (IsGrounded)
                {
                    var direction = Vector3.ProjectOnPlane(velocity, hit.normal).normalized;
                    velocity = direction * surfaceSpeed + hit.normal * normalSpeed;
                }
            }
            var nextX = _body.position.x + velocity.x * Time.fixedDeltaTime;
            if (nextX < _minimumX || nextX > _maximumX)
            {
                velocity.x = (Mathf.Clamp(nextX, _minimumX, _maximumX) - _body.position.x) / Time.fixedDeltaTime;
            }
            _body.velocity = velocity;
            if (IsGrounded)
            {
                if (velocity.z <= _stopSpeed)
                {
                    Stop();
                    _body.isKinematic = true;
                    return;
                }
                var tangentVelocity = Vector3.ProjectOnPlane(velocity, hit.normal);
                var additionalResistance = 0f;
                foreach (var zone in _slowdownZones)
                {
                    if (zone.Key != null && zone.Key.enabled && zone.Key.gameObject.activeInHierarchy)
                    {
                        additionalResistance = Mathf.Max(additionalResistance, zone.Value);
                    }
                }
                var resistance = Mathf.Min(_groundResistance + additionalResistance, tangentVelocity.magnitude / Time.fixedDeltaTime);
                // Rigidbody gravity already contributes once; amplify only its component along the slope.
                var slopeGravity = Vector3.ProjectOnPlane(Physics.gravity, hit.normal) * (_slopeGravityMultiplier - 1);
                _body.AddForce(slopeGravity - tangentVelocity.normalized * resistance, ForceMode.Acceleration);
            }
        }
    }
}
