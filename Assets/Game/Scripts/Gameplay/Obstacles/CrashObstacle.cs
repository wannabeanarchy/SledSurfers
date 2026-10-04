using SledSurfers.Gameplay.Player;
using UnityEngine;

namespace SledSurfers.Gameplay.Obstacles
{
    public sealed class CrashObstacle : MonoBehaviour
    {
        private void OnCollisionEnter(Collision collision)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            var body = collision.rigidbody;
            if (body != null && body.TryGetComponent<PlayerMotor>(out var player))
            {
                player.Crash();
            }
        }
    }
}
