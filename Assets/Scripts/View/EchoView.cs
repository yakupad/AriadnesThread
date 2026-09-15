using System.Collections.Generic;
using System.Linq;
using AriadnesThread.Core.Economy;
using AriadnesThread.Core.Grid;
using UnityEngine;

namespace AriadnesThread.View
{
    /// <summary>Tints floor tiles the echo currently reveals; un-tints them once they expire.</summary>
    public class EchoView : MonoBehaviour
    {
        private static readonly Color RevealColor = new Color(0.4f, 0.85f, 0.9f);

        private EchoEconomy _economy;
        private MazeView _mazeView;
        private readonly HashSet<CellCoord> _tinted = new HashSet<CellCoord>();

        public void Initialize(EchoEconomy economy, MazeView mazeView)
        {
            _economy = economy;
            _mazeView = mazeView;
        }

        private void Update()
        {
            if (_economy == null) return;

            var current = _economy.RevealedCells;

            List<CellCoord> noLongerRevealed = null;
            foreach (var cell in _tinted)
            {
                if (current.Contains(cell)) continue;
                (noLongerRevealed ??= new List<CellCoord>()).Add(cell);
                if (_mazeView.FloorTiles.TryGetValue(cell, out var floor))
                    floor.GetComponent<Renderer>().material.color = MazeView.FloorColor;
            }
            if (noLongerRevealed != null)
                foreach (var cell in noLongerRevealed)
                    _tinted.Remove(cell);

            foreach (var cell in current)
            {
                if (!_mazeView.FloorTiles.TryGetValue(cell, out var floor)) continue;

                float lifeFraction = _economy.GetRemainingLifetime(cell) / (float)_economy.RevealLifetimeSteps;
                floor.GetComponent<Renderer>().material.color = Color.Lerp(MazeView.FloorColor, RevealColor, Mathf.Clamp01(lifeFraction));
                _tinted.Add(cell);
            }
        }
    }
}
