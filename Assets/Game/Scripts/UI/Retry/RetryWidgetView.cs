using System;
using UnityEngine;
using UnityEngine.UI;

namespace SledSurfers.UI.Retry
{
    public sealed class RetryWidgetView : UIView
    {
        [SerializeField] private Button _button;
        public event Action RetryRequested;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnClicked);
        }

        private void OnClicked()
        {
            RetryRequested?.Invoke();
        }
    }
}
