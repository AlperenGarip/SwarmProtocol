using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace SwarmProtocol.Camera
{
    /// <summary>
    /// Manages a Cinemachine virtual camera with right-mouse-button horizontal orbit.
    /// Hold RMB and move mouse left/right to rotate the camera around the player.
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public class CameraController : MonoBehaviour
    {
        [Header("Follow Target")]
        [SerializeField] private Transform target;

        [Header("Orbit Settings")]
        [SerializeField] private float orbitDistance = 18f;
        [SerializeField] private float sensitivity = 0.3f;
        [SerializeField] private float minPitch = 10f;
        [SerializeField] private float maxPitch = 80f;

        private CinemachineCamera _virtualCamera;
        private CameraShake       _shake;
        private float _yaw;
        private float _pitch = 50f;

        private void Awake()
        {
            _virtualCamera = GetComponent<CinemachineCamera>();
            _shake         = GetComponent<CameraShake>();

            if (target != null)
                _virtualCamera.Follow = target;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // Free-look orbit — cursor is Locked during Playing so delta is always relative
            Vector2 delta = Mouse.current.delta.ReadValue();
            _yaw += delta.x * sensitivity;
            _pitch -= delta.y * sensitivity; // subtract: moving mouse up should raise camera
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

            // Position camera behind and above target at current yaw/pitch
            Vector3 basePos = target.position
                + Quaternion.Euler(_pitch, _yaw, 0f) * Vector3.back * orbitDistance;
            transform.position = basePos + (_shake != null ? _shake.CurrentOffset : Vector3.zero);

            transform.LookAt(target.position);
        }

        /// <summary>
        /// Sets the follow target at runtime (e.g., when player spawns).
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (_virtualCamera != null)
                _virtualCamera.Follow = target;
        }
    }
}
