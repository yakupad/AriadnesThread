using AriadnesThread.Core.Generation;
using UnityEngine;

namespace AriadnesThread.View
{
    /// <summary>Colors the gate marker by its current open/closed phase.</summary>
    public class TimedGateView : MonoBehaviour
    {
        private static readonly Color OpenColor = new Color(0.4f, 0.8f, 0.45f, 0.5f);
        private static readonly Color ClosedColor = new Color(0.8f, 0.25f, 0.25f, 0.9f);

        private TimedGate _gate;
        private GameObject _marker;

        public void Initialize(TimedGate gate, GameObject marker)
        {
            _gate = gate;
            _marker = marker;
        }

        private void Update()
        {
            if (_gate == null || _marker == null) return;
            _marker.GetComponent<Renderer>().material.color = _gate.IsOpen ? OpenColor : ClosedColor;
        }
    }
}
