using System;
using System.Collections.Generic;
using AriadnesThread.Core.Grid;

namespace AriadnesThread.Core.Generation
{
    /// <summary>
    /// Places a key room and a locked door on the main path, honoring the topological
    /// constraint that the key must be reachable before the locked door when walking
    /// from start to exit (i.e. the key room's attachment index &lt; the door's index).
    /// </summary>
    public static class KeyLockPlacer
    {
        public static KeyLockPlacement? Place(MazeGrid grid, GraphAnalysis analysis, Random rng, int minKeyLockDistance)
        {
            var mainPathSet = new HashSet<CellCoord>(analysis.MainPath);
            var mainPathIndex = new Dictionary<CellCoord, int>();
            for (int i = 0; i < analysis.MainPath.Count; i++)
                mainPathIndex[analysis.MainPath[i]] = i;

            var candidates = new List<(CellCoord keyRoom, int attachmentIndex)>();
            foreach (var deadEnd in analysis.DeadEnds)
            {
                if (mainPathSet.Contains(deadEnd)) continue;

                var attachment = FindNearestMainPathCell(grid, deadEnd, mainPathSet);
                if (attachment == null) continue;

                candidates.Add((deadEnd, mainPathIndex[attachment.Value]));
            }

            Shuffle(candidates, rng);

            foreach (var (keyRoom, attachmentIndex) in candidates)
            {
                int doorIndex = attachmentIndex + minKeyLockDistance;
                if (doorIndex >= analysis.MainPath.Count - 1) continue; // leave the final step to the exit unlocked

                var from = analysis.MainPath[doorIndex];
                var to = analysis.MainPath[doorIndex + 1];
                return new KeyLockPlacement(keyRoom, from, to);
            }

            return null;
        }

        private static CellCoord? FindNearestMainPathCell(MazeGrid grid, CellCoord start, HashSet<CellCoord> mainPathSet)
        {
            var visited = new HashSet<CellCoord> { start };
            var queue = new Queue<CellCoord>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (mainPathSet.Contains(current)) return current;

                foreach (var neighbor in grid.OpenNeighbors(current))
                {
                    if (!visited.Add(neighbor)) continue;
                    queue.Enqueue(neighbor);
                }
            }

            return null;
        }

        private static void Shuffle<T>(List<T> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
