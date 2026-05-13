using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SwarmProtocol.Progression;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.UI
{
    public class PowerUpCardUI : MonoBehaviour
    {
        [SerializeField] private Image    iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text rankText;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private Button   buyButton;

        private PowerUpDataSO        _data;
        private Action<PowerUpDataSO> _onBuy;

        public void Setup(PowerUpDataSO data, Action<PowerUpDataSO> onBuy)
        {
            _data  = data;
            _onBuy = onBuy;
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => _onBuy?.Invoke(_data));
            Refresh();
        }

        public void Refresh()
        {
            if (_data == null) return;
            var mgr    = MetaProgressionManager.Instance;
            int rank   = mgr?.GetRank(_data) ?? 0;
            bool isMax = rank >= _data.maxRank;
            int cost   = mgr?.GetCost(_data) ?? _data.baseCost;

            if (iconImage      != null) iconImage.sprite  = _data.icon;
            if (nameText       != null) nameText.text      = _data.powerUpName;
            if (descriptionText!= null) descriptionText.text = _data.description;
            if (rankText       != null) rankText.text      = isMax ? "MAX" : $"Rank {rank} / {_data.maxRank}";
            if (costText       != null) costText.text      = isMax ? "—"   : $"{cost} G";
            if (buyButton      != null) buyButton.interactable = !isMax;
        }
    }
}
