using TMPro;
using UnityEngine;

namespace SledSurfers.UI.Hud
{
    public sealed class TrackProgressWidgetView : UIView
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private RectTransform _fill;
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
    }
}
