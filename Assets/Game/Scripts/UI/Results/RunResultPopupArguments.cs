using System;

namespace SledSurfers.UI.Results
{
    public readonly struct RunResultPopupArguments
    {
        public int CoinsEarned { get; }
        public Action ContinueRequested { get; }

        public RunResultPopupArguments(int coinsEarned, Action continueRequested)
        {
            CoinsEarned = coinsEarned;
            ContinueRequested = continueRequested ?? throw new ArgumentNullException(nameof(continueRequested));
        }
    }
}
