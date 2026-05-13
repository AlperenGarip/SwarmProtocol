using UnityEngine;
using SwarmProtocol.Combat.Strategies;

namespace SwarmProtocol.ScriptableObjects
{
    public enum WeaponType
    {
        RapidFire,
        Shotgun,
        Melee,
        Orbital,
        Aura
    }

    /// <summary>
    /// Data container for weapon configuration. Create instances in the editor
    /// via right-click → Create → SwarmProtocol → Weapon.
    /// </summary>
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "SwarmProtocol/Weapon")]
    public class WeaponDataSO : ScriptableObject
    {
        [Header("General")]
        public string weaponName;
        [TextArea] public string description;
        public Sprite icon;
        public WeaponType weaponType;

        [Header("Prefabs")]
        public GameObject    weaponPrefab;     // WeaponBase MonoBehaviour prefab (instantiated into weapon container)
        public GameObject    projectilePrefab; // null for melee weapons
        public FireStrategySO fireStrategy;   // null → weapon uses its own Fire() override (legacy)

        [Header("Stats")]
        public float baseDamage = 10f;
        public float knockbackForce    = 2f;
        public float knockbackDuration = 0.2f;
        public float fireRate = 5f;          // shots per second
        public float projectileSpeed = 20f;
        public float range = 15f;
        public int projectilesPerShot = 1;   // 1 for single, 5–7 for shotgun
        public float spreadAngle = 0f;       // degrees, 0 for single shot
        public bool piercing = false;

        [Header("Leveling")]
        public int maxLevel = 8;
        public float damagePerLevel  = 5f;   // added to baseDamage on each level-up
        public float fireRatePerLevel = 0f;  // added to fireRate on each level-up

        [Header("Evolution")]
        public WeaponDataSO evolutionTarget;

        [Header("Audio")]
        public AudioClip fireSound;

        /// <summary>
        /// Time between shots in seconds, derived from fireRate.
        /// </summary>
        public float FireInterval => 1f / fireRate;
    }
}
