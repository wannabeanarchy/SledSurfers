using System;
using SledSurfers.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace SledSurfers.UI.Lobby
{
    public sealed class UpgradeCardView : MonoBehaviour
    {
        [SerializeField] private PlayerUpgradeType _upgradeType;
        [SerializeField] private Button _button;
        [SerializeField] private Image _buttonImage;
        [SerializeField] private Sprite _availableButtonSprite;
        [SerializeField] private Sprite _unavailableButtonSprite;
        [SerializeField] private Text _title;
        [SerializeField] private Text _description;
        [SerializeField] private Image _icon;
        [SerializeField] private Text _currentValue;
        [SerializeField] private Text _cost;
        [SerializeField] private Image[] _powerSegments;
        [SerializeField] private int _redAtLevel = 6;
        [SerializeField] private Color _emptySegmentColor = new Color32(83, 139, 182, 255);
        [SerializeField] private Color _filledSegmentColor = new Color32(255, 207, 42, 255);
        [SerializeField] private Color _redSegmentColor = new Color32(239, 66, 66, 255);

        public event Action<PlayerUpgradeType> UpgradeRequested;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
        }

        public void SetState(LobbyUpgradeCardState state)
        {
            _currentValue.text = state.CurrentValue;
            _cost.text = state.Cost;
            _button.interactable = state.CanPurchase;
            _buttonImage.sprite = state.CanPurchase ? _availableButtonSprite : _unavailableButtonSprite;

            var filledSegments = Mathf.Clamp(state.Level, 0, _powerSegments.Length);
            var redSegments = Mathf.Clamp(state.Level - _redAtLevel + 1, 0, filledSegments);
            for (var i = 0; i < _powerSegments.Length; i++)
            {
                if (i < redSegments)
                {
                    _powerSegments[i].color = _redSegmentColor;
                }
                else if (i < filledSegments)
                {
                    _powerSegments[i].color = _filledSegmentColor;
                }
                else
                {
                    _powerSegments[i].color = _emptySegmentColor;
                }
            }
        }

        private void OnButtonClicked()
        {
            UpgradeRequested?.Invoke(_upgradeType);
        }

    }
}
