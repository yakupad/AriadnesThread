using System.Collections.Generic;
using AriadnesThread.Core.Grid;

namespace AriadnesThread.Core.Generation
{
    public sealed class GraphAnalysis
    {
        /// <summary>Shortest path from start to exit, inclusive, in order.</summary>
        public IReadOnlyList<CellCoord> MainPath { get; }

        /// <summary>Cells with 3 or more open passages.</summary>
        public HashSet<CellCoord> Junctions { get; }

        /// <summary>Cells with exactly 1 open passage.</summary>
        public HashSet<CellCoord> DeadEnds { get; }

        public GraphAnalysis(IReadOnlyList<CellCoord> mainPath, HashSet<CellCoord> junctions, HashSet<CellCoord> deadEnds)
        {
            MainPath = mainPath;
            Junctions = junctions;
            DeadEnds = deadEnds;
        }
    }
}
