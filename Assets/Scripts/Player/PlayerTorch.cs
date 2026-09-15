using AriadnesThread.Audio;
using AriadnesThread.Core.Economy;
using AriadnesThread.View;
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
    [RequireComponent(typeof(AudioSource))]
    public class PlayerTorch : MonoBehaviour
    {
        [SerializeField] private int startingFuel = 80;
        [SerializeField] private int maxFuel = 120;
        [SerializeField] private float baseVisionRadius = 1f;
        [SerializeField] private float litVisionRadius = 4.5f;
        [SerializeField] private float cellSize = 3f;
        [SerializeField] private float flickerAmount = 0.4f;

        private Light _light;
        private AudioSource _audio;
        private ParticleSystem _flame;
        private float _baseLightIntensity;

        public TorchEconomy Economy { get; private set; }

        private void Awake()
        {
            Economy = new TorchEconomy(startingFuel, maxFuel, baseVisionRadius, litVisionRadius);
            _audio = GetComponent<AudioSource>();

            var lightGO = new GameObject("TorchLight");
            lightGO.transform.SetParent(transform);
            lightGO.transform.localPosition = Vector3.up * 0.5f;
            _light = lightGO.AddComponent<Light>();
            _light.type = LightType.Point;
            _light.color = new Color(1f, 0.75f, 0.45f);
            _baseLightIntensity = 4f; // punchy against the now near-black ambient — see TensionAtmosphere
            _light.intensity = _baseLightIntensity;

            _flame = ParticleEffects.CreateContinuousFlame(lightGO.transform);

            GetComponent<GridPlayerController>().OnStepTaken += HandleStep;
            UpdateLightRange();
        }

        private void Update()
        {
            if (Economy.IsLit)
                _light.intensity = _baseLightIntensity + Mathf.PerlinNoise(Time.time * 8f, 0f) * flickerAmount;

            if (Keyboard.current == null || !Keyboard.current.tKey.wasPressedThisFrame) return;

            Economy.SetLit(!Economy.IsLit);
            _audio.PlayOneShot(Economy.IsLit ? SfxLibrary.TorchOn : SfxLibrary.TorchOff, 0.5f);
            if (Economy.IsLit) _flame.Play(); else _flame.Stop();
            UpdateLightRange();
        }

        private void HandleStep()
        {
            Economy.OnStep();
            UpdateLightRange();
            if (!Economy.IsLit) _flame.Stop(); // fuel ran out mid-step
        }

        private void UpdateLightRange() => _light.range = Economy.CurrentVisionRadius * cellSize;
    }
}
