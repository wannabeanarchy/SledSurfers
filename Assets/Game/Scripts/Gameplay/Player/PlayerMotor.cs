using UnityEngine;

namespace SledSurfers.Gameplay.Player
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(PlayerFrictionModule))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        private float _groundTurnRate = 90;
        private float _airTurnRate = 30;
        private float _maximumSteeringAngle = 35;
        [SerializeField, Min(0)] private float _groundSnapDistance = .35f;
        [SerializeField, Min(0)] private float _groundProbeStartOffset = .04f;
        [SerializeField, Min(1)] private float _airGravityMultiplier = 1.8f;
        [SerializeField, Min(0)] private float _maximumUpwardSpeed = 5f;
        [SerializeField, Min(0)] private float _stopSpeed = .5f;
        private Rigidbody _body;
        private PlayerFrictionModule _friction;
        private CapsuleCollider _characterCollider;
        private BoxCollider _boardCollider;
        private Collider[] _surfaces;
        private float _minimumX;
        private float _maximumX;
        private float _steeringInput;
        private float _headingAngle;
        private Vector3 _launchOrigin;
        private Quaternion _launchOriginRotation = Quaternion.identity;
        private bool _hasGroundContact;
        private Vector3 _contactNormal;
        public bool IsRunning { get; private set; }
        public bool IsGrounded { get; private set; }
        public float HeadingAngle => _headingAngle;
        public Vector3 GroundNormal { get; private set; } = Vector3.up;
        public Vector3 Position => _body != null ? _body.position : transform.position;
        public Vector3 BoardPosition => _boardCollider != null ? _boardCollider.bounds.center : Position;
        public BoxCollider BoardGroundCollider => _boardCollider;
        public Vector3 Velocity => _body != null ? _body.velocity : Vector3.zero;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _friction = GetComponent<PlayerFrictionModule>();
            _characterCollider = GetComponent<CapsuleCollider>();
        }

        public void Initialize(Collider[] surfaces, float minimumX, float maximumX, BoxCollider boardCollider)
        {
            if (boardCollider == null || !boardCollider.enabled || boardCollider.attachedRigidbody != _body)
            {
                Debug.LogError("The skateboard ground collider must be enabled and attached to the player's Rigidbody.", this);
                return;
            }

            _surfaces = surfaces;
            _minimumX = minimumX;
            _maximumX = maximumX;
            _boardCollider = boardCollider;
            foreach (var surface in _surfaces)
            {
                if (surface != null && _characterCollider != null)
                {
                    Physics.IgnoreCollision(_characterCollider, surface, true);
                }
            }
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
            _launchOriginRotation = _body.rotation;
        }

        public void ResetRun(Vector3 position, Quaternion rotation)
        {
            Stop();
            _body.isKinematic = true;
            _body.position = position;
            _body.rotation = rotation;
            transform.SetPositionAndRotation(position, rotation);
            _launchOrigin = position;
            _launchOriginRotation = rotation;
            _steeringInput = 0;
            _headingAngle = 0;
            IsGrounded = false;
            GroundNormal = Vector3.up;
        }

        public void SetLaunchPose(float power, float aimOffset, float maximumBackwardDistance, float maximumLateralDistance)
        {
            if (_body.isKinematic && !IsRunning)
            {
                var localOffset = new Vector3(
                    Mathf.Clamp(aimOffset, -1, 1) * Mathf.Max(0, maximumLateralDistance),
                    0,
                    -Mathf.Clamp01(power) * Mathf.Max(0, maximumBackwardDistance));
                _body.position = _launchOrigin + _launchOriginRotation * localOffset;
            }
        }

        public void Launch(Vector3 velocity)
        {
            _friction.ClearSlowdownZones();
            _hasGroundContact = false;
            _steeringInput = 0;
            _headingAngle = Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg;
            IsGrounded = TryGetBoardGround(out var surfaceNormal, out _);
            GroundNormal = IsGrounded ? surfaceNormal : Vector3.up;
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
            _friction.ClearSlowdownZones();
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
            if (!IsTrackSurface(collision.collider))
            {
                return;
            }
            var normalSum = Vector3.zero;
            for (var i = 0; i < collision.contactCount; i++)
            {
                var normal = collision.GetContact(i).normal;
                if (normal.y < .5f)
                {
                    continue;
                }
                normalSum += normal;
            }
            if (normalSum.sqrMagnitude > 0)
            {
                _contactNormal = normalSum.normalized;
                _hasGroundContact = true;
                var velocity = _body.velocity;
                var normalSpeed = Vector3.Dot(velocity, _contactNormal);
                _body.velocity = velocity - _contactNormal * normalSpeed;
            }
        }

        private bool IsTrackSurface(Collider candidate)
        {
            foreach (var surface in _surfaces)
            {
                if (surface == candidate)
                {
                    return true;
                }
            }
            return false;
        }

        private bool TryGetBoardGround(out Vector3 normal, out float snapDistance)
        {
            normal = Vector3.zero;
            snapDistance = 0;
            if (_boardCollider == null || !_boardCollider.enabled)
            {
                return false;
            }

            var contactTransform = _boardCollider.transform;
            var halfLength = _boardCollider.size.z * .35f;
            var halfHeight = _boardCollider.size.y * .5f;
            var castDistance = _groundProbeStartOffset + _groundSnapDistance;
            var closestGap = float.MaxValue;
            var hitCount = 0;
            for (var probeIndex = -1; probeIndex <= 1; probeIndex++)
            {
                var localPoint = _boardCollider.center
                    + Vector3.forward * (halfLength * probeIndex)
                    + Vector3.up * (_groundProbeStartOffset - halfHeight);
                var ray = new Ray(contactTransform.TransformPoint(localPoint), Vector3.down);
                var hasClosestHit = false;
                var closestHit = new RaycastHit();
                var closestHitDistance = castDistance;
                foreach (var surface in _surfaces)
                {
                    if (surface == null || !surface.enabled || !surface.gameObject.activeInHierarchy
                        || !surface.Raycast(ray, out var candidate, castDistance)
                        || candidate.normal.y < .5f || candidate.distance >= closestHitDistance)
                    {
                        continue;
                    }

                    hasClosestHit = true;
                    closestHit = candidate;
                    closestHitDistance = candidate.distance;
                }

                if (hasClosestHit)
                {
                    normal += closestHit.normal;
                    closestGap = Mathf.Min(closestGap, Mathf.Max(0, closestHit.distance - _groundProbeStartOffset));
                    hitCount++;
                }
            }

            if (hitCount == 0)
            {
                return false;
            }

            normal.Normalize();
            snapDistance = closestGap;
            return true;
        }

        private void FixedUpdate()
        {
            if (!IsRunning || _surfaces == null)
            {
                return;
            }
            var hasSurface = _hasGroundContact;
            var surfaceNormal = hasSurface ? _contactNormal : Vector3.up;
            _hasGroundContact = false;
            if (!hasSurface && TryGetBoardGround(out var probeNormal, out var snapDistance))
            {
                hasSurface = true;
                surfaceNormal = probeNormal;
                if (snapDistance > .001f)
                {
                    _body.position -= Vector3.up * snapDistance;
                }
            }
            var velocity = _body.velocity;
            IsGrounded = hasSurface;
            GroundNormal = IsGrounded ? surfaceNormal : Vector3.up;
            var normalSpeed = IsGrounded ? Vector3.Dot(velocity, surfaceNormal) : 0;
            if (IsGrounded && Mathf.Abs(normalSpeed) > 0.001f)
            {
                velocity -= surfaceNormal * normalSpeed;
            }
            else if (!IsGrounded)
            {
                velocity.y = Mathf.Min(velocity.y, _maximumUpwardSpeed);
            }
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
                    var surfaceSpeed = Vector3.ProjectOnPlane(velocity, surfaceNormal).magnitude;
                    var slopeNormalSpeed = Vector3.Dot(velocity, surfaceNormal);
                    var direction = Vector3.ProjectOnPlane(forward, surfaceNormal).normalized;
                    velocity = direction * surfaceSpeed + surfaceNormal * slopeNormalSpeed;
                }
                else
                {
                    velocity.x = forward.x * horizontalSpeed;
                    velocity.z = forward.z * horizontalSpeed;
                }
            }
            _body.velocity = velocity;
            if (!IsGrounded)
            {
                _body.AddForce(Physics.gravity * (_airGravityMultiplier - 1), ForceMode.Acceleration);
            }
            else if (velocity.z > _stopSpeed)
            {
                _friction.ApplyGroundForces(_body, surfaceNormal, velocity);
            }

            ConstrainToTrackBounds();
            if (IsGrounded && velocity.magnitude <= _stopSpeed)
            {
                Stop();
            }
        }

        private void ConstrainToTrackBounds()
        {
            var halfWidth = _boardCollider != null ? _boardCollider.bounds.extents.x : 0;
            var minimumX = _minimumX + halfWidth;
            var maximumX = _maximumX - halfWidth;
            if (minimumX > maximumX)
            {
                minimumX = maximumX = (_minimumX + _maximumX) * .5f;
            }

            var position = _body.position;
            var velocity = _body.velocity;
            var nextX = position.x + velocity.x * Time.fixedDeltaTime;
            var constrainedX = Mathf.Clamp(nextX, minimumX, maximumX);
            if (Mathf.Abs(constrainedX - nextX) > .0001f || position.x < minimumX || position.x > maximumX)
            {
                position.x = constrainedX;
                _body.position = position;
                velocity.x = 0;
                _body.velocity = velocity;
            }
        }
    }
}
