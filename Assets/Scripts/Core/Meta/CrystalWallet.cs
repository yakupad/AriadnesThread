namespace AriadnesThread.Core.Meta
{
    /// <summary>The persistent currency. Never purchasable with real money — see the
    /// design doc's monetization decision: Kristal only comes from playing well.</summary>
    public sealed class CrystalWallet
    {
        public int Balance { get; private set; }

        public CrystalWallet(int startingBalance = 0) => Balance = startingBalance;

        public void Add(int amount) => Balance += amount;

        public bool TrySpend(int amount)
        {
            if (amount > Balance) return false;
            Balance -= amount;
            return true;
        }
    }
}
