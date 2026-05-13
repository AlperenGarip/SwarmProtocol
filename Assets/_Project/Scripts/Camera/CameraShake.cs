using UnityEngine;
using SwarmProtocol.Core;

namespace SwarmProtocol.Camera
{
    /// <summary>
    /// Adds a noise-based positional offset to the camera that decays over time.
    /// CameraController reads CurrentOffset each LateUpdate and adds it after positioning.
    /// Auto-shakes on player damage; external code can call Trigger() for chests, jackpots, etc.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        [SerializeField] private float defaultIntensity = 0.25f;
        [SerializeField] private float defaultDuration  = 0.20f;
        [SerializeField] private float frequency        = 25f;

        private float   _intensity;
        private float   _duration;
        private float   _timer;
        private Vector3 _seed;

        public Vector3 CurrentOffset { get; private set; }

        private void Awake()
        {
            _seed = new Vector3(Random.value * 100f, Random.value * 100f, Random.value * 100f);
            EventBus.OnPlayerDamaged += OnPlayerDamaged;
        }

        private void OnDestroy()
        {
            EventBus.OnPlayerDamaged -= OnPlayerDamaged;
        }

        private void OnPlayerDamaged(float amount)
        {
            if (amount <= 0f) return; // skip HUD-refresh pings
            Trigger(defaultIntensity, defaultDuration);
        }

        public void Trigger(float intensity, float duration)
        {
            _intensity = Mathf.Max(_intensity, intensity);
            _duration  = Mathf.Max(_duration, duration);
            _timer     = Mathf.Max(_timer, duration);
        }

        private void LateUpdate()
        {
            if (_timer <= 0f) { CurrentOffset = Vector3.zero; return; }
            _timer -= Time.unscaledDeltaTime;
            float falloff = _duration > 0f ? Mathf.Clamp01(_timer / _duration) : 0f;
            float t = Time.unscaledTime * frequency;

            // PerlinNoise returns 0..1 — remap to -1..1
            float x = (Mathf.PerlinNoise(_seed.x, t) - 0.5f) * 2f;
            float y = (Mathf.PerlinNoise(_seed.y, t) - 0.5f) * 2f;
            float z = (Mathf.PerlinNoise(_seed.z, t) - 0.5f) * 2f;
            CurrentOffset = new Vector3(x, y, z) * _intensity * falloff;

            if (_timer <= 0f)
            {
                _intensity = 0f;
                _duration  = 0f;
                CurrentOffset = Vector3.zero;
            }
        }
    }
}
