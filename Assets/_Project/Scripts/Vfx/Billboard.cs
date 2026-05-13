using UnityEngine;

namespace SwarmProtocol.Vfx
{
    /// <summary>
    /// Rotates this transform to face the main camera each LateUpdate.
    /// Use on sprite quads (treasure chest, gold coin, XP gem, etc.) so they
    /// always present their flat side to the player regardless of camera orbit.
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        [SerializeField] private bool lockY = true;

        private Transform _camTransform;

        private void OnEnable()
        {
            var cam = UnityEngine.Camera.main;
            if (cam != null) _camTransform = cam.transform;
        }

        private void LateUpdate()
        {
            if (_camTransform == null)
            {
                var cam = UnityEngine.Camera.main;
                if (cam == null) return;
                _camTransform = cam.transform;
            }

            Vector3 toCam = _camTransform.position - transform.position;
            if (lockY) toCam.y = 0f;
            if (toCam.sqrMagnitude < 0.0001f) return;
            transform.rotation = Quaternion.LookRotation(-toCam.normalized);
        }
    }
}
