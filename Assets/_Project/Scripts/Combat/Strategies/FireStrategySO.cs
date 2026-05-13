using UnityEngine;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Combat.Strategies
{
    /// <summary>
    /// All data a FireStrategySO needs to execute one shot.
    /// Passed by value — no scene references stored on the SO.
    /// </summary>
    public struct FireContext
    {
        public Transform    FirePoint;
        public Vector3      AimDirection;
        public WeaponDataSO WeaponData;
        public int          Level;
        public float        LevelDamageBonus;
        public float        OverchargeDamageMultiplier; // 1f normally; >1f during Overcharge
        public float        DamageMultiplier;           // Might passive (1f baseline)
        public float        CritChance;                 // Luck passive (0..1)
        public float        AreaMultiplier;             // Area passive (1f baseline)
    }

    /// <summary>
    /// Abstract base for weapon fire strategies (Strategy-as-SO pattern).
    /// Subclass + create a SO asset; reference it from WeaponDataSO.fireStrategy.
    /// New weapon type = new subclass + SO asset only — no new MonoBehaviour needed.
    /// </summary>
    public abstract class FireStrategySO : ScriptableObject
    {
        public abstract void Execute(FireContext ctx);
    }
}
