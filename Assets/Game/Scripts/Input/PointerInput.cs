using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SledSurfers.Input
{
    public sealed class PointerInput
    {
        public event Action<Vector2> Pressed;
        public event Action<Vector2> Moved;
        public event Action Released;
        public event Action Canceled;
        private int _fingerId = -1;
        private bool _mouseHeld;

        public void Tick()
        {
            if (!_mouseHeld && (UnityEngine.Input.touchCount > 0 || _fingerId >= 0))
            {
                ReadTouch();
                return;
            }
            ReadMouse(UnityEngine.Input.mousePosition, UnityEngine.Input.GetMouseButtonDown(0),
                UnityEngine.Input.GetMouseButton(0), UnityEngine.Input.GetMouseButtonUp(0));
        }

        private void ReadMouse(Vector2 position, bool pressed, bool held, bool released)
        {
            if (pressed && !_mouseHeld && IsInsideScreen(position) && !IsOverUI(-1))
            {
                _mouseHeld = true;
                Pressed?.Invoke(position);
            }
            if (_mouseHeld && held)
            {
                Moved?.Invoke(position);
            }
            if (_mouseHeld && released)
            {
                Moved?.Invoke(position);
                _mouseHeld = false;
                Released?.Invoke();
            }
            else if (_mouseHeld && !held)
            {
                Cancel();
            }
        }

        public void Cancel()
        {
            _fingerId = -1;
            _mouseHeld = false;
            Canceled?.Invoke();
        }

        private void ReadTouch()
        {
            for (var i = 0; i < UnityEngine.Input.touchCount; i++)
            {
                var touch = UnityEngine.Input.GetTouch(i);
                if (_fingerId < 0 && touch.phase == TouchPhase.Began && IsInsideScreen(touch.position) && !IsOverUI(touch.fingerId))
                {
                    _fingerId = touch.fingerId;
                    Pressed?.Invoke(touch.position);
                }
                if (touch.fingerId != _fingerId) { continue; }
                if (touch.phase == TouchPhase.Canceled) { Cancel(); return; }
                Moved?.Invoke(touch.position);
                if (touch.phase == TouchPhase.Ended)
                {
                    _fingerId = -1;
                    Released?.Invoke();
                }
                return;
            }
            // Missing touch samples are not release events; focus loss cancels the gesture.
        }

        private static bool IsInsideScreen(Vector2 position)
        {
            return position.x >= 0 && position.y >= 0 && position.x < Screen.width && position.y < Screen.height;
        }

        private static bool IsOverUI(int pointerId)
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId);
        }
    }
}
