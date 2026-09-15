using System;
using AriadnesThread.Core.Grid;

namespace AriadnesThread.Core.Generation
{
    /// <summary>
    /// Runs the full per-level generation pipeline: base maze -> graph analysis ->
    /// patrol loop + guard buffer -> dead-end/key-lock tagging -> solvability check.
    /// If a pass fails the solvability check, it retries with the next seed step
    /// (seed, seed+1, seed+2, ...) rather than giving up.
    /// </summary>
    public static class MazePipeline
    {
        public static MazeLevel Generate(int seed, LevelParameters parameters)
        {
            var start = new CellCoord(0, 0);
            var exit = new CellCoord(parameters.Width - 1, parameters.Height - 1);

            for (int attempt = 0; attempt < parameters.MaxGenerationRetries; attempt++)
            {
                int trySeed = seed + attempt;
                var grid = MazeGenerator.Generate(parameters.Width, parameters.Height, trySeed);
                var analysis = GraphAnalyzer.Analyze(grid, start, exit);
                var rng = new Random(trySeed);

                PatrolLoop? patrol = parameters.IncludeGuard
                    ? Braider.CreatePatrolLoop(grid, rng, parameters.MinPatrolLoopLength)
                    : null;

                var guardBuffer = ZoneTagger.TagGuardBuffer(grid, patrol, parameters.GuardVisionRadiusHops);
                var deadEndClusters = ZoneTagger.TagDeadEndClusters(grid, analysis, parameters.MinDeadEndBranchDepth);

                KeyLockPlacement? keyLock = parameters.IncludeKeyLock
                    ? KeyLockPlacer.Place(grid, analysis, rng, parameters.MinKeyLockDistance)
                    : null;

                TimedGate? timedGate = parameters.IncludeTimedGate
                    ? TimedGatePlacer.Place(analysis, rng, parameters.TimedGateOpenSteps, parameters.TimedGateClosedSteps)
                    : null;
                var timedGateZone = ZoneTagger.TagTimedGateZone(grid, timedGate, parameters.TimedGateZoneRadiusHops);

                if (!SolvabilityValidator.IsSolvable(grid, start, exit, keyLock))
                    continue;

                return new MazeLevel(grid, start, exit, analysis, patrol, guardBuffer, deadEndClusters, keyLock, timedGate, timedGateZone, trySeed);
            }

            throw new InvalidOperationException(
                $"Could not generate a solvable {parameters.Width}x{parameters.Height} maze after {parameters.MaxGenerationRetries} attempts starting at seed {seed}.");
        }
    }
}
