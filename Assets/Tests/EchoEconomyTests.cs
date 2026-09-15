using AriadnesThread.Core.Economy;
using AriadnesThread.Core.Generation;
using AriadnesThread.Core.Grid;
using NUnit.Framework;

namespace AriadnesThread.Core.Tests
{
    public class EchoEconomyTests
    {
        [Test]
        public void Use_WithNoCharges_Fails()
        {
            var economy = new EchoEconomy(startingCharges: 0, maxCharges: 5, radiusHops: 3, revealLifetimeSteps: 10);
            var grid = MazeGenerator.Generate(6, 6, seed: 1);

            var used = economy.TryUse(grid, new CellCoord(0, 0));

            Assert.That(used, Is.False);
        }

        [Test]
        public void Use_ConsumesOneCharge_AndRevealsOnlyStructure()
        {
            var economy = new EchoEconomy(startingCharges: 2, maxCharges: 5, radiusHops: 2, revealLifetimeSteps: 10);
            var grid = MazeGenerator.Generate(8, 8, seed: 1);

            economy.TryUse(grid, new CellCoord(0, 0));

            Assert.That(economy.Charges, Is.EqualTo(1));
            Assert.That(economy.RevealedCells, Is.Not.Empty);
            Assert.That(economy.RevealedCells, Has.Member(new CellCoord(0, 0)));
        }

        [Test]
        public void RevealedCells_FadeOutAfterLifetimeSteps()
        {
            var economy = new EchoEconomy(startingCharges: 1, maxCharges: 5, radiusHops: 2, revealLifetimeSteps: 3);
            var grid = MazeGenerator.Generate(8, 8, seed: 1);
            economy.TryUse(grid, new CellCoord(0, 0));

            economy.OnStep();
            economy.OnStep();
            Assert.That(economy.RevealedCells, Is.Not.Empty, "Should not have faded yet.");

            economy.OnStep();
            Assert.That(economy.RevealedCells, Is.Empty);
        }

        [Test]
        public void AddCharge_NeverExceedsMax()
        {
            var economy = new EchoEconomy(startingCharges: 4, maxCharges: 5, radiusHops: 2, revealLifetimeSteps: 10);
            economy.AddCharge(3);
            Assert.That(economy.Charges, Is.EqualTo(5));
        }
    }
}
