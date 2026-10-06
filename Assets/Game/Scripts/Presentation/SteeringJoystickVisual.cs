using UnityEngine;
using UnityEngine.UI;

namespace SledSurfers.Presentation
{
    public sealed class SteeringJoystickVisual
    {
        private const int SpriteSize = 64;
        private readonly RectTransform _canvasRoot;
        private readonly Canvas _canvas;
        private readonly RectTransform _joystickRoot;
        private readonly RectTransform _handle;
        private readonly Sprite _circleSprite;
        private readonly Texture2D _circleTexture;
        private readonly float _radius;

        public float SteeringRadiusPixels => _radius * Mathf.Max(.01f, _canvas.scaleFactor);

        public SteeringJoystickVisual(Transform parent, float radius)
        {
            _canvasRoot = parent as RectTransform;
            _canvas = parent.GetComponentInParent<Canvas>();
            _radius = Mathf.Max(24, radius);
            _circleTexture = CreateCircleTexture();
            _circleSprite = Sprite.Create(
                _circleTexture,
                new Rect(0, 0, SpriteSize, SpriteSize),
                new Vector2(.5f, .5f),
                100);
            _circleSprite.name = "Steering Joystick Circle";

            var baseObject = CreateImageObject("Steering Joystick", parent, _circleSprite);
            _joystickRoot = (RectTransform)baseObject.transform;
            ConfigureCenteredRect(_joystickRoot, Vector2.one * (_radius * 2));
            baseObject.GetComponent<Image>().color = new Color(.04f, .16f, .25f, .34f);

            var handleObject = CreateImageObject("Handle", _joystickRoot, _circleSprite);
            _handle = (RectTransform)handleObject.transform;
            ConfigureCenteredRect(_handle, Vector2.one * (_radius * .82f));
            handleObject.GetComponent<Image>().color = new Color(.35f, .88f, .96f, .88f);
            _joystickRoot.gameObject.SetActive(false);
        }

        public void ShowAt(Vector2 screenPosition)
        {
            if (!TryGetLocalPosition(screenPosition, out var localPosition))
            {
                return;
            }

            _joystickRoot.anchoredPosition = localPosition;
            _handle.anchoredPosition = Vector2.zero;
            _joystickRoot.gameObject.SetActive(true);
        }

        public void Move(Vector2 screenPosition)
        {
            if (!_joystickRoot.gameObject.activeSelf || !TryGetLocalPosition(screenPosition, out var localPosition))
            {
                return;
            }

            var delta = localPosition - _joystickRoot.anchoredPosition;
            _handle.anchoredPosition = new Vector2(Mathf.Clamp(delta.x, -_radius, _radius), 0);
        }

        public void Hide()
        {
            if (_joystickRoot != null)
            {
                _joystickRoot.gameObject.SetActive(false);
            }
        }

        public void Dispose()
        {
            if (_joystickRoot != null)
            {
                Object.Destroy(_joystickRoot.gameObject);
            }
            if (_circleSprite != null)
            {
                Object.Destroy(_circleSprite);
            }
            if (_circleTexture != null)
            {
                Object.Destroy(_circleTexture);
            }
        }

        private bool TryGetLocalPosition(Vector2 screenPosition, out Vector2 localPosition)
        {
            var camera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRoot,
                screenPosition,
                camera,
                out localPosition);
        }

        private static GameObject CreateImageObject(string name, Transform parent, Sprite sprite)
        {
            var imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageObject.layer = parent.gameObject.layer;
            imageObject.transform.SetParent(parent, false);
            var image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return imageObject;
        }

        private static void ConfigureCenteredRect(RectTransform rectTransform, Vector2 size)
        {
            rectTransform.anchorMin = new Vector2(.5f, .5f);
            rectTransform.anchorMax = new Vector2(.5f, .5f);
            rectTransform.pivot = new Vector2(.5f, .5f);
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = Vector2.zero;
        }

        private static Texture2D CreateCircleTexture()
        {
            var texture = new Texture2D(SpriteSize, SpriteSize, TextureFormat.RGBA32, false)
            {
                name = "Steering Joystick Circle Texture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[SpriteSize * SpriteSize];
            for (var y = 0; y < SpriteSize; y++)
            {
                for (var x = 0; x < SpriteSize; x++)
                {
                    var offset = new Vector2(x + .5f, y + .5f) / SpriteSize * 2 - Vector2.one;
                    var coverage = Mathf.Clamp01((1 - offset.magnitude) * SpriteSize);
                    pixels[y * SpriteSize + x] = new Color32(255, 255, 255, (byte)(coverage * 255));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }
    }
}
