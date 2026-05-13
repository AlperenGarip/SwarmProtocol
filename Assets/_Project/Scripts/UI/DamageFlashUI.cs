using UnityEngine;
using UnityEngine.UI;
using SwarmProtocol.Core;

namespace SwarmProtocol.UI
{
    /// <summary>
    /// Full-screen red overlay that pulses on player damage.
    /// Subscribes to EventBus.OnPlayerDamaged — only triggers for non-zero damage
    /// (so passive HUD-refresh PlayerDamaged(0) calls are ignored).
    /// </summary>
    public class DamageFlashUI : MonoBehaviour
    {
        [SerializeField] private Image flashImage;
        [SerializeField] private float peakAlpha   = 0.45f;
        [SerializeField] private float fadeDuration = 0.3f;

        private float _timer;

        private void Awake()
        {
            EventBus.OnPlayerDamaged += OnPlayerDamaged;
            if (flashImage != null) SetAlpha(0f);
        }

        private void OnDestroy()
        {
            EventBus.OnPlayerDamaged -= OnPlayerDamaged;
        }

        private void OnPlayerDamaged(float amount)
        {
            if (amount <= 0f) return; // ignore HUD-refresh pings
            _timer = fadeDuration;
            if (flashImage != null) SetAlpha(peakAlpha);
        }

        private void Update()
        {
            if (_timer <= 0f || flashImage == null) return;
            _timer -= Time.deltaTime;
            float t = Mathf.Clamp01(_timer / fadeDuration);
            SetAlpha(peakAlpha * t);
        }

        private void SetAlpha(float a)
        {
            var c = flashImage.color;
            c.a = a;
            flashImage.color = c;
        }
    }
}
