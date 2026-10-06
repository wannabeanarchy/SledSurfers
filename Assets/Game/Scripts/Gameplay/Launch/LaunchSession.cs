using UnityEngine;

namespace SledSurfers.Gameplay.Launch
{
    public enum RunPhase { Ready, Pulling, Running, Stopped }

    public sealed class LaunchSession
    {
        private readonly float _maximumSpeed;
        private readonly float _maximumAngle;
        private readonly float _pullScreenFraction;
        private Vector2 _origin;
        public RunPhase Phase { get; private set; }
        public float Power { get; private set; }
        public float Angle { get; private set; }
        public float AimOffset { get; private set; }

        public LaunchSession(float maximumSpeed, float maximumAngle, float pullScreenFraction)
        {
            _maximumSpeed = maximumSpeed;
            _maximumAngle = maximumAngle;
            _pullScreenFraction = pullScreenFraction;
        }

        public void Begin(Vector2 position)
        {
            if (Phase != RunPhase.Ready) { return; }
            _origin = position;
            Phase = RunPhase.Pulling;
        }

        public void Move(Vector2 position, float screenWidth, float screenHeight)
        {
            if (Phase != RunPhase.Pulling || screenWidth <= 0 || screenHeight <= 0) { return; }
            var aimDisplacement = (position.x - _origin.x) / (screenWidth * _pullScreenFraction);
            var backwardDisplacement = (_origin.y - position.y) / (screenHeight * _pullScreenFraction);
            AimOffset = Mathf.Clamp(aimDisplacement, -1, 1);
            Power = Mathf.Clamp01(backwardDisplacement);
            Angle = -AimOffset * _maximumAngle;
        }

        public bool Release(out Vector3 velocity)
        {
            velocity = Vector3.zero;
            if (Phase != RunPhase.Pulling) { return false; }
            if (Power < .05f) { Cancel(); return false; }
            velocity = Quaternion.Euler(0, Angle, 0) * Vector3.forward * (Power * _maximumSpeed);
            Phase = RunPhase.Running;
            return true;
        }

        public void Cancel()
        {
            if (Phase != RunPhase.Pulling) { return; }
            Power = 0;
            Angle = 0;
            AimOffset = 0;
            Phase = RunPhase.Ready;
        }

        public void Reset()
        {
            Phase = RunPhase.Ready;
            Power = 0;
            Angle = 0;
            AimOffset = 0;
            _origin = Vector2.zero;
        }

        public void Stop() { Phase = RunPhase.Stopped; }
    }
}
