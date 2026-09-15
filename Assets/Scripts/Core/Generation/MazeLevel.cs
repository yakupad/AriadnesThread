using System.Collections.Generic;
using AriadnesThread.Core.Grid;

namespace AriadnesThread.Core.Generation
{
    /// <summary>The fully tagged, validated output of one generation pass.</summary>
    public sealed class MazeLevel
    {
        public MazeGrid Grid { get; }
        public CellCoord Start { get; }
        public CellCoord Exit { get; }
        public GraphAnalysis Analysis { get; }
        public PatrolLoop? Patrol { get; }
        public HashSet<CellCoord> GuardBuffer { get; }
        public HashSet<CellCoord> DeadEndClusters { get; }
        public KeyLockPlacement? KeyLock { get; }
        public TimedGate? TimedGate { get; }
        public HashSet<CellCoord> TimedGateZone { get; }
        public int Seed { get; }

        public MazeLevel(
            MazeGrid grid,
            CellCoord start,
            CellCoord exit,
            GraphAnalysis analysis,
            PatrolLoop? patrol,
            HashSet<CellCoord> guardBuffer,
            HashSet<CellCoord> deadEndClusters,
            KeyLockPlacement? keyLock,
            TimedGate? timedGate,
            HashSet<CellCoord> timedGateZone,
            int seed)
        {
            Grid = grid;
            Start = start;
            Exit = exit;
            Analysis = analysis;
            Patrol = patrol;
            GuardBuffer = guardBuffer;
            DeadEndClusters = deadEndClusters;
            KeyLock = keyLock;
            TimedGate = timedGate;
            TimedGateZone = timedGateZone;
            Seed = seed;
        }
    }
}
