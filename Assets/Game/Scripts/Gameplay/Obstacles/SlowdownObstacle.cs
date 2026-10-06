using System.Collections.Generic;
using SledSurfers.Gameplay.Player;
using UnityEngine;

namespace SledSurfers.Gameplay.Obstacles
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class SlowdownObstacle : MonoBehaviour
    {
        [SerializeField, Min(0)] private float _additionalResistance = 30;
        private readonly HashSet<PlayerFrictionModule> _players = new HashSet<PlayerFrictionModule>();
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
            if (body != null && body.TryGetComponent<PlayerMotor>(out var player)
                && player.BoardGroundCollider == other
                && body.TryGetComponent<PlayerFrictionModule>(out var friction))
            {
                _players.Add(friction);
                friction.EnterSlowdownZone(_trigger, _additionalResistance);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var body = other.attachedRigidbody;
            if (body != null && body.TryGetComponent<PlayerMotor>(out var player)
                && player.BoardGroundCollider == other
                && body.TryGetComponent<PlayerFrictionModule>(out var friction))
            {
                _players.Remove(friction);
                friction.ExitSlowdownZone(_trigger);
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
