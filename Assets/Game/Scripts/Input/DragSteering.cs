using UnityEngine;

namespace SledSurfers.Input
{
    public sealed class DragSteering
    {
        private float _pointerOrigin;
        private float _playerOrigin;
        public bool IsDragging { get; private set; }
        public float TargetX { get; private set; }

        public void Begin(float pointerX, float playerX)
        {
            _pointerOrigin = pointerX;
            _playerOrigin = playerX;
            TargetX = playerX;
            IsDragging = true;
        }

        public void Move(float pointerX, float screenWidth, float dragRange, float minimumX, float maximumX)
        {
            if (!IsDragging || screenWidth <= 0)
            {
                return;
            }
            TargetX = Mathf.Clamp(_playerOrigin + (pointerX - _pointerOrigin) / screenWidth * dragRange, minimumX, maximumX);
        }

        public void End(float playerX)
        {
            IsDragging = false;
            TargetX = playerX;
        }
    }
}
