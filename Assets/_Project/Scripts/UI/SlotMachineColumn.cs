using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SwarmProtocol.Audio;
using SwarmProtocol.Progression.Commands;

namespace SwarmProtocol.UI
{
    /// <summary>
    /// Single slot-machine column. Cycles through random items at an accelerating-then-decelerating
    /// cadence, plays a tick on each swap, and bounces + flashes on lock.
    /// </summary>
    public class SlotMachineColumn : MonoBehaviour
    {
        [SerializeField] private Image           iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Image           backgroundImage; // optional — column card background, used for lock flash

        [Header("Lock animation")]
        [SerializeField] private float lockBounceScale = 1.18f;
        [SerializeField] private float lockBounceTime  = 0.25f;
        [SerializeField] private Color lockFlashColor  = new Color(1f, 0.95f, 0.4f, 1f);

        private Color   _backgroundOriginal;
        private Vector3 _initialScale;

        private void Awake()
        {
            _initialScale = transform.localScale;
            if (backgroundImage == null) backgroundImage = GetComponent<Image>();
            if (backgroundImage != null) _backgroundOriginal = backgroundImage.color;
        }

        public void Spin(IUpgradeCommand finalItem, List<IUpgradeCommand> pool, float duration, Action onLocked)
            => StartCoroutine(SpinCoroutine(finalItem, pool, duration, onLocked));

        private IEnumerator SpinCoroutine(IUpgradeCommand finalItem, List<IUpgradeCommand> pool,
                                          float duration, Action onLocked)
        {
            if (pool == null || pool.Count == 0)
            {
                ShowItem(finalItem);
                LockFx();
                onLocked?.Invoke();
                yield break;
            }

            float elapsed     = 0f;
            float minInterval = 0.05f;
            float maxInterval = 0.35f;
            float nextSwap    = 0f;
            int   idx         = 0;

            while (elapsed < duration)
            {
                float t        = elapsed / duration;
                float interval = Mathf.Lerp(minInterval, maxInterval, t * t);
                if (elapsed >= nextSwap)
                {
                    ShowItem(pool[idx % pool.Count]);
                    AudioService.Instance?.PlaySfx(SfxId.UIClick, 0.25f); // slot tick
                    idx++;
                    nextSwap = elapsed + interval;
                }
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            ShowItem(finalItem);
            LockFx();
            onLocked?.Invoke();
        }

        private void ShowItem(IUpgradeCommand cmd)
        {
            if (iconImage != null)
            {
                iconImage.sprite = cmd?.Icon;
                iconImage.color  = cmd?.Icon != null ? Color.white : new Color(1f, 1f, 1f, 0.2f);
            }
            if (nameText != null) nameText.SetText(cmd?.DisplayName ?? string.Empty);
        }

        private void LockFx()
        {
            // Slightly louder click on lock to mark the moment
            AudioService.Instance?.PlaySfx(SfxId.UIClick, 0.6f);
            StartCoroutine(BounceAndFlash());
        }

        private IEnumerator BounceAndFlash()
        {
            // Two-phase tween: scale up to lockBounceScale, then settle to 1.0
            // Background flashes lockFlashColor at peak, fades back to original
            float t = 0f;
            float half = lockBounceTime * 0.5f;

            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / half);
                transform.localScale = Vector3.Lerp(_initialScale, _initialScale * lockBounceScale, EaseOutCubic(k));
                if (backgroundImage != null)
                    backgroundImage.color = Color.Lerp(_backgroundOriginal, lockFlashColor, k);
                yield return null;
            }
            t = 0f;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / half);
                transform.localScale = Vector3.Lerp(_initialScale * lockBounceScale, _initialScale, EaseOutCubic(k));
                if (backgroundImage != null)
                    backgroundImage.color = Color.Lerp(lockFlashColor, _backgroundOriginal, k);
                yield return null;
            }
            transform.localScale = _initialScale;
            if (backgroundImage != null) backgroundImage.color = _backgroundOriginal;
        }

        private static float EaseOutCubic(float x) => 1f - Mathf.Pow(1f - x, 3f);
    }
}
