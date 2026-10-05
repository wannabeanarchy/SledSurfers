using UnityEngine;

namespace SledSurfers.Input
{
    public sealed class DragSteering
    {
        private float _pointerOrigin;
        public bool IsDragging { get; private set; }
        public float SteeringInput { get; private set; }

        public void Begin(float pointerX)
        {
            _pointerOrigin = pointerX;
            SteeringInput = 0;
            IsDragging = true;
        }

        public void Move(float pointerX, float screenWidth, float fullSteeringDragFraction)
        {
            if (!IsDragging || screenWidth <= 0 || fullSteeringDragFraction <= 0)
            {
                return;
            }

            var fullSteeringDrag = screenWidth * fullSteeringDragFraction;
            SteeringInput = Mathf.Clamp((pointerX - _pointerOrigin) / fullSteeringDrag, -1, 1);
        }

        public void End()
        {
            IsDragging = false;
            SteeringInput = 0;
        }
    }
}
