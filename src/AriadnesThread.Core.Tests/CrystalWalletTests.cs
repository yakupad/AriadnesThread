using AriadnesThread.Core.Meta;
using NUnit.Framework;

namespace AriadnesThread.Core.Tests
{
    public class CrystalWalletTests
    {
        [Test]
        public void Add_IncreasesBalance()
        {
            var wallet = new CrystalWallet(startingBalance: 5);
            wallet.Add(3);
            Assert.That(wallet.Balance, Is.EqualTo(8));
        }

        [Test]
        public void TrySpend_WithEnoughBalance_Succeeds()
        {
            var wallet = new CrystalWallet(startingBalance: 10);
            var spent = wallet.TrySpend(7);

            Assert.That(spent, Is.True);
            Assert.That(wallet.Balance, Is.EqualTo(3));
        }

        [Test]
        public void TrySpend_WithoutEnoughBalance_FailsAndLeavesBalanceUnchanged()
        {
            var wallet = new CrystalWallet(startingBalance: 5);
            var spent = wallet.TrySpend(6);

            Assert.That(spent, Is.False);
            Assert.That(wallet.Balance, Is.EqualTo(5));
        }
    }
}
