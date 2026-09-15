using AriadnesThread.Core.Generation;
using AriadnesThread.Core.Tension;
using AriadnesThread.Guard;
using AriadnesThread.Player;
using UnityEngine;

namespace AriadnesThread.Bootstrap
{
    /// <summary>
    /// Feeds the engine-agnostic TensionStateMachine its per-frame inputs and reacts to the
    /// result — switches the guard between patrol/chase and freezes the player on capture.
    /// The full retry/level-fail flow (M4/M5) isn't built yet; this is just enough to feel
    /// the state machine work end to end.
    /// </summary>
    public class TensionDirector : MonoBehaviour
    {
        private MazeLevel _level;
        private GridPlayerController _player;
        private GuardController _guard;
        private TensionStateMachine _fsm;

        public TensionState State => _fsm.State;

        public void Initialize(MazeLevel level, GridPlayerController player, GuardController guard)
        {
            _level = level;
            _player = player;
            _guard = guard;
            _fsm = new TensionStateMachine();
        }

        private void Update()
        {
            var cell = _player.CurrentCell;
            // "Wrong area" per the design doc is any of these three — a guard's territory,
            // a deep dead-end branch, or a timed-gate's vicinity. This OR is what the
            // "highest severity wins, every source must clear" rule needs from the caller.
            bool withinDangerZone = _level.GuardBuffer.Contains(cell)
                || _level.DeadEndClusters.Contains(cell)
                || _level.TimedGateZone.Contains(cell);
            bool seenByGuard = _guard.TryGetLineOfSightToPlayer(_player.transform, out var distance);

            var previousState = _fsm.State;
            _fsm.Tick(Time.deltaTime, withinDangerZone, seenByGuard, distance);

            if (_fsm.State != previousState)
                Debug.Log($"Tension: {previousState} -> {_fsm.State}");

            if (_fsm.State == TensionState.Chase)
                _guard.TickChase(_player.CurrentCell);
            else
                _guard.TickPatrol();

            if (_fsm.State == TensionState.Caught)
                _player.enabled = false;
        }
    }
}
