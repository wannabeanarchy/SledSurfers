using System;

namespace SledSurfers.UI.Results
{
    public readonly struct RunResultPopupArguments
    {
        public int CoinsEarned { get; }
        public Action RetryRequested { get; }
        public Action ContinueRequested { get; }

        public RunResultPopupArguments(int coinsEarned, Action retryRequested, Action continueRequested)
        {
            CoinsEarned = coinsEarned;
            RetryRequested = retryRequested ?? throw new ArgumentNullException(nameof(retryRequested));
            ContinueRequested = continueRequested ?? throw new ArgumentNullException(nameof(continueRequested));
        }
    }
}
