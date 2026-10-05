using System.Collections;
using UnityEngine;

namespace SledSurfers.Presentation
{
    public sealed class FinishFlagVisual : MonoBehaviour
    {
        [SerializeField] private Transform _model;
        [SerializeField, Min(0)] private float _raiseHeight = 4;
        [SerializeField, Min(0)] private float _hiddenClearance = .25f;
        [SerializeField, Min(.01f)] private float _raiseDuration = .75f;
        [SerializeField] private CoinRewardBurst _coinRewardBurst;

        private Vector3 _modelRestPosition;
        private float _hiddenOffset;
        private Coroutine _raiseRoutine;
        private bool _isPrepared;

        public float RaiseDuration => Mathf.Max(.01f, _raiseDuration);

        private void Awake()
        {
            PrepareModel();
        }

        public void RaiseAt(Vector3 playerPosition, float sideOffset, float trackCenterX)
        {
            if (_model == null)
            {
                return;
            }

            if (!_isPrepared)
            {
                PrepareModel();
            }

            var side = playerPosition.x >= trackCenterX ? -1 : 1;
            transform.SetPositionAndRotation(
                playerPosition + Vector3.right * (side * Mathf.Max(0, sideOffset)),
                Quaternion.identity);

            if (_raiseRoutine != null)
            {
                StopCoroutine(_raiseRoutine);
            }

            var startPosition = _modelRestPosition - Vector3.up * _hiddenOffset;
            _model.localPosition = startPosition;
            _raiseRoutine = StartCoroutine(Raise(startPosition, _modelRestPosition));
        }

        private void PrepareModel()
        {
            if (_model == null)
            {
                return;
            }

            _modelRestPosition = _model.localPosition;
            _hiddenOffset = Mathf.Max(0, _raiseHeight);

            var renderers = _model.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length > 0)
            {
                var bounds = renderers[0].bounds;
                for (var i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }

                var modelTopAboveRoot = bounds.max.y - transform.position.y;
                _hiddenOffset = Mathf.Max(
                    _hiddenOffset,
                    modelTopAboveRoot + Mathf.Max(0, _hiddenClearance));
            }

            _model.localPosition = _modelRestPosition - Vector3.up * _hiddenOffset;
            _isPrepared = true;
        }

        private IEnumerator Raise(Vector3 startPosition, Vector3 targetPosition)
        {
            var duration = Mathf.Max(.01f, _raiseDuration);
            var elapsed = 0f;
            while (elapsed < duration && _model != null)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / duration);
                progress = progress * progress * (3 - 2 * progress);
                _model.localPosition = Vector3.LerpUnclamped(startPosition, targetPosition, progress);
                yield return null;
            }

            if (_model != null)
            {
                _model.localPosition = targetPosition;
                _coinRewardBurst?.Play(GetModelTopPosition());
            }

            _raiseRoutine = null;
        }

        private Vector3 GetModelTopPosition()
        {
            var renderers = _model.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return _model.position + Vector3.up * 1.5f;
            }

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        }
    }
}
