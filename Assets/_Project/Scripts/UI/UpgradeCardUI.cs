using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SwarmProtocol.Progression;
using SwarmProtocol.Progression.Commands;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.UI
{
    public class UpgradeCardUI : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Button banishButton;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Image rarityBackground;

        private static readonly Color CommonColor = new Color(0.55f, 0.55f, 0.55f, 0.25f);
        private static readonly Color RareColor   = new Color(0.20f, 0.50f, 1.00f, 0.35f);
        private static readonly Color EpicColor   = new Color(0.65f, 0.15f, 1.00f, 0.35f);

        private int _optionIndex;

        public void Setup(IUpgradeCommand cmd, int index)
        {
            _optionIndex = index;

            if (nameText        != null) nameText.text        = cmd.DisplayName;
            if (descriptionText != null) descriptionText.text = cmd.DisplayDescription;

            if (iconImage != null)
            {
                iconImage.sprite  = cmd.Icon;
                iconImage.enabled = cmd.Icon != null;
            }

            if (levelText != null)
            {
                bool showLevel = cmd.IsWeapon && cmd.WeaponCurrentLevel > 0;
                levelText.text  = showLevel ? $"Lv.{cmd.WeaponCurrentLevel} → {cmd.WeaponCurrentLevel + 1}" : "";
                levelText.color = showLevel ? new Color(1f, 0.85f, 0.3f) : Color.clear;
                // Keep GameObject active so the card's layout height stays constant across all cards.
            }

            if (rarityBackground != null)
            {
                rarityBackground.color = cmd.Rarity switch
                {
                    UpgradeRarity.Rare => RareColor,
                    UpgradeRarity.Epic => EpicColor,
                    _                  => CommonColor
                };
            }

            button?.onClick.RemoveAllListeners();
            button?.onClick.AddListener(() => UpgradeManager.Instance?.SelectOption(_optionIndex));

            if (banishButton != null)
            {
                banishButton.onClick.RemoveAllListeners();
                bool canBanish = cmd.SourceItem != null &&
                                 (UpgradeManager.Instance?.BanishCharges ?? 0) > 0;
                // Keep active — use visual dimming so layout height stays constant.
                banishButton.interactable = canBanish;
                var banishImg  = banishButton.GetComponent<Image>();
                var banishText = banishButton.GetComponentInChildren<TextMeshProUGUI>();
                Color btnColor = canBanish ? new Color(0.7f, 0.2f, 0.2f) : new Color(0.3f, 0.3f, 0.3f, 0.4f);
                Color txtColor = canBanish ? Color.white : new Color(1f, 1f, 1f, 0.3f);
                if (banishImg  != null) banishImg.color  = btnColor;
                if (banishText != null) banishText.color = txtColor;
                if (canBanish)
                    banishButton.onClick.AddListener(() => UpgradeManager.Instance?.TryBanish(_optionIndex));
            }
        }
    }
}
