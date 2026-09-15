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

        public void SetTarget(Transform target)
        {
            _target = target;
            transform.position = target.position + offset;
            transform.LookAt(target.position);
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            var desiredPosition = _target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothing * Time.deltaTime);
            transform.LookAt(_target.position);
        }
    }
}
