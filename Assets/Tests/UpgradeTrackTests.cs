using AriadnesThread.Core.Meta;
using NUnit.Framework;

namespace AriadnesThread.Core.Tests
{
    public class UpgradeTrackTests
    {
        [Test]
        public void NextCost_GrowsByMultiplierEachLevel()
        {
            var track = new UpgradeTrack(maxLevel: 5, baseCost: 100, costMultiplier: 2.0);

            Assert.That(track.NextCost, Is.EqualTo(100));
            var wallet = new CrystalWallet(1000);
            track.TryUpgrade(wallet);
            Assert.That(track.NextCost, Is.EqualTo(200));
            track.TryUpgrade(wallet);
            Assert.That(track.NextCost, Is.EqualTo(400));
        }

        [Test]
        public void TryUpgrade_WithoutEnoughCrystal_Fails()
        {
            var track = new UpgradeTrack(maxLevel: 5, baseCost: 100);
            var wallet = new CrystalWallet(startingBalance: 50);

            var upgraded = track.TryUpgrade(wallet);

            Assert.That(upgraded, Is.False);
            Assert.That(track.Level, Is.EqualTo(0));
            Assert.That(wallet.Balance, Is.EqualTo(50), "A failed upgrade must not spend anything.");
        }

        [Test]
        public void TryUpgrade_AtMaxLevel_Fails()
        {
            var track = new UpgradeTrack(maxLevel: 1, baseCost: 10);
            var wallet = new CrystalWallet(startingBalance: 1000);

            Assert.That(track.TryUpgrade(wallet), Is.True);
            Assert.That(track.IsMaxed, Is.True);
            Assert.That(track.TryUpgrade(wallet), Is.False);
        }
    }
}
