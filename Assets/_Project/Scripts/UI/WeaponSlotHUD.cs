using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SwarmProtocol.Combat;

namespace SwarmProtocol.UI
{
    public class WeaponSlotHUD : MonoBehaviour
    {
        [SerializeField] private WeaponManager weaponManager;
        [SerializeField] private List<Image>              iconSlots;
        [SerializeField] private List<TextMeshProUGUI>    levelTexts;
        [SerializeField] private Color emptySlotColor = new Color(1f, 1f, 1f, 0.15f);

        private void LateUpdate()
        {
            if (weaponManager == null) return;
            var weapons = weaponManager.ActiveWeapons;

            for (int i = 0; i < iconSlots.Count; i++)
            {
                if (iconSlots[i] == null) continue;

                bool hasWeapon = i < weapons.Count && weapons[i] != null;
                var  wd        = hasWeapon ? weapons[i].WeaponData : null;

                iconSlots[i].sprite  = wd?.icon;
                iconSlots[i].color   = hasWeapon && wd?.icon != null ? Color.white : emptySlotColor;

                if (levelTexts != null && i < levelTexts.Count && levelTexts[i] != null)
                {
                    levelTexts[i].gameObject.SetActive(hasWeapon);
                    if (hasWeapon)
                        levelTexts[i].SetText($"Lv.{weapons[i].CurrentLevel}");
                }
            }
        }
    }
}
