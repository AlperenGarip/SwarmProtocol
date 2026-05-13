using UnityEngine;
using UnityEngine.UI;
using SwarmProtocol.Audio;
using SwarmProtocol.Core;

namespace SwarmProtocol.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private Button     startButton;
        [SerializeField] private Button     quitButton;
        [SerializeField] private Button     shopButton;
        [SerializeField] private Button     characterSelectButton;

        [Header("Sub-panels")]
        [SerializeField] private PowerUpShopUI      shopUI;
        [SerializeField] private CharacterSelectUI  characterSelectUI;

        private void OnEnable()  => EventBus.OnGameStateChanged += OnGameStateChanged;
        private void OnDisable() => EventBus.OnGameStateChanged -= OnGameStateChanged;

        private void Start()
        {
            startButton?.onClick.AddListener(() => { Click(); GameManager.Instance?.StartGame(); });
            quitButton?.onClick.AddListener(() => { Click(); QuitGame(); });
            shopButton?.onClick.AddListener(() => { Click(); shopUI?.Show(); });
            characterSelectButton?.onClick.AddListener(() => { Click(); characterSelectUI?.Show(); });
            menuPanel?.SetActive(true);
        }

        private static void Click() => AudioService.Instance?.PlaySfx(SfxId.UIClick);

        private void QuitGame()
        {
#if UNITY_EDITOR
            // Application.Quit is a no-op in the editor — stop Play Mode instead.
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnGameStateChanged(GameState newState)
        {
            menuPanel?.SetActive(newState == GameState.Menu);
        }
    }
}
