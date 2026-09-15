using System;

namespace AriadnesThread.Core.Generation
{
    public static class TimedGatePlacer
    {
        /// <summary>
        /// Places a timed gate on a main-path edge, away from the very start/exit so it's
        /// a genuine mid-route obstacle. Returns null if the path is too short for that.
        /// </summary>
        public static TimedGate? Place(GraphAnalysis analysis, Random rng, int openSteps, int closedSteps, int edgeMargin = 2)
        {
            int lastUsableIndex = analysis.MainPath.Count - 1 - edgeMargin;
            if (lastUsableIndex <= edgeMargin) return null;

            int index = edgeMargin + rng.Next(lastUsableIndex - edgeMargin);
            var from = analysis.MainPath[index];
            var to = analysis.MainPath[index + 1];
            return new TimedGate(from, to, openSteps, closedSteps);
        }
    }
}
