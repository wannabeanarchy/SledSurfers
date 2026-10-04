using System;
using UnityEngine;
using UnityEngine.UI;

namespace SledSurfers.UI.Results
{
    public sealed class RunResultPopupView : UIView
    {
        [SerializeField] private Text _coinsEarned;
        [SerializeField] private Button _continueButton;

        public event Action ContinueRequested;

        public void SetCoinsEarned(int coins)
        {
            _coinsEarned.text = $"YOU GOT: {Mathf.Max(0, coins):N0} COINS";
        }

        private void OnEnable()
        {
            _continueButton.onClick.AddListener(OnContinueClicked);
        }

        private void OnDisable()
        {
            _continueButton.onClick.RemoveListener(OnContinueClicked);
        }

        private void OnContinueClicked()
        {
            ContinueRequested?.Invoke();
        }
    }
}
