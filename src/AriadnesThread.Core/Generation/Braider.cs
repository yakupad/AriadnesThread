using System;
using System.Collections.Generic;
using AriadnesThread.Core.Grid;

namespace AriadnesThread.Core.Generation
{
    /// <summary>
    /// A perfect maze (spanning tree) has no cycles, so a guard has nowhere to patrol.
    /// Opening exactly one more wall creates exactly one cycle in the tree — that cycle
    /// becomes the patrol loop. This is the "braiding" step referenced in the design doc.
    /// </summary>
    public static class Braider
    {
        /// <summary>
        /// Tries to open one wall that creates a cycle at least <paramref name="minLoopLength"/>
        /// cells long. Returns null if no candidate wall meets the minimum (rare on small grids).
        /// </summary>
        public static PatrolLoop? CreatePatrolLoop(MazeGrid grid, Random rng, int minLoopLength)
        {
            var candidates = new List<(CellCoord from, Direction dir)>();
            foreach (var cell in grid.AllCells())
            {
                foreach (var dir in MazeGrid.AllDirections)
                {
                    if (grid.IsOpen(cell, dir)) continue;
                    var neighbor = MazeGrid.Move(cell, dir);
                    if (!grid.InBounds(neighbor)) continue;

                    // Only consider each wall once (from the lower-indexed side) to avoid duplicates.
                    if (dir == Direction.South || dir == Direction.West) continue;
                    candidates.Add((cell, dir));
                }
            }

            Shuffle(candidates, rng);

            foreach (var (from, dir) in candidates)
            {
                var to = MazeGrid.Move(from, dir);
                var treePath = GraphAnalyzer.ShortestPath(grid, from, to);
                if (treePath == null) continue; // should not happen on a connected spanning tree

                // Closing the new edge onto the existing tree path forms the cycle.
                if (treePath.Count < minLoopLength) continue;

                grid.Open(from, dir);
                return new PatrolLoop(treePath);
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
