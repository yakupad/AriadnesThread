using System.Collections.Generic;

namespace AriadnesThread.Core.Grid
{
    /// <summary>Shared read-only queries over a maze grid's connectivity.</summary>
    public static class GridQuery
    {
        /// <summary>Every cell reachable within <paramref name="maxHops"/> open-passage hops of <paramref name="start"/>.</summary>
        public static IEnumerable<CellCoord> CellsWithinHops(MazeGrid grid, CellCoord start, int maxHops)
        {
            var visited = new HashSet<CellCoord> { start };
            var frontier = new Queue<(CellCoord cell, int depth)>();
            frontier.Enqueue((start, 0));

            while (frontier.Count > 0)
            {
                var (cell, depth) = frontier.Dequeue();
                yield return cell;
                if (depth >= maxHops) continue;

                foreach (var neighbor in grid.OpenNeighbors(cell))
                {
                    if (!visited.Add(neighbor)) continue;
                    frontier.Enqueue((neighbor, depth + 1));
                }
            }
        }
    }
}
