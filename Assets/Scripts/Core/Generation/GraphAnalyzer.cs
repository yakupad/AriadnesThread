using System.Collections.Generic;
using AriadnesThread.Core.Grid;

namespace AriadnesThread.Core.Generation
{
    public static class GraphAnalyzer
    {
        public static GraphAnalysis Analyze(MazeGrid grid, CellCoord start, CellCoord exit)
        {
            var junctions = new HashSet<CellCoord>();
            var deadEnds = new HashSet<CellCoord>();

            foreach (var cell in grid.AllCells())
            {
                var degree = grid.OpenDegree(cell);
                if (degree >= 3) junctions.Add(cell);
                if (degree == 1) deadEnds.Add(cell);
            }

            var mainPath = ShortestPath(grid, start, exit)
                ?? throw new System.InvalidOperationException("Grid has no path between start and exit — generation invariant violated.");

            return new GraphAnalysis(mainPath, junctions, deadEnds);
        }

        /// <summary>BFS shortest path between two cells using only open passages. Null if unreachable.</summary>
        public static List<CellCoord>? ShortestPath(MazeGrid grid, CellCoord from, CellCoord to)
        {
            var cameFrom = new Dictionary<CellCoord, CellCoord>();
            var visited = new HashSet<CellCoord> { from };
            var queue = new Queue<CellCoord>();
            queue.Enqueue(from);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current.Equals(to))
                    return ReconstructPath(cameFrom, from, to);

                foreach (var next in grid.OpenNeighbors(current))
                {
                    if (!visited.Add(next)) continue;
                    cameFrom[next] = current;
                    queue.Enqueue(next);
                }
            }

            return null; // 'to' is unreachable from 'from'
        }

        private static List<CellCoord> ReconstructPath(Dictionary<CellCoord, CellCoord> cameFrom, CellCoord from, CellCoord to)
        {
            var path = new List<CellCoord> { to };
            var current = to;
            while (!current.Equals(from))
            {
                current = cameFrom[current];
                path.Add(current);
            }
            path.Reverse();
            return path;
        }
    }
}
