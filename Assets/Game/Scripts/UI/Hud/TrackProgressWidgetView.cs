using TMPro;
using UnityEngine;

namespace SledSurfers.UI.Hud
{
    public sealed class TrackProgressWidgetView : UIView
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private RectTransform _fill;
        [SerializeField] private RectTransform[] _coinMarkers;
        [SerializeField] private RectTransform _bestMarker;
        [SerializeField] private TMP_Text _bestLabel;
        private int _displayedValue = -1;

        public void SetValue(float value)
        {
            value = Mathf.Clamp01(value);
            _fill.anchorMax = new Vector2(1, value);
            var displayedValue = Mathf.RoundToInt(value * 100);
            if (_displayedValue != displayedValue)
            {
                _displayedValue = displayedValue;
                _label.SetText("{0}%", displayedValue);
            }
        }

        public void SetCoinMarkers(float[] positions)
        {
            for (var i = 0; i < _coinMarkers.Length; i++)
            {
                if (_coinMarkers[i] == null)
                {
                    continue;
                }

                var hasPosition = positions != null && i < positions.Length;
                var progress = hasPosition ? Mathf.Clamp01(positions[i]) : 0f;
                _coinMarkers[i].anchorMin = new Vector2(.5f, progress);
                _coinMarkers[i].anchorMax = new Vector2(.5f, progress);
                _coinMarkers[i].anchoredPosition = Vector2.zero;
                _coinMarkers[i].gameObject.SetActive(hasPosition);
            }
        }

        public void SetBestProgress(float progress, bool hasBestDistance)
        {
            if (_bestMarker == null || _bestLabel == null)
            {
                return;
            }

            progress = Mathf.Clamp01(progress);
            _bestMarker.anchorMin = new Vector2(.5f, progress);
            _bestMarker.anchorMax = new Vector2(.5f, progress);
            _bestMarker.anchoredPosition = Vector2.zero;
            var labelRect = _bestLabel.rectTransform;
            labelRect.anchorMin = new Vector2(.5f, progress);
            labelRect.anchorMax = new Vector2(.5f, progress);
            labelRect.anchoredPosition = new Vector2(48, 0);
            _bestMarker.gameObject.SetActive(hasBestDistance);
            _bestLabel.gameObject.SetActive(hasBestDistance);
        }
    }
}
