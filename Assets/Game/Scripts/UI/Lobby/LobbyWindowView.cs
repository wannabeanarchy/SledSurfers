using System;
using SledSurfers.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace SledSurfers.UI.Lobby
{
    public sealed class LobbyWindowView : UIView
    {
        [SerializeField] private Text _coinBalance;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _resetButton;
        [SerializeField] private UpgradeCardView _slingshotUpgradeCard;
        [SerializeField] private UpgradeCardView _skateUpgradeCard;
        [SerializeField] private UpgradeCardView _incomeUpgradeCard;

        public event Action PlayRequested;
        public event Action ResetRequested;
        public event Action<PlayerUpgradeType> UpgradeRequested;

        public void SetCoinBalance(int balance)
        {
            _coinBalance.text = Mathf.Max(0, balance).ToString("N0");
        }

        public void SetUpgradeCards(
            LobbyUpgradeCardState slingshot,
            LobbyUpgradeCardState skate,
            LobbyUpgradeCardState income)
        {
            _slingshotUpgradeCard.SetState(slingshot);
            _skateUpgradeCard.SetState(skate);
            _incomeUpgradeCard.SetState(income);
        }

        private void OnEnable()
        {
            _playButton.onClick.AddListener(OnPlayClicked);
            _resetButton.onClick.AddListener(OnResetClicked);
            _slingshotUpgradeCard.UpgradeRequested += OnUpgradeRequested;
            _skateUpgradeCard.UpgradeRequested += OnUpgradeRequested;
            _incomeUpgradeCard.UpgradeRequested += OnUpgradeRequested;
        }

        private void OnDisable()
        {
            _playButton.onClick.RemoveListener(OnPlayClicked);
            _resetButton.onClick.RemoveListener(OnResetClicked);
            _slingshotUpgradeCard.UpgradeRequested -= OnUpgradeRequested;
            _skateUpgradeCard.UpgradeRequested -= OnUpgradeRequested;
            _incomeUpgradeCard.UpgradeRequested -= OnUpgradeRequested;
        }

        private void OnPlayClicked()
        {
            PlayRequested?.Invoke();
        }

        private void OnResetClicked()
        {
            ResetRequested?.Invoke();
        }

        private void OnUpgradeRequested(PlayerUpgradeType upgradeType)
        {
            UpgradeRequested?.Invoke(upgradeType);
        }

    }
}
