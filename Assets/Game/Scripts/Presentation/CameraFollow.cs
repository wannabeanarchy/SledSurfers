using UnityEngine;

namespace SledSurfers.Presentation
{
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        private Vector3 _offset;
        private Vector3 _startPosition;
        [SerializeField, Min(.01f)] private float _smoothTime = .18f;
        [SerializeField, Min(0f)] private float _maximumBackwardDistance = 2f;
        private Vector3 _velocity;
        private float _minimumZ;

        private void Start()
        {
            if (_target == null)
            {
                Debug.LogError("Camera follow target is missing.", this);
                enabled = false;
                return;
            }
            _startPosition = transform.position;
            _offset = transform.position - _target.position;
            _minimumZ = transform.position.z - Mathf.Max(0f, _maximumBackwardDistance);
        }

        public void ResetRun()
        {
            transform.position = _startPosition;
            _velocity = Vector3.zero;
        }

        private void LateUpdate()
        {
            var desiredPosition = _target.position + _offset;
            desiredPosition.z = Mathf.Max(_minimumZ, desiredPosition.z);
            var position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, _smoothTime);
            if (position.z < _minimumZ)
            {
                position.z = _minimumZ;
                _velocity.z = Mathf.Max(0f, _velocity.z);
            }
            transform.position = position;
        }
    }
}
