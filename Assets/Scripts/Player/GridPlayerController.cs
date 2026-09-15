using System.Collections;
using System.Collections.Generic;
using AriadnesThread.Core.Generation;
using AriadnesThread.Core.Grid;
using AriadnesThread.View;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AriadnesThread.Player
{
    /// <summary>
    /// Tap-to-move: a tap on a reachable tile paths there via the same graph the maze
    /// was validated with, then walks it cell by cell. Each cell crossing increments the
    /// step counter — the unit every resource economy (markers, torch fuel) is defined in.
    /// </summary>
    public class GridPlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 4f;

        private MazeLevel _level;
        private float _cellSize;
        private CellCoord _currentCell;
        private bool _isMoving;

        public int StepCount { get; private set; }
        public CellCoord CurrentCell => _currentCell;

        /// <summary>Set once the player has visited the key room. Never resets — a level attempt is one continuous run.</summary>
        public bool HasKey { get; private set; }

        /// <summary>Fires once per cell crossing — the tick unit every step-based resource (torch fuel, markers) uses.</summary>
        public event System.Action OnStepTaken;

        public void Initialize(MazeLevel level, float cellSize)
        {
            _level = level;
            _cellSize = cellSize;
            _currentCell = level.Start;
        }

        private void Update()
        {
            if (_isMoving) return;
            if (!TryGetTapWorldPosition(out var worldPos)) return;

            var targetCell = WorldToCell(worldPos);
            if (!_level.Grid.InBounds(targetCell)) return;

            var path = GraphAnalyzer.ShortestPath(_level.Grid, _currentCell, targetCell);
            if (path == null || path.Count < 2) return;

            StartCoroutine(FollowPath(path));
        }

        private bool TryGetTapWorldPosition(out Vector3 worldPos)
        {
            worldPos = default;
            Vector2 screenPos;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                screenPos = Mouse.current.position.ReadValue();
            else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
            else
                return false;

            var cam = Camera.main;
            if (cam == null) return false;

            var ray = cam.ScreenPointToRay(screenPos);
            if (!Physics.Raycast(ray, out var hit, 200f)) return false;

            worldPos = hit.point;
            return true;
        }

        private CellCoord WorldToCell(Vector3 worldPos) =>
            new CellCoord(Mathf.RoundToInt(worldPos.x / _cellSize), Mathf.RoundToInt(worldPos.z / _cellSize));

        private IEnumerator FollowPath(List<CellCoord> path)
        {
            _isMoving = true;

            for (int i = 1; i < path.Count; i++)
            {
                if (IsEdgeBlocked(_currentCell, path[i]))
                    break; // locked door without the key, or a timed gate currently closed

                var targetWorld = MazeView.CellToWorld(path[i], _cellSize) + Vector3.up;
                while (Vector3.Distance(transform.position, targetWorld) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetWorld, moveSpeed * Time.deltaTime);
                    yield return null;
                }

                transform.position = targetWorld;
                _currentCell = path[i];
                StepCount++;

                if (_level.KeyLock != null && _currentCell.Equals(_level.KeyLock.KeyRoom))
                    HasKey = true;

                OnStepTaken?.Invoke();
            }

            _isMoving = false;
        }

        private bool IsEdgeBlocked(CellCoord from, CellCoord to)
        {
            if (_level.KeyLock != null && !HasKey && _level.KeyLock.IsLockedEdge(from, to))
                return true;

            if (_level.TimedGate != null && _level.TimedGate.IsGateEdge(from, to) && !_level.TimedGate.IsOpen)
                return true;

            return false;
        }
    }
}
