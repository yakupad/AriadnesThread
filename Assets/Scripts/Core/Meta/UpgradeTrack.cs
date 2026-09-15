using System;

namespace AriadnesThread.Core.Meta
{
    /// <summary>
    /// One capacity upgrade track (marker/torch/echo taban kapasitesi). Cost grows ~1.5-2x
    /// per level per the design doc, so the first upgrade stays cheap while later ones don't
    /// trivialize the resource economy they're upgrading.
    /// </summary>
    public sealed class UpgradeTrack
    {
        public int Level { get; private set; }
        public int MaxLevel { get; }

        private readonly int _baseCost;
        private readonly double _costMultiplier;

        public UpgradeTrack(int maxLevel, int baseCost, double costMultiplier = 1.75, int startingLevel = 0)
        {
            MaxLevel = maxLevel;
            _baseCost = baseCost;
            _costMultiplier = costMultiplier;
            Level = startingLevel;
        }

        public bool IsMaxed => Level >= MaxLevel;

        public int NextCost => (int)Math.Round(_baseCost * Math.Pow(_costMultiplier, Level));

        public bool TryUpgrade(CrystalWallet wallet)
        {
            if (IsMaxed) return false;
            if (!wallet.TrySpend(NextCost)) return false;

            Level++;
            return true;
        }
    }
}
