using System;
using AriadnesThread.Core.Grid;

namespace AriadnesThread.Core.Generation
{
    /// <summary>
    /// A periodically open/closed edge. The phase advances on player steps, not real time —
    /// a shared-seed daily challenge must give every player the identical obstacle timing
    /// regardless of when in real time they happen to reach it.
    /// </summary>
    public sealed class TimedGate
    {
        public CellCoord From { get; }
        public CellCoord To { get; }
        public int OpenSteps { get; }
        public int ClosedSteps { get; }

        private int _stepCounter;

        public TimedGate(CellCoord from, CellCoord to, int openSteps, int closedSteps)
        {
            if (openSteps <= closedSteps)
                throw new ArgumentException("Open duration must exceed closed duration, or the gate could stay closed forever.");

            From = from;
            To = to;
            OpenSteps = openSteps;
            ClosedSteps = closedSteps;
        }

        public bool IsOpen => _stepCounter % (OpenSteps + ClosedSteps) < OpenSteps;

        public bool IsGateEdge(CellCoord a, CellCoord b) =>
            (a.Equals(From) && b.Equals(To)) || (a.Equals(To) && b.Equals(From));

        public void OnStep() => _stepCounter++;
    }
}
