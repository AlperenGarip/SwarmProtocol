using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SwarmProtocol.Core;
using SwarmProtocol.Progression;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.UI
{
    public class CharacterSelectUI : MonoBehaviour
    {
        [SerializeField] private GameObject             selectPanel;
        [SerializeField] private List<CharacterDataSO>  characters;
        [SerializeField] private Transform              cardContainer;
        [SerializeField] private GameObject             characterCardPrefab;
        [SerializeField] private Button                 closeButton;

        private void OnEnable()  => EventBus.OnGameStateChanged += OnGameStateChanged;
        private void OnDisable() => EventBus.OnGameStateChanged -= OnGameStateChanged;

        private void Start()
        {
            selectPanel?.SetActive(false);
            closeButton?.onClick.AddListener(Hide);
            BuildCards();
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state != GameState.Menu) Hide();
        }

        public void Show() => selectPanel?.SetActive(true);
        public void Hide() => selectPanel?.SetActive(false);

        private void BuildCards()
        {
            if (characterCardPrefab == null || cardContainer == null) return;
            foreach (var data in characters)
            {
                if (data == null) continue;
                var go = Instantiate(characterCardPrefab, cardContainer);

                // Portrait: first Image that is NOT a background (has a sprite or data.portrait is set)
                var images = go.GetComponentsInChildren<Image>();
                Image portrait = null;
                foreach (var img in images)
                {
                    if (img.gameObject.name == "Portrait") { portrait = img; break; }
                }
                if (portrait == null && images.Length > 0) portrait = images[0];
                if (portrait != null && data.portrait != null) portrait.sprite = data.portrait;

                // Texts: first TMP = name, second TMP = description
                var texts = go.GetComponentsInChildren<TMP_Text>();
                if (texts.Length > 0) texts[0].text = data.characterName;
                if (texts.Length > 1) texts[1].text = data.characterDescription;

                var btn      = go.GetComponentInChildren<Button>();
                var captured = data;
                btn?.onClick.AddListener(() =>
                {
                    CharacterSelectManager.Instance?.Select(captured);
                    Hide();
                });
            }
        }
    }
}
