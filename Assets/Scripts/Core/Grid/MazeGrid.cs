using System;
using System.Collections.Generic;

namespace AriadnesThread.Core.Grid
{
    /// <summary>
    /// A rectangular grid of cells connected by passages. Only North/East openness is
    /// stored per cell; South/West are derived from the neighboring cell so each wall
    /// exists exactly once in the data.
    /// </summary>
    public sealed class MazeGrid
    {
        public int Width { get; }
        public int Height { get; }

        private readonly bool[,] _openNorth;
        private readonly bool[,] _openEast;

        public MazeGrid(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException(nameof(width), "Grid dimensions must be positive.");

            Width = width;
            Height = height;
            _openNorth = new bool[width, height];
            _openEast = new bool[width, height];
        }

        public bool InBounds(CellCoord c) => c.X >= 0 && c.X < Width && c.Y >= 0 && c.Y < Height;

        public static IReadOnlyList<Direction> AllDirections { get; } = new[]
        {
            Direction.North, Direction.East, Direction.South, Direction.West
        };

        public static Direction Opposite(Direction dir) => dir switch
        {
            Direction.North => Direction.South,
            Direction.South => Direction.North,
            Direction.East => Direction.West,
            Direction.West => Direction.East,
            _ => throw new ArgumentOutOfRangeException(nameof(dir))
        };

        public static CellCoord Move(CellCoord c, Direction dir) => dir switch
        {
            Direction.North => new CellCoord(c.X, c.Y + 1),
            Direction.South => new CellCoord(c.X, c.Y - 1),
            Direction.East => new CellCoord(c.X + 1, c.Y),
            Direction.West => new CellCoord(c.X - 1, c.Y),
            _ => throw new ArgumentOutOfRangeException(nameof(dir))
        };

        public bool IsOpen(CellCoord from, Direction dir)
        {
            if (!InBounds(from)) return false;

            switch (dir)
            {
                case Direction.North:
                    return _openNorth[from.X, from.Y];
                case Direction.East:
                    return _openEast[from.X, from.Y];
                case Direction.South:
                {
                    var south = Move(from, Direction.South);
                    return InBounds(south) && _openNorth[south.X, south.Y];
                }
                case Direction.West:
                {
                    var west = Move(from, Direction.West);
                    return InBounds(west) && _openEast[west.X, west.Y];
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(dir));
            }
        }

        /// <summary>Opens the passage between <paramref name="from"/> and its neighbor in <paramref name="dir"/>.</summary>
        public void Open(CellCoord from, Direction dir)
        {
            if (!InBounds(from))
                throw new ArgumentOutOfRangeException(nameof(from));

            var to = Move(from, dir);
            if (!InBounds(to))
                throw new ArgumentOutOfRangeException(nameof(dir), $"{from} has no neighbor to the {dir}.");

            switch (dir)
            {
                case Direction.North:
                    _openNorth[from.X, from.Y] = true;
                    break;
                case Direction.East:
                    _openEast[from.X, from.Y] = true;
                    break;
                case Direction.South:
                    _openNorth[to.X, to.Y] = true;
                    break;
                case Direction.West:
                    _openEast[to.X, to.Y] = true;
                    break;
            }
        }

        public IEnumerable<CellCoord> AllCells()
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    yield return new CellCoord(x, y);
        }

        /// <summary>Neighbors reachable through an open passage.</summary>
        public IEnumerable<CellCoord> OpenNeighbors(CellCoord c)
        {
            foreach (var dir in AllDirections)
            {
                if (!IsOpen(c, dir)) continue;
                var next = Move(c, dir);
                if (InBounds(next)) yield return next;
            }
        }

        /// <summary>Number of open passages out of a cell — 1 means dead end, 3+ means junction.</summary>
        public int OpenDegree(CellCoord c)
        {
            int count = 0;
            foreach (var dir in AllDirections)
                if (IsOpen(c, dir)) count++;
            return count;
        }
    }
}
