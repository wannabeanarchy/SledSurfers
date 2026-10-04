using UnityEngine;

namespace SledSurfers.Gameplay.Run
{
    public sealed class RunMetrics
    {
        private float _launchZ;
        private float _targetDistance;
        public float Distance { get; private set; }
        public float SpeedKmh { get; private set; }
        public int CollectedCoinCount { get; private set; }
        public float Progress => _targetDistance > 0 ? Mathf.Clamp01(Distance / _targetDistance) : 0;

        public void Begin(float launchZ, float targetDistance)
        {
            Reset();
            _launchZ = launchZ;
            _targetDistance = Mathf.Max(0, targetDistance);
        }

        public void Update(float positionZ, Vector3 velocity)
        {
            Distance = Mathf.Max(Distance, positionZ - _launchZ);
            SpeedKmh = velocity.magnitude * 3.6f;
        }

        public void RegisterCoinCollected()
        {
            CollectedCoinCount++;
        }

        public void Reset()
        {
            _launchZ = 0;
            _targetDistance = 0;
            Distance = 0;
            SpeedKmh = 0;
            CollectedCoinCount = 0;
        }
    }
}
