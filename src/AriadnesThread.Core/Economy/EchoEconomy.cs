using System;
using System.Collections.Generic;
using AriadnesThread.Core.Grid;

namespace AriadnesThread.Core.Economy
{
    /// <summary>
    /// A discrete-charge burst ability — unlike torch fuel, it isn't a continuous drain, it's
    /// spent per use. Reveals only static structure (which cells exist) and never a guard's
    /// position: that split between "structural info" and "immediate danger info" is deliberate.
    /// </summary>
    public sealed class EchoEconomy
    {
        private readonly Dictionary<CellCoord, int> _revealedCells = new Dictionary<CellCoord, int>();
        private readonly int _radiusHops;

        public int Charges { get; private set; }
        public int MaxCharges { get; }
        public int RevealLifetimeSteps { get; }

        public EchoEconomy(int startingCharges, int maxCharges, int radiusHops, int revealLifetimeSteps)
        {
            Charges = startingCharges;
            MaxCharges = maxCharges;
            _radiusHops = radiusHops;
            RevealLifetimeSteps = revealLifetimeSteps;
        }

        public IReadOnlyCollection<CellCoord> RevealedCells => _revealedCells.Keys;

        public int GetRemainingLifetime(CellCoord cell) => _revealedCells.TryGetValue(cell, out var v) ? v : 0;

        public bool TryUse(MazeGrid grid, CellCoord origin)
        {
            if (Charges <= 0) return false;

            Charges--;
            foreach (var cell in GridQuery.CellsWithinHops(grid, origin, _radiusHops))
                _revealedCells[cell] = RevealLifetimeSteps;

            return true;
        }

        public void AddCharge(int amount = 1) => Charges = Math.Min(MaxCharges, Charges + amount);

        public void OnStep()
        {
            List<CellCoord>? expired = null;

            foreach (var cell in new List<CellCoord>(_revealedCells.Keys))
            {
                var remaining = _revealedCells[cell] - 1;
                if (remaining <= 0)
                    (expired ??= new List<CellCoord>()).Add(cell);
                else
                    _revealedCells[cell] = remaining;
            }

            if (expired == null) return;
            foreach (var cell in expired)
                _revealedCells.Remove(cell);
        }
    }
}
