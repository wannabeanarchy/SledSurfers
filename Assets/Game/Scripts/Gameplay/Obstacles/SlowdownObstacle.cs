using System.Collections.Generic;
using SledSurfers.Gameplay.Player;
using UnityEngine;

namespace SledSurfers.Gameplay.Obstacles
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class SlowdownObstacle : MonoBehaviour
    {
        [SerializeField, Min(0)] private float _additionalResistance = 30;
        private readonly HashSet<PlayerMotor> _players = new HashSet<PlayerMotor>();
        private BoxCollider _trigger;

        private void Awake()
        {
            _trigger = GetComponent<BoxCollider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            var body = other.attachedRigidbody;
            if (body != null && body.TryGetComponent<PlayerMotor>(out var player))
            {
                _players.Add(player);
                player.EnterSlowdownZone(_trigger, _additionalResistance);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var body = other.attachedRigidbody;
            if (body != null && body.TryGetComponent<PlayerMotor>(out var player))
            {
                _players.Remove(player);
                player.ExitSlowdownZone(_trigger);
            }
        }

        private void OnDisable()
        {
            foreach (var player in _players)
            {
                if (player != null)
                {
                    player.ExitSlowdownZone(_trigger);
                }
            }
            _players.Clear();
        }
    }
}
