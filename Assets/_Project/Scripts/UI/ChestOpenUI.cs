using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SwarmProtocol.Audio;
using SwarmProtocol.Chest;
using SwarmProtocol.Core;
using SwarmProtocol.Events;
using SwarmProtocol.Progression;
using SwarmProtocol.Progression.Commands;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.UI
{
    public class ChestOpenUI : MonoBehaviour
    {
        [SerializeField] private GameObject              chestPanel;
        [SerializeField] private TextMeshProUGUI         tierText;
        [SerializeField] private TextMeshProUGUI         jackpotBannerText;
        [SerializeField] private Button                  collectButton;
        [SerializeField] private List<SlotMachineColumn> columns;
        [SerializeField] private List<GameObject>        columnObjects;
        [SerializeField] private GameConfigSO            config;

        private JackpotResult         _pendingResult;
        private List<IUpgradeCommand> _pendingItems;

        private void Start()
        {
            collectButton?.onClick.AddListener(OnCollect);
            chestPanel?.SetActive(false);
        }

        private void OnEnable()  => EventBus.OnGameStateChanged += OnGameStateChanged;
        private void OnDisable() => EventBus.OnGameStateChanged -= OnGameStateChanged;

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.ChestOpen)
                StartCoroutine(RunChestUI());
            else
            {
                chestPanel?.SetActive(false);
                StopAllCoroutines();
            }
        }

        private IEnumerator RunChestUI()
        {
            int tier = ChestRewardResolver.RollTier(config);
            var items = ChestRewardResolver.RollItems(tier);

            while (items.Count < tier && items.Count > 0)
                items.Add(items[items.Count - 1]);

            if (items.Count == 0) { GameManager.Instance?.ResumeFromChestOpen(); yield break; }

            _pendingResult = JackpotProcessor.Evaluate(items, config);
            _pendingItems  = items;

            // Phase 1 — tier reveal
            chestPanel?.SetActive(true);
            AudioService.Instance?.PlaySfx(SfxId.ChestOpen);
            if (tierText != null)
            {
                tierText.SetText($"Tier {tier} Chest!");
                StartCoroutine(PopIn(tierText.transform, 0.4f, 1.25f));
            }
            for (int i = 0; i < columnObjects.Count; i++)
                columnObjects[i]?.SetActive(i < tier);
            jackpotBannerText?.gameObject.SetActive(false);
            collectButton?.gameObject.SetActive(false);

            yield return new WaitForSecondsRealtime(1.5f);

            // Phase 2 — spin columns
            int locked = 0;
            for (int i = 0; i < tier && i < columns.Count; i++)
            {
                if (columns[i] == null) continue;
                float spinDuration = 1.5f + i * 0.5f;
                columns[i].Spin(items[i], items, spinDuration, () => locked++);
            }

            float timeout = 0f;
            while (locked < tier)
            {
                timeout += Time.unscaledDeltaTime;
                if (timeout > 15f) break;
                yield return null;
            }

            // Phase 3 — result
            yield return new WaitForSecondsRealtime(0.25f); // small beat after the last lock
            if (jackpotBannerText != null)
            {
                string bannerText = BannerText(_pendingResult);
                if (!string.IsNullOrEmpty(bannerText))
                {
                    jackpotBannerText.gameObject.SetActive(true);
                    jackpotBannerText.SetText(bannerText);
                    AudioService.Instance?.PlaySfx(SfxId.LevelUp); // jackpot reuse — louder fanfare cue
                    StartCoroutine(PopIn(jackpotBannerText.transform, 0.5f, 1.4f));
                }
            }
            collectButton?.gameObject.SetActive(true);
            if (collectButton != null) StartCoroutine(PopIn(collectButton.transform, 0.3f, 1.15f));
        }

        /// <summary>Scale-in tween: 0 → peak → 1.0 with ease-out-back. Uses unscaled time (works in pause).</summary>
        private static IEnumerator PopIn(Transform t, float duration, float peak)
        {
            if (t == null) yield break;
            Vector3 baseScale = Vector3.one;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(elapsed / duration);
                // overshoot ease: pop above 1 then settle
                float s = k < 0.6f
                    ? Mathf.Lerp(0f, peak, EaseOutBack(k / 0.6f))
                    : Mathf.Lerp(peak, 1f, (k - 0.6f) / 0.4f);
                t.localScale = baseScale * s;
                yield return null;
            }
            t.localScale = baseScale;
        }

        private static float EaseOutBack(float x)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
        }

        private void OnCollect()
        {
            ApplyRewards(_pendingResult, _pendingItems);
            GameManager.Instance?.ResumeFromChestOpen();
            chestPanel?.SetActive(false);
        }

        private void ApplyRewards(JackpotResult result, List<IUpgradeCommand> items)
        {
            if (result.Type == JackpotType.None)
            {
                foreach (var cmd in items) cmd?.Execute();
                return;
            }

            if (result.ShouldForceEvolve)
            {
                var sourceWeapon = result.WinningCommand?.SourceItem as WeaponDataSO;
                WeaponEvolutionSystem.Instance?.ForceEvolve(sourceWeapon);
                return;
            }

            result.WinningCommand?.Execute();
            for (int i = 0; i < result.ExtraExecutions; i++) result.WinningCommand?.Execute();
            if (result.BonusGold > 0) EventBus.GoldCollected(result.BonusGold);
            if (result.TriggerOvercharge)
                Event<OverchargeTriggeredEvent>.Fire(new OverchargeTriggeredEvent
                    { Duration = config != null ? config.overchargeDuration : 60f });
        }

        private static string BannerText(JackpotResult r) => r.Type switch
        {
            JackpotType.Pair        => "PAIR!",
            JackpotType.Triple      => "TRIPLE!",
            JackpotType.FourOfAKind => "FOUR OF A KIND! OVERCHARGE!",
            JackpotType.FiveOfAKind => "JACKPOT! EVOLUTION!",
            _                       => string.Empty,
        };
    }
}
