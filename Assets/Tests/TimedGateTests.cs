using System;
using AriadnesThread.Core.Generation;
using AriadnesThread.Core.Grid;
using NUnit.Framework;

namespace AriadnesThread.Core.Tests
{
    public class TimedGateTests
    {
        [Test]
        public void ClosedDurationNotShorterThanOpen_ThrowsAtConstruction()
        {
            Assert.Throws<ArgumentException>(() =>
                new TimedGate(new CellCoord(0, 0), new CellCoord(1, 0), openSteps: 2, closedSteps: 2));
        }

        [Test]
        public void IsOpen_CyclesOnStepCount_NotRealTime()
        {
            var gate = new TimedGate(new CellCoord(0, 0), new CellCoord(1, 0), openSteps: 3, closedSteps: 2);

            Assert.That(gate.IsOpen, Is.True, "Step 0 should be open.");

            gate.OnStep(); gate.OnStep(); // steps 1, 2 — still within the open window
            Assert.That(gate.IsOpen, Is.True);

            gate.OnStep(); // step 3 — closed window starts
            Assert.That(gate.IsOpen, Is.False);

            gate.OnStep(); // step 4 — still closed
            Assert.That(gate.IsOpen, Is.False);

            gate.OnStep(); // step 5 — cycle restarts, open again
            Assert.That(gate.IsOpen, Is.True);
        }

        [Test]
        public void IsGateEdge_MatchesBothDirections()
        {
            var a = new CellCoord(2, 3);
            var b = new CellCoord(2, 4);
            var gate = new TimedGate(a, b, openSteps: 5, closedSteps: 1);

            Assert.That(gate.IsGateEdge(a, b), Is.True);
            Assert.That(gate.IsGateEdge(b, a), Is.True);
            Assert.That(gate.IsGateEdge(a, new CellCoord(2, 2)), Is.False);
        }
    }
}
