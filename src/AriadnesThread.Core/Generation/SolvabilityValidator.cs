using System.Collections.Generic;
using AriadnesThread.Core.Grid;

namespace AriadnesThread.Core.Generation
{
    /// <summary>
    /// Confirms start-to-exit reachability. When a key/lock is present, a plain BFS over
    /// cells would give a false positive ("reachable" through a door that actually needs
    /// a key first) — so the search state is (cell, hasKey) instead of just cell.
    /// </summary>
    public static class SolvabilityValidator
    {
        public static bool IsSolvable(MazeGrid grid, CellCoord start, CellCoord exit, KeyLockPlacement? lockInfo)
        {
            var visited = new HashSet<(CellCoord cell, bool hasKey)>();
            var queue = new Queue<(CellCoord cell, bool hasKey)>();

            var startState = (start, false);
            visited.Add(startState);
            queue.Enqueue(startState);

            while (queue.Count > 0)
            {
                var (cell, hasKey) = queue.Dequeue();
                if (cell.Equals(exit)) return true;

                bool hasKeyHere = hasKey || (lockInfo != null && cell.Equals(lockInfo.KeyRoom));

                foreach (var dir in MazeGrid.AllDirections)
                {
                    if (!grid.IsOpen(cell, dir)) continue;
                    var next = MazeGrid.Move(cell, dir);
                    if (!grid.InBounds(next)) continue;

                    if (lockInfo != null && lockInfo.IsLockedEdge(cell, next) && !hasKeyHere)
                        continue;

                    var nextState = (next, hasKeyHere);
                    if (visited.Add(nextState))
                        queue.Enqueue(nextState);
                }
            }

            return false;
        }
    }
}
