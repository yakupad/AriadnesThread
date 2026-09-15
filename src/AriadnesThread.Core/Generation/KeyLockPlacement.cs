using AriadnesThread.Core.Grid;

namespace AriadnesThread.Core.Generation
{
    /// <summary>A key room and the locked door on the main path that requires it.</summary>
    public sealed class KeyLockPlacement
    {
        public CellCoord KeyRoom { get; }
        public CellCoord LockedDoorFrom { get; }
        public CellCoord LockedDoorTo { get; }

        public KeyLockPlacement(CellCoord keyRoom, CellCoord lockedDoorFrom, CellCoord lockedDoorTo)
        {
            KeyRoom = keyRoom;
            LockedDoorFrom = lockedDoorFrom;
            LockedDoorTo = lockedDoorTo;
        }

        public bool IsLockedEdge(CellCoord a, CellCoord b) =>
            (a.Equals(LockedDoorFrom) && b.Equals(LockedDoorTo)) ||
            (a.Equals(LockedDoorTo) && b.Equals(LockedDoorFrom));
    }
}
