using System;
using System.Collections.Generic;
using AriadnesThread.Core.Grid;

namespace AriadnesThread.Core.Economy
{
    /// <summary>
    /// Limited breadcrumb stock. Which cells are valid placement spots (junctions/dead-ends)
    /// is a maze-structure question the caller answers, not this class's concern — same split
    /// as TorchEconomy staying ignorant of the maze grid.
    /// </summary>
    public sealed class MarkerEconomy
    {
        private sealed class Marker
        {
            public MarkerColor Color;
            public int StepsRemaining;
        }

        private readonly Dictionary<CellCoord, Marker> _markers = new Dictionary<CellCoord, Marker>();
        private readonly int _lifetimeSteps;

        public int Stock { get; private set; }
        public int MaxStock { get; }

        public MarkerEconomy(int startingStock, int maxStock, int lifetimeSteps)
        {
            Stock = startingStock;
            MaxStock = maxStock;
            _lifetimeSteps = lifetimeSteps;
        }

        public IReadOnlyCollection<CellCoord> MarkedCells => _markers.Keys;

        public MarkerColor? GetColor(CellCoord cell) => _markers.TryGetValue(cell, out var m) ? m.Color : (MarkerColor?)null;

        public bool IsFading(CellCoord cell, int warningThresholdSteps) =>
            _markers.TryGetValue(cell, out var m) && m.StepsRemaining <= warningThresholdSteps;

        /// <summary>
        /// Placing on an empty cell costs 1 from stock. Re-marking an already-marked cell is
        /// a free color toggle — correcting a mistaken marker should never be penalized.
        /// </summary>
        public bool TryPlaceOrRecolor(CellCoord cell, MarkerColor color)
        {
            if (_markers.TryGetValue(cell, out var existing))
            {
                existing.Color = color;
                return true;
            }

            if (Stock <= 0) return false;

            Stock--;
            _markers[cell] = new Marker { Color = color, StepsRemaining = _lifetimeSteps };
            return true;
        }

        /// <summary>A deliberate pickup — unlike passive decay, this refunds stock.</summary>
        public bool TryRetrieve(CellCoord cell)
        {
            if (!_markers.Remove(cell)) return false;
            Stock = Math.Min(MaxStock, Stock + 1);
            return true;
        }

        public void AddStock(int amount) => Stock = Math.Min(MaxStock, Stock + amount);

        /// <summary>Ages every marker by one step; expired ones vanish without refunding stock.</summary>
        public void OnStep()
        {
            List<CellCoord>? expired = null;

            foreach (var kvp in _markers)
            {
                kvp.Value.StepsRemaining--;
                if (kvp.Value.StepsRemaining <= 0)
                    (expired ??= new List<CellCoord>()).Add(kvp.Key);
            }

            if (expired == null) return;
            foreach (var cell in expired)
                _markers.Remove(cell);
        }
    }
}
