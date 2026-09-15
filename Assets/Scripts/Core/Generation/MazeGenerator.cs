using System;
using System.Collections.Generic;
using AriadnesThread.Core.Grid;

namespace AriadnesThread.Core.Generation
{
    /// <summary>
    /// Recursive backtracker maze generation. Produces a spanning tree over the grid,
    /// which guarantees exactly one solution path between any two cells.
    /// </summary>
    public static class MazeGenerator
    {
        public static MazeGrid Generate(int width, int height, int seed)
        {
            var grid = new MazeGrid(width, height);
            var rng = new Random(seed);
            var visited = new bool[width, height];
            var stack = new Stack<CellCoord>();

            var start = new CellCoord(0, 0);
            visited[start.X, start.Y] = true;
            stack.Push(start);

            while (stack.Count > 0)
            {
                var current = stack.Peek();
                var unvisitedDirs = new List<Direction>();

                foreach (var dir in MazeGrid.AllDirections)
                {
                    var next = MazeGrid.Move(current, dir);
                    if (grid.InBounds(next) && !visited[next.X, next.Y])
                        unvisitedDirs.Add(dir);
                }

                if (unvisitedDirs.Count == 0)
                {
                    stack.Pop();
                    continue;
                }

                var chosenDir = unvisitedDirs[rng.Next(unvisitedDirs.Count)];
                var target = MazeGrid.Move(current, chosenDir);

                grid.Open(current, chosenDir);
                visited[target.X, target.Y] = true;
                stack.Push(target);
            }

            return grid;
        }
    }
}
