using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SledSurfers.UI.Results
{
    public sealed class RunResultPopupView : UIView
    {
        [SerializeField] private Text _coinsEarned;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _continueButton;
        [SerializeField, Min(0)] private float _fadeDuration = .35f;
        [SerializeField] private CanvasGroup _canvasGroup;

        private Coroutine _fadeRoutine;

        public event Action RetryRequested;
        public event Action ContinueRequested;

        public void SetCoinsEarned(int coins)
        {
            _coinsEarned.text = $"YOU GOT: {Mathf.Max(0, coins):N0} COINS";
        }

        private void OnEnable()
        {
            _retryButton.onClick.AddListener(OnRetryClicked);
            _continueButton.onClick.AddListener(OnContinueClicked);
            if (_canvasGroup != null)
            {
                _fadeRoutine = StartCoroutine(FadeIn());
            }
        }

        private void OnDisable()
        {
            _retryButton.onClick.RemoveListener(OnRetryClicked);
            _continueButton.onClick.RemoveListener(OnContinueClicked);
            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
                _fadeRoutine = null;
            }
        }

        private void OnRetryClicked()
        {
            RetryRequested?.Invoke();
        }

        private void OnContinueClicked()
        {
            ContinueRequested?.Invoke();
        }

        private IEnumerator FadeIn()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = true;

            var duration = Mathf.Max(0, _fadeDuration);
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _fadeRoutine = null;
        }
    }
}
