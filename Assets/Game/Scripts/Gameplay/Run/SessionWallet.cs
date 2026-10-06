using UnityEngine;

namespace SledSurfers.Gameplay.Run
{
    public static class SessionWallet
    {
        private static int _balance;
        private static bool _isInitialized;

        public static int Balance => _balance;

        public static void Initialize(int startingBalance)
        {
            if (_isInitialized)
            {
                return;
            }

            _balance = Mathf.Max(0, startingBalance);
            _isInitialized = true;
        }

        public static void ResetBalance(int startingBalance)
        {
            _balance = Mathf.Max(0, startingBalance);
            _isInitialized = true;
        }

        public static void Add(int amount)
        {
            Initialize(0);
            _balance += Mathf.Max(0, amount);
        }

        public static bool TrySpend(int amount)
        {
            Initialize(0);
            amount = Mathf.Max(0, amount);
            if (_balance < amount)
            {
                return false;
            }

            _balance -= amount;
            return true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            _balance = 0;
            _isInitialized = false;
        }
    }
}
