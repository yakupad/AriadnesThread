namespace AriadnesThread.Core.Generation
{
    /// <summary>Tunable difficulty knobs for one level's generation pass.</summary>
    public sealed class LevelParameters
    {
        public int Width { get; set; } = 12;
        public int Height { get; set; } = 12;

        public bool IncludeGuard { get; set; } = true;
        public int MinPatrolLoopLength { get; set; } = 6;
        public int GuardVisionRadiusHops { get; set; } = 5;

        public bool IncludeKeyLock { get; set; }
        public int MinKeyLockDistance { get; set; } = 4;

        public bool IncludeTimedGate { get; set; }
        public int TimedGateOpenSteps { get; set; } = 6;
        public int TimedGateClosedSteps { get; set; } = 3;
        public int TimedGateZoneRadiusHops { get; set; } = 3;

        public int MinDeadEndBranchDepth { get; set; } = 3;

        public int MaxGenerationRetries { get; set; } = 20;
    }
}
