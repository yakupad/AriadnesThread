using System.Collections.Generic;
using AriadnesThread.Core.Grid;

namespace AriadnesThread.Core.Generation
{
    /// <summary>An ordered, closed walk a guard patrols along.</summary>
    public sealed class PatrolLoop
    {
        public IReadOnlyList<CellCoord> Cells { get; }

        public PatrolLoop(IReadOnlyList<CellCoord> cells)
        {
            Cells = cells;
        }
    }
}
