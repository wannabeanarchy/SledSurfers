using System.Collections.Generic;
using UnityEngine;

namespace SledSurfers.Gameplay.Player
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        private float _groundTurnRate = 90;
        private float _airTurnRate = 30;
        private float _maximumSteeringAngle = 35;
        [SerializeField, Min(0)] private float _groundResistance = 2f;
        [SerializeField, Min(1)] private float _slopeGravityMultiplier = 2;
        [SerializeField, Min(.01f)] private float _groundProbeDistance = .2f;
        [SerializeField, Min(0)] private float _stopSpeed = .3f;
        private readonly Dictionary<Collider, float> _slowdownZones = new Dictionary<Collider, float>();
        private Rigidbody _body;
        private Collider[] _surfaces;
        private float _minimumX;
        private float _maximumX;
        private float _steeringInput;
        private float _headingAngle;
        private Vector3 _launchOrigin;
        private bool _hasGroundContact;
        private Vector3 _contactNormal;
        public bool IsRunning { get; private set; }
        public bool IsGrounded { get; private set; }
        public float HeadingAngle => _headingAngle;
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

        public void ConfigureHandling(float groundTurnRate, float airTurnRate, float maximumSteeringAngle)
        {
            _groundTurnRate = Mathf.Max(0, groundTurnRate);
            _airTurnRate = Mathf.Max(0, airTurnRate);
            _maximumSteeringAngle = Mathf.Clamp(maximumSteeringAngle, 0, 80);
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
            _steeringInput = 0;
            _headingAngle = 0;
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
            _steeringInput = 0;
            _headingAngle = Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg;
            _body.isKinematic = false;
            _body.velocity = velocity;
            IsRunning = true;
        }

        public void SetSteering(float steeringInput, bool isSteering)
        {
            _steeringInput = isSteering ? Mathf.Clamp(steeringInput, -1, 1) : 0;
        }

        public void Stop()
        {
            _slowdownZones.Clear();
            _hasGroundContact = false;
            IsRunning = false;
            _steeringInput = 0;
            if (_body != null && !_body.isKinematic)
            {
                _body.velocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
            }
        }

        public void Freeze()
        {
            Stop();
            if (_body != null)
            {
                _body.isKinematic = true;
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
            Freeze();
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
            if (Mathf.Abs(_steeringInput) > 0.001f && horizontalSpeed > 0.01f)
            {
                var turnRate = IsGrounded ? _groundTurnRate : _airTurnRate;
                _headingAngle = Mathf.Clamp(
                    _headingAngle + _steeringInput * turnRate * Time.fixedDeltaTime,
                    -_maximumSteeringAngle,
                    _maximumSteeringAngle);
            }

            if (horizontalSpeed > 0.01f)
            {
                var forward = Quaternion.Euler(0, _headingAngle, 0) * Vector3.forward;
                if (IsGrounded)
                {
                    var surfaceSpeed = Vector3.ProjectOnPlane(velocity, hit.normal).magnitude;
                    var normalSpeed = Vector3.Dot(velocity, hit.normal);
                    var direction = Vector3.ProjectOnPlane(forward, hit.normal).normalized;
                    velocity = direction * surfaceSpeed + hit.normal * normalSpeed;
                }
                else
                {
                    velocity.x = forward.x * horizontalSpeed;
                    velocity.z = forward.z * horizontalSpeed;
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
