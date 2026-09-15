using AriadnesThread.Audio;
using AriadnesThread.CameraControl;
using AriadnesThread.Core.Tension;
using AriadnesThread.View;
using UnityEngine;

namespace AriadnesThread.Bootstrap
{
    /// <summary>
    /// Turns TensionDirector.State into feel: camera zoom, ambient light tint, a pitch/volume-
    /// shifting drone, and transition stingers. Consolidated in one place rather than scattered
    /// across camera/audio/lighting scripts, since they all key off the same single state.
    /// </summary>
    public class TensionAtmosphere : MonoBehaviour
    {
        private AudioSource sfxSource;
        private AudioSource droneSource;

        private TensionDirector _director;
        private Camera _camera;
        private IsometricCameraFollow _cameraFollow;
        private Transform _guardTransform;
        private float _baseOrthographicSize;
        private TensionState _previousState = TensionState.Calm;

        public void Initialize(TensionDirector director, Camera camera, Transform guardTransform, AudioSource sfxSource, AudioSource droneSource)
        {
            _director = director;
            _camera = camera;
            _cameraFollow = camera.GetComponent<IsometricCameraFollow>();
            _guardTransform = guardTransform;
            _baseOrthographicSize = camera.orthographicSize;
            this.sfxSource = sfxSource;
            this.droneSource = droneSource;

            droneSource.clip = SfxLibrary.Drone;
            droneSource.loop = true;
            droneSource.volume = 0f;
            droneSource.Play();
        }

        private void Update()
        {
            if (_director == null) return;

            var state = _director.State;
            if (state != _previousState)
            {
                OnTransition(state);
                _previousState = state;
            }

            ApplyPresentation(state);
        }

        private void OnTransition(TensionState enteringState)
        {
            switch (enteringState)
            {
                case TensionState.Chase:
                    sfxSource.PlayOneShot(SfxLibrary.GuardSpotted);
                    if (_guardTransform != null)
                        ParticleEffects.SpawnBurst(_guardTransform.position + Vector3.up * 1.5f, new Color(1f, 0.85f, 0.2f), count: 15, speed: 2f, lifetime: 0.4f, size: 0.1f);
                    break;
                case TensionState.Caught:
                    sfxSource.PlayOneShot(SfxLibrary.Caught);
                    if (_cameraFollow != null)
                        _cameraFollow.Shake(duration: 0.4f, magnitude: 0.6f);
                    break;
            }
        }

        private void ApplyPresentation(TensionState state)
        {
            var (zoomMultiplier, ambientTint, dronePitch, droneVolume) = state switch
            {
                TensionState.Calm => (1f, new Color(0.03f, 0.03f, 0.05f), 1f, 0.03f),
                TensionState.Alert => (0.9f, new Color(0.06f, 0.045f, 0.03f), 1.15f, 0.12f),
                TensionState.Chase => (0.75f, new Color(0.08f, 0.02f, 0.02f), 1.4f, 0.28f),
                TensionState.Caught => (0.75f, new Color(0.05f, 0.01f, 0.01f), 0.6f, 0.35f),
                _ => (1f, new Color(0.03f, 0.03f, 0.05f), 1f, 0.1f)
            };

            float lerpSpeed = Time.deltaTime * 3f;

            if (_camera != null)
                _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _baseOrthographicSize * zoomMultiplier, lerpSpeed);

            RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, ambientTint, lerpSpeed);
            RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, ambientTint, lerpSpeed);

            if (droneSource != null)
            {
                droneSource.pitch = Mathf.Lerp(droneSource.pitch, dronePitch, lerpSpeed);
                droneSource.volume = Mathf.Lerp(droneSource.volume, droneVolume, lerpSpeed);
            }
        }
    }
}
