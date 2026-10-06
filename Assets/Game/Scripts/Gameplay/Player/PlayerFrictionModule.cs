using System.Collections.Generic;
using UnityEngine;

namespace SledSurfers.Gameplay.Player
{
    public sealed class PlayerFrictionModule : MonoBehaviour
    {
        [SerializeField, Min(0)] private float _groundResistance = 2f;
        [SerializeField, Min(1)] private float _slopeGravityMultiplier = 2f;
        private readonly Dictionary<Collider, float> _slowdownZones = new Dictionary<Collider, float>();

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

        public void ClearSlowdownZones()
        {
            _slowdownZones.Clear();
        }

        public void ApplyGroundForces(Rigidbody body, Vector3 surfaceNormal, Vector3 velocity)
        {
            if (body == null)
            {
                return;
            }

            var tangentVelocity = Vector3.ProjectOnPlane(velocity, surfaceNormal);
            var additionalResistance = 0f;
            foreach (var zone in _slowdownZones)
            {
                if (zone.Key != null && zone.Key.enabled && zone.Key.gameObject.activeInHierarchy)
                {
                    additionalResistance = Mathf.Max(additionalResistance, zone.Value);
                }
            }

            var resistance = Mathf.Min(_groundResistance + additionalResistance, tangentVelocity.magnitude / Time.fixedDeltaTime);
            var slopeGravity = Vector3.ProjectOnPlane(Physics.gravity, surfaceNormal) * (_slopeGravityMultiplier - 1);
            // Rigidbody gravity already contributes once; add only the extra slope component.
            body.AddForce(slopeGravity - tangentVelocity.normalized * resistance, ForceMode.Acceleration);
        }
    }
}
