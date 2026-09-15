using AriadnesThread.Audio;
using UnityEngine;

namespace AriadnesThread.Player
{
    [RequireComponent(typeof(GridPlayerController))]
    [RequireComponent(typeof(AudioSource))]
    public class PlayerAudio : MonoBehaviour
    {
        private AudioSource _source;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            // Safe in Awake (unlike MarkerPlacer/EchoCaster): PlayStep is this instance's own
            // method, never null, regardless of Initialize() ordering.
            GetComponent<GridPlayerController>().OnStepTaken += PlayStep;
        }

        private void PlayStep() => _source.PlayOneShot(SfxLibrary.Step, 0.5f);
    }
}
