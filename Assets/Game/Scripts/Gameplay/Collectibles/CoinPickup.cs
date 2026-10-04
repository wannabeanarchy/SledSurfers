using SledSurfers.Gameplay.Player;
using UnityEngine;

namespace SledSurfers.Gameplay.Collectibles
{
    public sealed class CoinPickup : MonoBehaviour
    {
        private bool _isCollected;

        private void OnTriggerEnter(Collider other)
        {
            if (_isCollected || !isActiveAndEnabled)
            {
                return;
            }

            var body = other.attachedRigidbody;
            if (body == null || !body.TryGetComponent<PlayerMotor>(out _))
            {
                return;
            }

            _isCollected = true;
            gameObject.SetActive(false);
        }

        public void ResetPickup()
        {
            _isCollected = false;
            gameObject.SetActive(true);
        }
    }
}
