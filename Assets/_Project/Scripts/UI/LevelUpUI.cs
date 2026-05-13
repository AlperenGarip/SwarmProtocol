using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SwarmProtocol.Core;
using SwarmProtocol.Progression;

namespace SwarmProtocol.UI
{
    public class LevelUpUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject levelUpPanel;

        [Header("Cards (assign exactly 3)")]
        [SerializeField] private List<UpgradeCardUI> cards;

        [Header("Reroll / Skip")]
        [SerializeField] private Button rerollButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private TextMeshProUGUI rerollChargeText;
        [SerializeField] private TextMeshProUGUI skipChargeText;

        [Header("Stats Overlay")]
        [SerializeField] private StatsOverlayUI statsOverlay;

        private void OnEnable()  => EventBus.OnGameStateChanged += OnGameStateChanged;
        private void OnDisable() => EventBus.OnGameStateChanged -= OnGameStateChanged;

        private void Start()
        {
            levelUpPanel?.SetActive(false);

            rerollButton?.onClick.AddListener(() => UpgradeManager.Instance?.TryReroll());
            skipButton?.onClick.AddListener(()   => UpgradeManager.Instance?.TrySkip());

            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.OnOptionsRolled  += Refresh;
                UpgradeManager.Instance.OnChargesChanged += UpdateChargeDisplay;
            }
        }

        private void OnDestroy()
        {
            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.OnOptionsRolled  -= Refresh;
                UpgradeManager.Instance.OnChargesChanged -= UpdateChargeDisplay;
            }
        }

        private void OnGameStateChanged(GameState newState)
        {
            if (newState == GameState.LevelUp)
                Show();
            else
                levelUpPanel?.SetActive(false);
        }

        private void Show()
        {
            levelUpPanel?.SetActive(true);
            PopulateCards();
            UpdateChargeDisplay();
            statsOverlay?.Refresh();
        }

        public void Refresh()
        {
            if (levelUpPanel != null && levelUpPanel.activeSelf)
            {
                PopulateCards();
                UpdateChargeDisplay();
            }
        }

        private void PopulateCards()
        {
            if (UpgradeManager.Instance == null) return;
            var options = UpgradeManager.Instance.CurrentOptions;
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i] == null) continue;
                bool hasOption = i < options.Count;
                cards[i].gameObject.SetActive(hasOption);
                if (hasOption) cards[i].Setup(options[i], i);
            }
            if (levelUpPanel != null)
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(
                    levelUpPanel.GetComponent<UnityEngine.RectTransform>());
        }

        private void UpdateChargeDisplay()
        {
            var um = UpgradeManager.Instance;
            if (um == null) return;

            if (rerollChargeText != null) rerollChargeText.SetText($"Reroll ({um.RerollCharges})");
            if (skipChargeText   != null) skipChargeText.SetText($"Skip ({um.SkipCharges})");
            if (rerollButton     != null) rerollButton.interactable = um.RerollCharges > 0;
            if (skipButton       != null) skipButton.interactable   = um.SkipCharges   > 0;
        }
    }
}
