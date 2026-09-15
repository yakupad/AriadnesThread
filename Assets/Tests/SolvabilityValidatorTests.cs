using AriadnesThread.Core.Generation;
using AriadnesThread.Core.Grid;
using NUnit.Framework;

namespace AriadnesThread.Core.Tests
{
    public class SolvabilityValidatorTests
    {
        // A simple 4-cell line: (0,0) - (1,0) - (2,0) - (3,0), all connected.
        private static MazeGrid BuildLineOfFour()
        {
            var grid = new MazeGrid(4, 1);
            grid.Open(new CellCoord(0, 0), Direction.East);
            grid.Open(new CellCoord(1, 0), Direction.East);
            grid.Open(new CellCoord(2, 0), Direction.East);
            return grid;
        }

        [Test]
        public void NoLock_ConnectedGrid_IsSolvable()
        {
            var grid = BuildLineOfFour();
            var solvable = SolvabilityValidator.IsSolvable(grid, new CellCoord(0, 0), new CellCoord(3, 0), lockInfo: null);
            Assert.That(solvable, Is.True);
        }

        [Test]
        public void KeyBeforeLock_IsSolvable()
        {
            var grid = BuildLineOfFour();
            // Key sits at (1,0), lock is the edge after it — key is always picked up in time.
            var lockInfo = new KeyLockPlacement(new CellCoord(1, 0), new CellCoord(2, 0), new CellCoord(3, 0));

            var solvable = SolvabilityValidator.IsSolvable(grid, new CellCoord(0, 0), new CellCoord(3, 0), lockInfo);
            Assert.That(solvable, Is.True);
        }

        [Test]
        public void KeyBehindItsOwnLock_IsUnsolvable()
        {
            var grid = BuildLineOfFour();
            // Key sits at (3,0), but reaching it requires crossing the very edge it unlocks —
            // a plain (non-state-augmented) BFS would wrongly call this solvable.
            var lockInfo = new KeyLockPlacement(new CellCoord(3, 0), new CellCoord(2, 0), new CellCoord(3, 0));

            var solvable = SolvabilityValidator.IsSolvable(grid, new CellCoord(0, 0), new CellCoord(3, 0), lockInfo);
            Assert.That(solvable, Is.False);
        }
    }
}
