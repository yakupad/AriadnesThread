using System.Collections.Generic;
using AriadnesThread.Core.Generation;
using AriadnesThread.Core.Grid;
using AriadnesThread.View;
using UnityEngine;

namespace AriadnesThread.Guard
{
    /// <summary>
    /// Patrols the braided loop from generation; when the tension director says the player
    /// is in Chase, it re-paths toward the player's cell instead (same deterministic grid
    /// graph, not NavMesh — see the tech plan's determinism note).
    /// </summary>
    public class GuardController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float visionRangeCells = 5.5f;
        [SerializeField] private float visionAngleDegrees = 90f;

        private MazeLevel _level;
        private float _cellSize;
        private IReadOnlyList<CellCoord> _patrolLoop;
        private int _patrolIndex;
        private CellCoord _currentCell;

        private List<CellCoord> _chasePath;
        private int _chasePathIndex;

        public void Initialize(MazeLevel level, float cellSize)
        {
            _level = level;
            _cellSize = cellSize;
            _patrolLoop = level.Patrol.Cells;
            _patrolIndex = 0;
            _currentCell = _patrolLoop[0];
            transform.position = MazeView.CellToWorld(_currentCell, cellSize) + Vector3.up;
        }

        /// <summary>Range/angle/occlusion check against the player. <paramref name="distance"/> is
        /// always the straight-line distance, even when sight fails — the caller (tension FSM)
        /// only reads it while sight succeeds.</summary>
        public bool TryGetLineOfSightToPlayer(Transform player, out float distance)
        {
            distance = Vector3.Distance(transform.position, player.position);
            if (distance > visionRangeCells * _cellSize) return false;

            var toPlayer = player.position - transform.position;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude < 0.0001f) return true; // player is standing on the guard

            var flatForward = transform.forward;
            flatForward.y = 0f;
            if (Vector3.Angle(flatForward, toPlayer) > visionAngleDegrees * 0.5f) return false;

            var origin = transform.position + Vector3.up * 0.5f;
            var target = player.position + Vector3.up * 0.5f;
            if (Physics.Linecast(origin, target, out var hit) && hit.transform != player)
                return false; // a wall (or anything else) is in the way

            return true;
        }

        public void TickPatrol()
        {
            _chasePath = null;
            var next = _patrolLoop[(_patrolIndex + 1) % _patrolLoop.Count];
            MoveTowardCell(next, advancingPatrol: true);
        }

        public void TickChase(CellCoord playerCell)
        {
            bool needsNewPath = _chasePath == null
                || _chasePathIndex >= _chasePath.Count
                || !_chasePath[_chasePath.Count - 1].Equals(playerCell);

            if (needsNewPath)
            {
                _chasePath = GraphAnalyzer.ShortestPath(_level.Grid, _currentCell, playerCell);
                _chasePathIndex = 1; // index 0 is the guard's current cell
            }

            if (_chasePath != null && _chasePathIndex < _chasePath.Count)
                MoveTowardCell(_chasePath[_chasePathIndex], advancingPatrol: false);
        }

        private void MoveTowardCell(CellCoord targetCell, bool advancingPatrol)
        {
            var targetWorld = MazeView.CellToWorld(targetCell, _cellSize) + Vector3.up;
            transform.position = Vector3.MoveTowards(transform.position, targetWorld, moveSpeed * Time.deltaTime);

            var facing = targetWorld - transform.position;
            facing.y = 0f;
            if (facing.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(facing);

            if (Vector3.Distance(transform.position, targetWorld) < 0.05f)
            {
                _currentCell = targetCell;
                if (advancingPatrol)
                    _patrolIndex = (_patrolIndex + 1) % _patrolLoop.Count;
                else
                    _chasePathIndex++;
            }
        }
    }
}
