using UnityEngine;
using SwarmProtocol.Events;
using SwarmProtocol.ScriptableObjects;
using SwarmProtocol.Progression;

namespace SwarmProtocol.Core
{
    /// <summary>
    /// First MonoBehaviour to run (Script Execution Order: -100).
    /// Registers all game services with ServiceLocator so consumers can call
    /// ServiceLocator.Get&lt;T&gt;() at any point after Awake.
    ///
    /// Add this to a persistent "Bootstrap" GameObject that exists across scenes
    /// (DontDestroyOnLoad). Assign GameConfigSO in the Inspector.
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private GameConfigSO gameConfig;

        [Header("Core Services (on same GameObject or children)")]
        [SerializeField] private GameManager     gameManager;
        [SerializeField] private ObjectPoolManager poolManager;
        [SerializeField] private ActiveEnemyRegistry enemyRegistry;

        [Header("Progression Services")]
        [SerializeField] private GoldManager goldManager;

        private void Awake()
        {
            // Clear stale references from any previous scene
            ServiceLocator.Reset();
            EventSystem.ResetAll();

            // Register config first — other services may need it
            if (gameConfig != null)
                ServiceLocator.Register<GameConfigSO>(gameConfig);
            else
                Debug.LogError("[Bootstrapper] GameConfigSO is not assigned!");

            // Core services
            if (gameManager      != null) ServiceLocator.Register<GameManager>(gameManager);
            if (poolManager      != null) ServiceLocator.Register<ObjectPoolManager>(poolManager);
            if (enemyRegistry    != null) ServiceLocator.Register<ActiveEnemyRegistry>(enemyRegistry);
            if (goldManager      != null) ServiceLocator.Register<GoldManager>(goldManager);

            DontDestroyOnLoad(gameObject);
        }
    }
}
