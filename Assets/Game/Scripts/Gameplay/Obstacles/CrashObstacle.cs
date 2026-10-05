using SledSurfers.Gameplay.Effects;
using SledSurfers.Gameplay.Player;
using UnityEngine;

namespace SledSurfers.Gameplay.Obstacles
{
    public sealed class CrashObstacle : MonoBehaviour
    {
        [SerializeField] private GameObject _impactEffect;

        private void OnCollisionEnter(Collision collision)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            var body = collision.rigidbody;
            if (body != null && body.TryGetComponent<PlayerMotor>(out var player))
            {
                if (!player.IsRunning)
                {
                    return;
                }

                var impactPosition = collision.contactCount > 0
                    ? collision.GetContact(0).point
                    : transform.position;
                ParticleEffectSpawner.Play(_impactEffect, impactPosition);
                player.Crash();
            }
        }
    }
}
