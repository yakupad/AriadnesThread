using AriadnesThread.Core.Economy;
using NUnit.Framework;

namespace AriadnesThread.Core.Tests
{
    public class TorchEconomyTests
    {
        [Test]
        public void Unlit_UsesBaseVisionRadius_AndNeverConsumesFuel()
        {
            var torch = new TorchEconomy(startingFuel: 10, maxFuel: 50, baseVisionRadius: 1f, litVisionRadius: 5f);

            Assert.That(torch.CurrentVisionRadius, Is.EqualTo(1f));
            torch.OnStep();
            torch.OnStep();
            Assert.That(torch.Fuel, Is.EqualTo(10));
        }

        [Test]
        public void Lit_UsesLitVisionRadius_AndConsumesOneFuelPerStep()
        {
            var torch = new TorchEconomy(startingFuel: 10, maxFuel: 50, baseVisionRadius: 1f, litVisionRadius: 5f);
            torch.SetLit(true);

            Assert.That(torch.CurrentVisionRadius, Is.EqualTo(5f));
            torch.OnStep();
            Assert.That(torch.Fuel, Is.EqualTo(9));
        }

        [Test]
        public void RunningOutOfFuel_AutomaticallyExtinguishes()
        {
            var torch = new TorchEconomy(startingFuel: 2, maxFuel: 50, baseVisionRadius: 1f, litVisionRadius: 5f);
            torch.SetLit(true);

            torch.OnStep();
            torch.OnStep();

            Assert.That(torch.Fuel, Is.EqualTo(0));
            Assert.That(torch.IsLit, Is.False);
            Assert.That(torch.CurrentVisionRadius, Is.EqualTo(1f), "Should fall back to base radius, not go dark entirely.");
        }

        [Test]
        public void SetLit_WithNoFuel_RefusesToLight()
        {
            var torch = new TorchEconomy(startingFuel: 0, maxFuel: 50);
            torch.SetLit(true);
            Assert.That(torch.IsLit, Is.False);
        }

        [Test]
        public void AddFuel_NeverExceedsMaxFuel()
        {
            var torch = new TorchEconomy(startingFuel: 45, maxFuel: 50);
            torch.AddFuel(20);
            Assert.That(torch.Fuel, Is.EqualTo(50));
        }
    }
}
