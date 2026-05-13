using UnityEngine;

namespace SwarmProtocol.ScriptableObjects
{
    /// <summary>
    /// Shared magnet behaviour config for pooled pickups (XP gems, gold coins, health orbs).
    /// Create one instance per pickup type so each type has independently tuned values.
    /// </summary>
    [CreateAssetMenu(fileName = "NewMagnetConfig", menuName = "SwarmProtocol/MagnetConfig")]
    public class MagnetConfigSO : ScriptableObject
    {
        [Header("Attraction")]
        public float magnetSpeed    = 12f;
        public float magnetRange    = 5f;
        public float pickupDistance = 0.5f;

        [Header("Idle Bob")]
        public float bobSpeed  = 2f;
        public float bobHeight = 0.3f;

        [Tooltip("If true, uses PlayerStats.MagnetRange instead of magnetRange above.")]
        public bool usePlayerMagnetRange = false;
    }
}
