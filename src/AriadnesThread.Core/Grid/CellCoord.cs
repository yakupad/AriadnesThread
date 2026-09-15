using System;

namespace AriadnesThread.Core.Grid
{
    public readonly struct CellCoord : IEquatable<CellCoord>
    {
        public readonly int X;
        public readonly int Y;

        public CellCoord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(CellCoord other) => X == other.X && Y == other.Y;

        public override bool Equals(object obj) => obj is CellCoord other && Equals(other);

        public override int GetHashCode() => (X * 397) ^ Y;

        public override string ToString() => $"({X},{Y})";

        public static bool operator ==(CellCoord a, CellCoord b) => a.Equals(b);

        public static bool operator !=(CellCoord a, CellCoord b) => !a.Equals(b);
    }
}
