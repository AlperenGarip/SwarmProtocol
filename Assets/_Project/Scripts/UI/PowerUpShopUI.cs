using System.Collections.Generic;
using TMPro;
using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.Progression;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.UI
{
    public class PowerUpShopUI : MonoBehaviour
    {
        [SerializeField] private GameObject    shopPanel;
        [SerializeField] private TMP_Text      goldText;
        [SerializeField] private PowerUpCardUI cardPrefab;
        [SerializeField] private Transform     cardContainer;

        private readonly List<PowerUpCardUI> _cards = new();

        private void OnEnable()  => EventBus.OnGameStateChanged += OnGameStateChanged;
        private void OnDisable() => EventBus.OnGameStateChanged -= OnGameStateChanged;

        private void Start()
        {
            shopPanel?.SetActive(false);
            // Wire close button if present
            var closeBtn = shopPanel != null
                ? shopPanel.transform.Find("CloseButton")?.GetComponent<UnityEngine.UI.Button>()
                : null;
            closeBtn?.onClick.AddListener(Hide);
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state != GameState.Menu) Hide();
        }

        public void Show()
        {
            shopPanel?.SetActive(true);
            if (_cards.Count == 0) BuildCards();
            RefreshAll();
        }

        public void Hide() => shopPanel?.SetActive(false);

        private void BuildCards()
        {
            if (MetaProgressionManager.Instance == null || cardPrefab == null || cardContainer == null) return;
            foreach (var pu in MetaProgressionManager.Instance.AllPowerUps)
            {
                var card = Instantiate(cardPrefab, cardContainer);
                card.Setup(pu, OnBuy);
                _cards.Add(card);
            }
            if (cardContainer is RectTransform rt)
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }

        private void OnBuy(PowerUpDataSO data)
        {
            MetaProgressionManager.Instance?.TryPurchase(data);
            RefreshAll();
        }

        private void RefreshAll()
        {
            foreach (var card in _cards) card.Refresh();
            if (goldText != null && GoldService.Instance != null)
                goldText.text = $"Gold: {GoldService.Instance.PersistentGold}";
        }
    }
}
