using System;
using UnityEngine;
using UnityEngine.UI;

namespace SledSurfers.UI.Lobby
{
    public sealed class LobbyWindowView : UIView
    {
        [SerializeField] private Text _coinBalance;
        [SerializeField] private Button _playButton;

        public event Action PlayRequested;

        public void SetCoinBalance(int balance)
        {
            _coinBalance.text = Mathf.Max(0, balance).ToString("N0");
        }

        private void OnEnable()
        {
            _playButton.onClick.AddListener(OnPlayClicked);
        }

        private void OnDisable()
        {
            _playButton.onClick.RemoveListener(OnPlayClicked);
        }

        private void OnPlayClicked()
        {
            PlayRequested?.Invoke();
        }
    }
}
