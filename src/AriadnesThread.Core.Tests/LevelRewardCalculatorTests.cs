using AriadnesThread.Core.Meta;
using NUnit.Framework;

namespace AriadnesThread.Core.Tests
{
    public class LevelRewardCalculatorTests
    {
        [Test]
        public void ZeroEfficiency_ReturnsBaseRewardOnly()
        {
            var calculator = new LevelRewardCalculator(baseReward: 10, efficiencyBonusMultiplier: 20);
            var reward = calculator.CalculateReward(0, 0, 0);
            Assert.That(reward, Is.EqualTo(10));
        }

        [Test]
        public void FullEfficiency_AddsTheFullBonus()
        {
            var calculator = new LevelRewardCalculator(baseReward: 10, efficiencyBonusMultiplier: 20);
            var reward = calculator.CalculateReward(1, 1, 1);
            Assert.That(reward, Is.EqualTo(30));
        }

        [Test]
        public void PartialEfficiency_IsTheAverageOfAllThree()
        {
            var calculator = new LevelRewardCalculator(baseReward: 0, efficiencyBonusMultiplier: 30);
            // average of 1.0, 0.5, 0.0 = 0.5 -> 0.5 * 30 = 15
            var reward = calculator.CalculateReward(1.0, 0.5, 0.0);
            Assert.That(reward, Is.EqualTo(15));
        }

        [Test]
        public void OutOfRangeFractions_AreClamped()
        {
            var calculator = new LevelRewardCalculator(baseReward: 0, efficiencyBonusMultiplier: 30);
            var reward = calculator.CalculateReward(2.0, -1.0, 1.0); // clamps to 1, 0, 1 -> avg 0.667
            Assert.That(reward, Is.EqualTo(20));
        }
    }
}
