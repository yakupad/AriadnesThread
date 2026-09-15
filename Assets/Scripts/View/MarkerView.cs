using System.Collections.Generic;
using System.Linq;
using AriadnesThread.Core.Economy;
using AriadnesThread.Core.Grid;
using UnityEngine;

namespace AriadnesThread.View
{
    /// <summary>Syncs a thin colored disk per active marker; pulses white as it nears expiry.</summary>
    public class MarkerView : MonoBehaviour
    {
        private static readonly Color RedColor = new Color(0.8f, 0.2f, 0.2f);
        private static readonly Color GreenColor = new Color(0.25f, 0.75f, 0.3f);

        [SerializeField] private int fadeWarningSteps = 10;

        private MarkerEconomy _economy;
        private float _cellSize;
        private readonly Dictionary<CellCoord, GameObject> _visuals = new Dictionary<CellCoord, GameObject>();

        public void Initialize(MarkerEconomy economy, float cellSize)
        {
            _economy = economy;
            _cellSize = cellSize;
        }

        private void Update()
        {
            if (_economy == null) return;

            var current = _economy.MarkedCells;

            List<CellCoord> stale = null;
            foreach (var cell in _visuals.Keys)
            {
                if (!current.Contains(cell))
                    (stale ??= new List<CellCoord>()).Add(cell);
            }
            if (stale != null)
            {
                foreach (var cell in stale)
                {
                    Destroy(_visuals[cell]);
                    _visuals.Remove(cell);
                }
            }

            foreach (var cell in current)
            {
                if (!_visuals.TryGetValue(cell, out var go))
                {
                    go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    go.name = $"Marker_{cell}";
                    go.transform.SetParent(transform);
                    go.transform.position = MazeView.CellToWorld(cell, _cellSize) + Vector3.up * 0.05f;
                    go.transform.localScale = new Vector3(_cellSize * 0.35f, 0.02f, _cellSize * 0.35f);
                    Destroy(go.GetComponent<Collider>());
                    _visuals[cell] = go;
                }

                var baseColor = _economy.GetColor(cell) == MarkerColor.Red ? RedColor : GreenColor;
                var tint = _economy.IsFading(cell, fadeWarningSteps)
                    ? Color.Lerp(baseColor, Color.white, Mathf.PingPong(Time.time * 4f, 1f))
                    : baseColor;

                go.GetComponent<Renderer>().material.color = tint;
            }
        }
    }
}
