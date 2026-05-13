using TMPro;
using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.Progression;

namespace SwarmProtocol.UI
{
    public class StageTransitionUI : MonoBehaviour
    {
        [SerializeField] private GameObject  panel;
        [SerializeField] private TMP_Text    headlineText;
        [SerializeField] private StageManager stageManager;

        private void OnEnable()  => EventBus.OnGameStateChanged += OnGameStateChanged;
        private void OnDisable() => EventBus.OnGameStateChanged -= OnGameStateChanged;

        private void Start() => panel?.SetActive(false);

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.StageTransition)
            {
                if (panel != null) panel.SetActive(true);
                if (headlineText != null && stageManager != null)
                    headlineText.text = $"STAGE {stageManager.CurrentStageNumber} INCOMING…";
            }
            else
            {
                panel?.SetActive(false);
            }
        }
    }
}
