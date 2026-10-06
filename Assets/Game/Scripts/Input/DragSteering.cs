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

        public void Move(float pointerX, float steeringRadiusPixels)
        {
            if (!IsDragging || steeringRadiusPixels <= 0)
            {
                return;
            }

            SteeringInput = Mathf.Clamp((pointerX - _pointerOrigin) / steeringRadiusPixels, -1, 1);
        }

        public void End()
        {
            IsDragging = false;
            SteeringInput = 0;
        }
    }
}
