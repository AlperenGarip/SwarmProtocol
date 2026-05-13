using SwarmProtocol.Enemies;
using SwarmProtocol.ScriptableObjects;
using UnityEngine;

namespace SwarmProtocol.Events
{
    public struct EnemyKilledEvent
    {
        public EnemyBase Enemy;
        public EnemyDataSO EnemyData;
        public Vector3 DeathPosition;
    }

    public struct PlayerDamagedEvent
    {
        public float Amount;
        public float CurrentHP;
        public float MaxHP;
    }

    public struct PlayerDeathEvent { }
}
