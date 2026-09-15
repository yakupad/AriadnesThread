using AriadnesThread.Core.Economy;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AriadnesThread.Player
{
    /// <summary>
    /// Represents vision radius as a literal point light, per the tech plan — no separate
    /// fog-of-war/tile-masking shader for this prototype. Manual toggle only; nothing here
    /// ever forces the torch on, including the tension state machine.
    /// </summary>
    [RequireComponent(typeof(GridPlayerController))]
    public class PlayerTorch : MonoBehaviour
    {
        [SerializeField] private int startingFuel = 80;
        [SerializeField] private int maxFuel = 120;
        [SerializeField] private float baseVisionRadius = 1f;
        [SerializeField] private float litVisionRadius = 4.5f;
        [SerializeField] private float cellSize = 3f;

        private Light _light;

        public TorchEconomy Economy { get; private set; }

        private void Awake()
        {
            Economy = new TorchEconomy(startingFuel, maxFuel, baseVisionRadius, litVisionRadius);

            var lightGO = new GameObject("TorchLight");
            lightGO.transform.SetParent(transform);
            lightGO.transform.localPosition = Vector3.up * 0.5f;
            _light = lightGO.AddComponent<Light>();
            _light.type = LightType.Point;
            _light.color = new Color(1f, 0.75f, 0.45f);
            _light.intensity = 1.5f;

            GetComponent<GridPlayerController>().OnStepTaken += HandleStep;
            UpdateLightRange();
        }

        private void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.tKey.wasPressedThisFrame) return;

            Economy.SetLit(!Economy.IsLit);
            UpdateLightRange();
        }

        private void HandleStep()
        {
            Economy.OnStep();
            UpdateLightRange();
        }

        private void UpdateLightRange() => _light.range = Economy.CurrentVisionRadius * cellSize;
    }
}
