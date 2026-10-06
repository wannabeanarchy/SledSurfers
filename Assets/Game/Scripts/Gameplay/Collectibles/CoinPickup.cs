using System;
using SledSurfers.Gameplay.Player;
using UnityEngine;

namespace SledSurfers.Gameplay.Collectibles
{
    public sealed class CoinPickup : MonoBehaviour
    {
        private bool _isCollected;

        public event Action Collected;

        private void OnTriggerEnter(Collider other)
        {
            if (_isCollected || !isActiveAndEnabled)
            {
                return;
            }

            var body = other.attachedRigidbody;
            if (body == null || !body.TryGetComponent<PlayerMotor>(out var player) || !player.IsRunning)
            {
                return;
            }

            _isCollected = true;
            Collected?.Invoke();
            gameObject.SetActive(false);
        }

        public void ResetPickup()
        {
            _isCollected = false;
            gameObject.SetActive(true);
        }
    }
}
