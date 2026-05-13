using System;
using System.Collections.Generic;
using UnityEngine;

namespace SwarmProtocol.ScriptableObjects
{
    /// <summary>
    /// Defines a timed stage: duration, time-based spawn brackets, boss spawns,
    /// elite burst, and evolution time-gate.
    /// Create instances via right-click → Create → SwarmProtocol → Stage.
    /// </summary>
    [CreateAssetMenu(fileName = "NewStage", menuName = "SwarmProtocol/Stage")]
    public class StageDataSO : ScriptableObject
    {
        [Header("Stage Info")]
        public string stageName = "Stage 1";
        public int    stageNumber = 1;
        public float  stageDuration = 300f;         // seconds (300 = 5 min)
        public float  difficultyMultiplier = 1f;    // scales enemy HP and damage

        [Header("Spawn Brackets")]
        public List<SpawnBracket> spawnBrackets = new();

        [Serializable]
        public class SpawnBracket
        {
            public float     fromTime      = 0f;   // elapsed seconds when bracket activates
            public EnemyDataSO enemyData;
            public float     spawnInterval = 2f;   // seconds between spawn bursts
            public int       spawnCount    = 1;    // enemies per burst
        }

        [Header("Boss Spawns (timed)")]
        public List<BossSpawn> bossSpawns = new();

        [Serializable]
        public class BossSpawn
        {
            public float       atTime;             // elapsed seconds when boss spawns
            public EnemyDataSO bossEnemy;
        }

        [Header("Elite Burst (near end)")]
        public bool        overrideEliteBurstTime = false;
        public float       eliteBurstTimeBeforeEnd = 30f;   // seconds before stage end
        public int         eliteBurstCount = 5;
        public EnemyDataSO eliteEnemyData;                  // kept for legacy EnemySpawner

        [Header("Weapon Evolution Time-Gate")]
        [Tooltip("Boss chests opened before this elapsed time grant standard rewards only (no evolution).")]
        public float evolutionAllowedAfterTime = 120f;      // e.g., 2 min into stage

        /// <summary>Returns all brackets active at <paramref name="elapsedTime"/>.</summary>
        public List<SpawnBracket> GetActiveBrackets(float elapsedTime)
        {
            var active = new List<SpawnBracket>();
            foreach (var bracket in spawnBrackets)
            {
                if (elapsedTime >= bracket.fromTime)
                    active.Add(bracket);
            }
            return active;
        }
    }
}
