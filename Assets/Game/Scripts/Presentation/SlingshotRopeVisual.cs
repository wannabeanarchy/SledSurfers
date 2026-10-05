using SledSurfers.Gameplay.Launch;
using UnityEngine;

namespace SledSurfers.Presentation
{
    public sealed class SlingshotRopeVisual : MonoBehaviour
    {
        [SerializeField] private Transform _leftAnchor;
        [SerializeField] private Transform _rightAnchor;
        [SerializeField] private Vector3 _anchorOffset = new Vector3(0.01f, 1.16f, -0.76f);
        [SerializeField] private Vector3 _waistCenterOffset = new Vector3(0, 4f, -1.12f);
        [SerializeField, Min(0.1f)] private float _waistHalfWidth = 0.35f;
        [SerializeField, Min(0.01f)] private float _waistDepth = 0.25f;
        [SerializeField, Min(0)] private float _restSag = 0.65f;
        [SerializeField, Min(0.001f)] private float _lineWidth = 0.055f;
        [SerializeField] private Color _ropeColor = new Color(0.64f, 0.43f, 0.23f);
        [SerializeField, Range(2, 24)] private int _tailSegments = 8;
        [SerializeField, Range(8, 48)] private int _waistSegments = 24;

        private LaunchSession _session;
        private Transform _player;
        private LineRenderer _rope;
        private Material _lineMaterial;

        public void Initialize(LaunchSession session, Transform player)
        {
            _session = session;
            _player = player;
        }

        private void Awake()
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader == null)
            {
                Debug.LogError("Could not find a shader for the slingshot rope.", this);
                enabled = false;
                return;
            }

            _lineMaterial = new Material(shader)
            {
                name = "Slingshot Rope (Runtime)",
                color = _ropeColor
            };

            _tailSegments = Mathf.Max(2, _tailSegments);
            _waistSegments = Mathf.Max(8, _waistSegments);
            var ropeObject = new GameObject("Slingshot Rope");
            ropeObject.transform.SetParent(transform, false);

            _rope = ropeObject.AddComponent<LineRenderer>();
            _rope.useWorldSpace = true;
            _rope.positionCount = _tailSegments * 2 + _waistSegments + 1;
            _rope.startWidth = _lineWidth;
            _rope.endWidth = _lineWidth;
            _rope.numCapVertices = 3;
            _rope.numCornerVertices = 3;
            _rope.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _rope.receiveShadows = false;
            _rope.sharedMaterial = _lineMaterial;
            _rope.startColor = _lineMaterial.color;
            _rope.endColor = _lineMaterial.color;
            _rope.enabled = false;
        }

        private void LateUpdate()
        {
            if (_session == null || _player == null || _leftAnchor == null || _rightAnchor == null)
            {
                SetVisible(false);
                return;
            }

            var isVisible = _session.Phase == RunPhase.Ready || _session.Phase == RunPhase.Pulling;
            SetVisible(isVisible);
            if (!isVisible)
            {
                return;
            }

            var power = _session.Phase == RunPhase.Pulling ? _session.Power : 0;
            var sag = Mathf.Lerp(_restSag, 0.05f, power);
            var leftStart = _leftAnchor.TransformPoint(_anchorOffset);
            var rightStart = _rightAnchor.TransformPoint(_anchorOffset);
            var waistCenter = _player.TransformPoint(_waistCenterOffset);
            var leftWaist = _player.TransformPoint(_waistCenterOffset + Vector3.left * _waistHalfWidth);
            var rightWaist = _player.TransformPoint(_waistCenterOffset + Vector3.right * _waistHalfWidth);

            DrawTail(0, leftStart, leftWaist, sag);
            DrawWaistArc(_tailSegments + 1, waistCenter);
            DrawTail(_tailSegments + _waistSegments + 1, rightWaist, rightStart, sag, true);
        }

        private void DrawTail(int startIndex, Vector3 start, Vector3 end, float sag, bool skipStart = false)
        {
            var control = (start + end) * 0.5f + Vector3.down * sag;
            var firstSegment = skipStart ? 1 : 0;
            for (var i = firstSegment; i <= _tailSegments; i++)
            {
                var t = i / (float)_tailSegments;
                var inverseT = 1 - t;
                var point = inverseT * inverseT * start + 2 * inverseT * t * control + t * t * end;
                _rope.SetPosition(startIndex + i - firstSegment, point);
            }
        }

        private void DrawWaistArc(int startIndex, Vector3 waistCenter)
        {
            for (var i = 1; i <= _waistSegments; i++)
            {
                var angle = Mathf.PI * i / _waistSegments;
                var localOffset = new Vector3(
                    -_waistHalfWidth * Mathf.Cos(angle),
                    0,
                    -_waistDepth * Mathf.Sin(angle));
                _rope.SetPosition(startIndex + i - 1, waistCenter + _player.TransformVector(localOffset));
            }
        }

        private void SetVisible(bool visible)
        {
            if (_rope != null)
            {
                _rope.enabled = visible;
            }
        }

        private void OnDestroy()
        {
            if (_lineMaterial != null)
            {
                Destroy(_lineMaterial);
            }
        }
    }
}
