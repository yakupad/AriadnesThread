using AriadnesThread.Core.Economy;
using AriadnesThread.Core.Generation;
using AriadnesThread.Core.Grid;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AriadnesThread.Player
{
    /// <summary>
    /// G places/recolors a marker at the player's current cell (only valid on a junction or
    /// dead-end — see the marker economy rules); Shift+G forces the opposite of the suggested
    /// color; R retrieves one. Keyboard stand-ins for what would be a long-press + UI button
    /// on device — see PlayerTorch for the same simplification.
    /// </summary>
    [RequireComponent(typeof(GridPlayerController))]
    public class MarkerPlacer : MonoBehaviour
    {
        [SerializeField] private int startingStock = 3;
        [SerializeField] private int maxStock = 8;
        [SerializeField] private int lifetimeSteps = 50;

        private MazeLevel _level;
        private GridPlayerController _controller;

        public MarkerEconomy Economy { get; private set; }

        public void Initialize(MazeLevel level)
        {
            _level = level;
            Economy = new MarkerEconomy(startingStock, maxStock, lifetimeSteps);
            // Not in Awake(): AddComponent<T>() calls Awake() synchronously, before the
            // caller gets a chance to invoke Initialize() — subscribing there would close
            // over a still-null Economy and throw on the first step.
            _controller.OnStepTaken += Economy.OnStep;
        }

        private void Awake()
        {
            _controller = GetComponent<GridPlayerController>();
        }

        private void Update()
        {
            if (Keyboard.current == null) return;

            var current = _controller.CurrentCell;
            bool eligible = _level.Analysis.Junctions.Contains(current) || _level.Analysis.DeadEnds.Contains(current);

            if (eligible && Keyboard.current.gKey.wasPressedThisFrame)
            {
                var suggested = SuggestColor(current);
                var color = Keyboard.current.leftShiftKey.isPressed ? Opposite(suggested) : suggested;
                Economy.TryPlaceOrRecolor(current, color);
            }
            else if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                Economy.TryRetrieve(current);
            }
        }

        // Simplification: color by whether the CURRENT cell is itself a dead end, rather than
        // the fuller "which way did I come from" look-ahead the design doc describes.
        private MarkerColor SuggestColor(CellCoord cell) =>
            _level.Analysis.DeadEnds.Contains(cell) ? MarkerColor.Red : MarkerColor.Green;

        private static MarkerColor Opposite(MarkerColor color) =>
            color == MarkerColor.Red ? MarkerColor.Green : MarkerColor.Red;
    }
}
