using System.Collections;
using UnityEngine;

namespace AriadnesThread.CameraControl
{
    /// <summary>
    /// A hand-rolled follow (no Cinemachine dependency yet — this is a deliberate scope
    /// cut from the original tech plan: a plain damped LookAt gets the same isometric feel
    /// for prototype purposes without pulling in an extra package to learn/wire up).
    /// </summary>
    public class IsometricCameraFollow : MonoBehaviour
    {
        [SerializeField] private Vector3 offset = new Vector3(0f, 12f, -12f);
        [SerializeField] private float smoothing = 5f;

        private Transform _target;
        private Vector3 _smoothedPosition;
        private Vector3 _shakeOffset;
        private Coroutine _shakeRoutine;

        public void SetTarget(Transform target)
        {
            _target = target;
            _smoothedPosition = target.position + offset;
            transform.position = _smoothedPosition;
            transform.LookAt(target.position);
        }

        /// <summary>Brief positional jitter on top of the follow — e.g. on getting caught.</summary>
        public void Shake(float duration, float magnitude)
        {
            if (_shakeRoutine != null) StopCoroutine(_shakeRoutine);
            _shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            var desiredPosition = _target.position + offset;
            // Shake is tracked separately from the smoothed base position, not folded into
            // it — otherwise the jitter would bleed into the lerp and drift the follow.
            _smoothedPosition = Vector3.Lerp(_smoothedPosition, desiredPosition, smoothing * Time.deltaTime);
            transform.position = _smoothedPosition + _shakeOffset;
            transform.LookAt(_target.position);
        }

        private IEnumerator ShakeRoutine(float duration, float magnitude)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                var random = Random.insideUnitSphere * magnitude;
                _shakeOffset = new Vector3(random.x, 0f, random.z);
                elapsed += Time.deltaTime;
                yield return null;
            }
            _shakeOffset = Vector3.zero;
        }
    }
}
