namespace AriadnesThread.Core.Tension
{
    /// <summary>
    /// Calm -> Alert -> Chase -> Caught, per the design doc's tension rules. Engine-agnostic:
    /// the caller supplies deltaTime and the two booleans each tick, aggregated across every
    /// active zone/guard as a simple OR — which is exactly what the "highest severity wins,
    /// all sources must clear to de-escalate" rule needs, with no extra bookkeeping here.
    /// </summary>
    public sealed class TensionStateMachine
    {
        public TensionState State { get; private set; } = TensionState.Calm;

        private readonly float _calmDownSeconds;
        private readonly float _searchSeconds;
        private readonly float _captureRadius;

        private float _calmDownTimer;
        private float _searchTimer;

        public TensionStateMachine(float calmDownSeconds = 1.75f, float searchSeconds = 4f, float captureRadius = 0.75f)
        {
            _calmDownSeconds = calmDownSeconds;
            _searchSeconds = searchSeconds;
            _captureRadius = captureRadius;
        }

        /// <param name="deltaTime">Seconds since the last tick.</param>
        /// <param name="withinDangerZone">
        /// True while the player is inside a tension zone OR its exit-hysteresis buffer.
        /// Computing that buffer is the caller's job — this class only owns the timer.
        /// </param>
        /// <param name="seenByGuard">True while any guard's vision cone currently hits the player.</param>
        /// <param name="nearestSeenGuardDistance">
        /// Distance to the closest guard that currently sees the player. Ignored when
        /// <paramref name="seenByGuard"/> is false.
        /// </param>
        public void Tick(float deltaTime, bool withinDangerZone, bool seenByGuard, float nearestSeenGuardDistance)
        {
            switch (State)
            {
                case TensionState.Calm:
                    // Being directly seen outranks merely standing in a danger zone —
                    // jump straight to Chase rather than forcing a detour through Alert.
                    if (seenByGuard)
                    {
                        EnterChase();
                    }
                    else if (withinDangerZone)
                    {
                        EnterAlert();
                    }
                    break;

                case TensionState.Alert:
                    if (seenByGuard)
                    {
                        EnterChase();
                        break;
                    }

                    if (withinDangerZone)
                    {
                        _calmDownTimer = 0f;
                    }
                    else
                    {
                        _calmDownTimer += deltaTime;
                        if (_calmDownTimer >= _calmDownSeconds)
                            State = TensionState.Calm;
                    }
                    break;

                case TensionState.Chase:
                    if (seenByGuard)
                    {
                        _searchTimer = 0f;
                        if (nearestSeenGuardDistance <= _captureRadius)
                            State = TensionState.Caught;
                    }
                    else
                    {
                        _searchTimer += deltaTime;
                        if (_searchTimer >= _searchSeconds)
                            EnterAlert();
                    }
                    break;

                case TensionState.Caught:
                    break; // terminal — a new level/attempt builds a fresh state machine
            }
        }

        private void EnterAlert()
        {
            State = TensionState.Alert;
            _calmDownTimer = 0f;
        }

        private void EnterChase()
        {
            State = TensionState.Chase;
            _searchTimer = 0f;
        }
    }
}
