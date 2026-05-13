using UnityEngine;
using SwarmProtocol.Enemies.Behaviors;

namespace SwarmProtocol.ScriptableObjects
{
    public enum XPGemSize { Small, Medium, Large }

    /// <summary>
    /// Data container for enemy type configuration.
    /// Create instances via right-click → Create → SwarmProtocol → Enemy.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "SwarmProtocol/Enemy")]
    public class EnemyDataSO : ScriptableObject
    {
        [Header("General")]
        public string     enemyName;
        public GameObject prefab;

        [Header("Stats")]
        public float maxHealth      = 30f;
        public float moveSpeed      = 4f;
        public float damage         = 10f;
        public float attackRange    = 1.5f;
        public float attackCooldown = 1f;

        [Header("Drops")]
        public int       xpDropAmount  = 5;
        public XPGemSize xpGemSize     = XPGemSize.Small;
        [Range(0, 100)] public int goldDropChance       = 20;
        [Range(0, 100)] public int healthOrbDropChance  = 0;

        [Header("Classification")]
        public bool            isBoss    = false;
        public bool            isElite   = false;    // elites always drop a chest
        public EnemyBehaviorSO behavior;             // null → enemy uses its own UpdateBehavior() override

        [Header("Visuals")]
        public Color tintColor = Color.white;
    }
}
