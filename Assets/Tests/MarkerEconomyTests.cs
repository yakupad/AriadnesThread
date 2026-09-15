using AriadnesThread.Core.Economy;
using AriadnesThread.Core.Grid;
using NUnit.Framework;

namespace AriadnesThread.Core.Tests
{
    public class MarkerEconomyTests
    {
        [Test]
        public void PlacingOnEmptyCell_ConsumesOneStock()
        {
            var economy = new MarkerEconomy(startingStock: 3, maxStock: 8, lifetimeSteps: 10);
            economy.TryPlaceOrRecolor(new CellCoord(0, 0), MarkerColor.Green);
            Assert.That(economy.Stock, Is.EqualTo(2));
        }

        [Test]
        public void RecoloringAnExistingMarker_IsFree()
        {
            var economy = new MarkerEconomy(startingStock: 3, maxStock: 8, lifetimeSteps: 10);
            var cell = new CellCoord(0, 0);

            economy.TryPlaceOrRecolor(cell, MarkerColor.Green);
            economy.TryPlaceOrRecolor(cell, MarkerColor.Red);

            Assert.That(economy.Stock, Is.EqualTo(2), "Recoloring must not spend a second unit of stock.");
            Assert.That(economy.GetColor(cell), Is.EqualTo(MarkerColor.Red));
        }

        [Test]
        public void PlacingWithNoStock_Fails()
        {
            var economy = new MarkerEconomy(startingStock: 0, maxStock: 8, lifetimeSteps: 10);
            var placed = economy.TryPlaceOrRecolor(new CellCoord(0, 0), MarkerColor.Green);
            Assert.That(placed, Is.False);
        }

        [Test]
        public void PassiveDecay_RemovesMarker_WithoutRefundingStock()
        {
            var economy = new MarkerEconomy(startingStock: 3, maxStock: 8, lifetimeSteps: 2);
            var cell = new CellCoord(0, 0);
            economy.TryPlaceOrRecolor(cell, MarkerColor.Green);

            economy.OnStep();
            economy.OnStep();

            Assert.That(economy.GetColor(cell), Is.Null);
            Assert.That(economy.Stock, Is.EqualTo(2), "Fading away is a loss, not a refund.");
        }

        [Test]
        public void DeliberateRetrieval_RefundsStock()
        {
            var economy = new MarkerEconomy(startingStock: 3, maxStock: 8, lifetimeSteps: 10);
            var cell = new CellCoord(0, 0);
            economy.TryPlaceOrRecolor(cell, MarkerColor.Green);

            var retrieved = economy.TryRetrieve(cell);

            Assert.That(retrieved, Is.True);
            Assert.That(economy.Stock, Is.EqualTo(3));
            Assert.That(economy.GetColor(cell), Is.Null);
        }

        [Test]
        public void IsFading_TrueOnlyNearExpiry()
        {
            var economy = new MarkerEconomy(startingStock: 3, maxStock: 8, lifetimeSteps: 10);
            var cell = new CellCoord(0, 0);
            economy.TryPlaceOrRecolor(cell, MarkerColor.Green);

            Assert.That(economy.IsFading(cell, warningThresholdSteps: 3), Is.False);

            for (int i = 0; i < 8; i++) economy.OnStep();

            Assert.That(economy.IsFading(cell, warningThresholdSteps: 3), Is.True);
        }
    }
}
