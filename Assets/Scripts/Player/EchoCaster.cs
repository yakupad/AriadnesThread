using AriadnesThread.Audio;
using AriadnesThread.Core.Economy;
using AriadnesThread.Core.Generation;
using AriadnesThread.View;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AriadnesThread.Player
{
    /// <summary>E triggers a burst reveal of nearby structure — never the guard's position.
    /// Keyboard-only, so CastEcho() is the same action exposed for MazeBootstrap's on-screen button.</summary>
    [RequireComponent(typeof(GridPlayerController))]
    [RequireComponent(typeof(AudioSource))]
    public class EchoCaster : MonoBehaviour
    {
        [SerializeField] private int startingCharges = 2;
        [SerializeField] private int maxCharges = 5;
        [SerializeField] private int radiusHops = 4;
        [SerializeField] private int revealLifetimeSteps = 18;

        private MazeLevel _level;
        private GridPlayerController _controller;
        private AudioSource _audio;

        public EchoEconomy Economy { get; private set; }

        public void Initialize(MazeLevel level)
        {
            _level = level;
            Economy = new EchoEconomy(startingCharges, maxCharges, radiusHops, revealLifetimeSteps);
            // Not in Awake(): see MarkerPlacer.Initialize for why this can't subscribe there.
            _controller.OnStepTaken += Economy.OnStep;
        }

        private void Awake()
        {
            _controller = GetComponent<GridPlayerController>();
            _audio = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                CastEcho();
        }

        public void CastEcho()
        {
            if (!Economy.TryUse(_level.Grid, _controller.CurrentCell)) return;

            _audio.PlayOneShot(SfxLibrary.EchoPing, 0.6f);
            ParticleEffects.SpawnBurst(transform.position, new Color(0.4f, 0.85f, 0.9f), count: 40, speed: 5f, lifetime: 0.5f, size: 0.08f);
        }
    }
}
