using System.Collections.Generic;
using AriadnesThread.Core.Grid;

namespace AriadnesThread.Core.Generation
{
    /// <summary>
    /// Generation-time zone tagging. These tags are a coarse "could plausibly be seen or
    /// is a risky dead end" over-approximation for level-design purposes (e.g. deciding where
    /// traps may overlap a guard's territory) — the runtime tension state machine still does
    /// its own precise cone-of-vision check against the guard's current facing direction.
    /// </summary>
    public static class ZoneTagger
    {
        /// <summary>
        /// Tags every cell reachable within <paramref name="visionRadiusHops"/> open-passage
        /// hops from any patrol cell as part of the guard's buffer zone.
        /// </summary>
        public static HashSet<CellCoord> TagGuardBuffer(MazeGrid grid, PatrolLoop? patrol, int visionRadiusHops)
        {
            var buffer = new HashSet<CellCoord>();
            if (patrol == null) return buffer;

            foreach (var patrolCell in patrol.Cells)
            {
                foreach (var reachable in GridQuery.CellsWithinHops(grid, patrolCell, visionRadiusHops))
                    buffer.Add(reachable);
            }

            return buffer;
        }

        /// <summary>Tags cells near a timed gate — entering the area builds tension, but (per
        /// design) a timed obstacle alone never escalates past Alert the way a guard does.</summary>
        public static HashSet<CellCoord> TagTimedGateZone(MazeGrid grid, TimedGate? gate, int radiusHops)
        {
            var zone = new HashSet<CellCoord>();
            if (gate == null) return zone;

            foreach (var cell in GridQuery.CellsWithinHops(grid, gate.From, radiusHops))
                zone.Add(cell);
            foreach (var cell in GridQuery.CellsWithinHops(grid, gate.To, radiusHops))
                zone.Add(cell);

            return zone;
        }

        /// <summary>
        /// Tags cells belonging to a dead-end branch (off the main path) whose depth is at
        /// least <paramref name="minBranchDepth"/> — short stubs aren't worth flagging.
        /// </summary>
        public static HashSet<CellCoord> TagDeadEndClusters(MazeGrid grid, GraphAnalysis analysis, int minBranchDepth)
        {
            var tagged = new HashSet<CellCoord>();
            var mainPathSet = new HashSet<CellCoord>(analysis.MainPath);

            foreach (var deadEnd in analysis.DeadEnds)
            {
                if (mainPathSet.Contains(deadEnd)) continue;

                var branch = new List<CellCoord> { deadEnd };
                var visited = new HashSet<CellCoord> { deadEnd };
                var current = deadEnd;

                // Walk back from the dead end until we hit the main path or a junction
                // (a junction means we've reached a branching point, not a single stub).
                while (true)
                {
                    CellCoord? next = null;
                    foreach (var neighbor in grid.OpenNeighbors(current))
                    {
                        if (visited.Contains(neighbor)) continue;
                        next = neighbor;
                        break;
                    }

                    if (next == null) break;
                    var nextCell = next.Value;

                    if (mainPathSet.Contains(nextCell) || analysis.Junctions.Contains(nextCell))
                        break;

                    branch.Add(nextCell);
                    visited.Add(nextCell);
                    current = nextCell;
                }

                if (branch.Count >= minBranchDepth)
                    foreach (var cell in branch)
                        tagged.Add(cell);
            }

            return tagged;
        }
    }
}
