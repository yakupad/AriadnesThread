using System;

namespace AriadnesThread.Core.Meta
{
    /// <summary>
    /// Base reward + an efficiency bonus from how much of each resource is left — the
    /// design doc's "kaynak tasarrufunu anlamlı kılan geri bildirim döngüsü".
    /// </summary>
    public sealed class LevelRewardCalculator
    {
        private readonly int _baseReward;
        private readonly double _efficiencyBonusMultiplier;

        public LevelRewardCalculator(int baseReward = 10, double efficiencyBonusMultiplier = 10.0)
        {
            _baseReward = baseReward;
            _efficiencyBonusMultiplier = efficiencyBonusMultiplier;
        }

        /// <param name="markerStockFraction">Remaining marker stock / max stock, at level end.</param>
        /// <param name="torchFuelFraction">Remaining torch fuel / max fuel.</param>
        /// <param name="echoChargeFraction">Remaining echo charges / max charges.</param>
        public int CalculateReward(double markerStockFraction, double torchFuelFraction, double echoChargeFraction)
        {
            double efficiency = (Clamp01(markerStockFraction) + Clamp01(torchFuelFraction) + Clamp01(echoChargeFraction)) / 3.0;
            return _baseReward + (int)Math.Round(efficiency * _efficiencyBonusMultiplier);
        }

        private static double Clamp01(double v) => Math.Min(1.0, Math.Max(0.0, v));
    }
}
