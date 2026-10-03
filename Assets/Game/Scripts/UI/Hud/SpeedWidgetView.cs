using TMPro;
using UnityEngine;

namespace SledSurfers.UI.Hud
{
    public sealed class SpeedWidgetView : UIView
    {
        [SerializeField] private TMP_Text _label;
        private int _displayedValue = -1;

        public void SetValue(float value)
        {
            var displayedValue = Mathf.RoundToInt(Mathf.Max(0, value));
            if (_displayedValue != displayedValue)
            {
                _displayedValue = displayedValue;
                _label.SetText("{0} km/h", displayedValue);
            }
        }
    }
}
