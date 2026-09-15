using AriadnesThread.Core.Tension;
using NUnit.Framework;

namespace AriadnesThread.Core.Tests
{
    public class TensionStateMachineTests
    {
        [Test]
        public void EnteringDangerZone_FromCalm_GoesToAlertImmediately()
        {
            var fsm = new TensionStateMachine();
            fsm.Tick(deltaTime: 0.016f, withinDangerZone: true, seenByGuard: false, nearestSeenGuardDistance: 0f);
            Assert.That(fsm.State, Is.EqualTo(TensionState.Alert));
        }

        [Test]
        public void LeavingDangerZone_StaysAlertUntilCalmDownTimerElapses()
        {
            var fsm = new TensionStateMachine(calmDownSeconds: 1f, searchSeconds: 4f, captureRadius: 0.75f);
            fsm.Tick(0.016f, withinDangerZone: true, seenByGuard: false, nearestSeenGuardDistance: 0f);

            fsm.Tick(0.5f, withinDangerZone: false, seenByGuard: false, nearestSeenGuardDistance: 0f);
            Assert.That(fsm.State, Is.EqualTo(TensionState.Alert), "Should not calm down before the timer elapses.");

            fsm.Tick(0.6f, withinDangerZone: false, seenByGuard: false, nearestSeenGuardDistance: 0f);
            Assert.That(fsm.State, Is.EqualTo(TensionState.Calm));
        }

        [Test]
        public void ReenteringDangerZone_ResetsTheCalmDownTimer()
        {
            var fsm = new TensionStateMachine(calmDownSeconds: 1f, searchSeconds: 4f, captureRadius: 0.75f);
            fsm.Tick(0.016f, withinDangerZone: true, seenByGuard: false, nearestSeenGuardDistance: 0f);

            fsm.Tick(0.9f, withinDangerZone: false, seenByGuard: false, nearestSeenGuardDistance: 0f);
            fsm.Tick(0.016f, withinDangerZone: true, seenByGuard: false, nearestSeenGuardDistance: 0f); // back in before 1s elapsed
            fsm.Tick(0.9f, withinDangerZone: false, seenByGuard: false, nearestSeenGuardDistance: 0f);   // would have tripped without the reset

            Assert.That(fsm.State, Is.EqualTo(TensionState.Alert));
        }

        [Test]
        public void GuardSight_FromAlert_GoesToChase()
        {
            var fsm = new TensionStateMachine();
            fsm.Tick(0.016f, withinDangerZone: true, seenByGuard: false, nearestSeenGuardDistance: 0f);
            fsm.Tick(0.016f, withinDangerZone: true, seenByGuard: true, nearestSeenGuardDistance: 3f);
            Assert.That(fsm.State, Is.EqualTo(TensionState.Chase));
        }

        [Test]
        public void LosingSight_DuringChase_FallsBackToAlertAfterSearchTimer()
        {
            var fsm = new TensionStateMachine(calmDownSeconds: 1f, searchSeconds: 2f, captureRadius: 0.75f);
            fsm.Tick(0.016f, true, true, 3f); // -> Chase

            fsm.Tick(1.5f, withinDangerZone: true, seenByGuard: false, nearestSeenGuardDistance: 0f);
            Assert.That(fsm.State, Is.EqualTo(TensionState.Chase), "Should still be searching before the timer elapses.");

            fsm.Tick(0.6f, withinDangerZone: true, seenByGuard: false, nearestSeenGuardDistance: 0f);
            Assert.That(fsm.State, Is.EqualTo(TensionState.Alert));
        }

        [Test]
        public void GuardWithinCaptureRadius_DuringChase_Catches()
        {
            var fsm = new TensionStateMachine(captureRadius: 0.75f);
            fsm.Tick(0.016f, true, true, 3f); // -> Chase
            fsm.Tick(0.016f, true, true, 0.5f); // close enough
            Assert.That(fsm.State, Is.EqualTo(TensionState.Caught));
        }

        [Test]
        public void Caught_IsTerminal()
        {
            var fsm = new TensionStateMachine(captureRadius: 0.75f);
            fsm.Tick(0.016f, true, true, 3f);
            fsm.Tick(0.016f, true, true, 0.1f); // -> Caught

            fsm.Tick(1f, withinDangerZone: false, seenByGuard: false, nearestSeenGuardDistance: 0f);
            Assert.That(fsm.State, Is.EqualTo(TensionState.Caught));
        }
    }
}
