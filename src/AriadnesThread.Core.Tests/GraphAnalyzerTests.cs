using AriadnesThread.Core.Generation;
using AriadnesThread.Core.Grid;
using NUnit.Framework;

namespace AriadnesThread.Core.Tests
{
    public class GraphAnalyzerTests
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void MainPath_ConnectsStartToExit(int seed)
        {
            var grid = MazeGenerator.Generate(10, 10, seed);
            var start = new CellCoord(0, 0);
            var exit = new CellCoord(9, 9);

            var analysis = GraphAnalyzer.Analyze(grid, start, exit);

            Assert.That(analysis.MainPath[0], Is.EqualTo(start));
            Assert.That(analysis.MainPath[^1], Is.EqualTo(exit));

            for (int i = 0; i < analysis.MainPath.Count - 1; i++)
            {
                bool adjacentAndOpen = false;
                foreach (var dir in MazeGrid.AllDirections)
                {
                    if (grid.IsOpen(analysis.MainPath[i], dir) &&
                        MazeGrid.Move(analysis.MainPath[i], dir).Equals(analysis.MainPath[i + 1]))
                    {
                        adjacentAndOpen = true;
                        break;
                    }
                }
                Assert.That(adjacentAndOpen, Is.True,
                    $"Main path step {i} -> {i + 1} is not a real open passage.");
            }
        }

        [Test]
        public void Junctions_HaveAtLeastThreeOpenPassages()
        {
            var grid = MazeGenerator.Generate(10, 10, seed: 5);
            var analysis = GraphAnalyzer.Analyze(grid, new CellCoord(0, 0), new CellCoord(9, 9));

            foreach (var junction in analysis.Junctions)
                Assert.That(grid.OpenDegree(junction), Is.GreaterThanOrEqualTo(3));
        }

        [Test]
        public void DeadEnds_HaveExactlyOneOpenPassage()
        {
            var grid = MazeGenerator.Generate(10, 10, seed: 5);
            var analysis = GraphAnalyzer.Analyze(grid, new CellCoord(0, 0), new CellCoord(9, 9));

            foreach (var deadEnd in analysis.DeadEnds)
                Assert.That(grid.OpenDegree(deadEnd), Is.EqualTo(1));
        }
    }
}
