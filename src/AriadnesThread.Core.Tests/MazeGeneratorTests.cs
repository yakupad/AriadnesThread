using System.Collections.Generic;
using AriadnesThread.Core.Generation;
using AriadnesThread.Core.Grid;
using NUnit.Framework;

namespace AriadnesThread.Core.Tests
{
    public class MazeGeneratorTests
    {
        [TestCase(1)]
        [TestCase(42)]
        [TestCase(12345)]
        public void SameSeed_ProducesIdenticalMaze(int seed)
        {
            var a = MazeGenerator.Generate(10, 10, seed);
            var b = MazeGenerator.Generate(10, 10, seed);

            foreach (var cell in a.AllCells())
                foreach (var dir in MazeGrid.AllDirections)
                    Assert.That(b.IsOpen(cell, dir), Is.EqualTo(a.IsOpen(cell, dir)),
                        $"Mismatch at {cell} facing {dir} for seed {seed}");
        }

        [TestCase(1, 42)]
        [TestCase(42, 43)]
        public void DifferentSeeds_TypicallyProduceDifferentMazes(int seedA, int seedB)
        {
            var a = MazeGenerator.Generate(10, 10, seedA);
            var b = MazeGenerator.Generate(10, 10, seedB);

            bool anyDifference = false;
            foreach (var cell in a.AllCells())
            {
                foreach (var dir in MazeGrid.AllDirections)
                {
                    if (a.IsOpen(cell, dir) != b.IsOpen(cell, dir))
                    {
                        anyDifference = true;
                        break;
                    }
                }
            }

            Assert.That(anyDifference, Is.True, "Two different seeds produced byte-identical mazes.");
        }

        [Test]
        public void Generate_IsFullyConnected_EveryCellReachableFromOrigin()
        {
            var grid = MazeGenerator.Generate(15, 15, seed: 7);
            var origin = new CellCoord(0, 0);

            var visited = new HashSet<CellCoord> { origin };
            var queue = new Queue<CellCoord>();
            queue.Enqueue(origin);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var neighbor in grid.OpenNeighbors(current))
                    if (visited.Add(neighbor))
                        queue.Enqueue(neighbor);
            }

            Assert.That(visited.Count, Is.EqualTo(15 * 15), "Spanning tree should reach every cell exactly once.");
        }
    }
}
