using UnityEngine;
using SwarmProtocol.Factories;
using SwarmProtocol.ScriptableObjects;
using SwarmProtocol.Stage;

namespace SwarmProtocol.Enemies
{
    /// <summary>
    /// Pure spawn executor. Picks a position from SpawnPointProvider,
    /// then delegates creation to EnemyFactory — no pool or init logic here.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private SpawnPointProvider spawnPoints;

        private Transform _playerTransform;

        public EnemyBase Spawn(EnemyDataSO data, float difficultyMultiplier)
        {
            if (data == null) return null;

            if (_playerTransform == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p != null) _playerTransform = p.transform;
            }

            Vector3 pos = spawnPoints != null
                ? spawnPoints.GetSpawnPoint(_playerTransform)
                : new Vector3(Random.Range(-40f, 40f), 0f, Random.Range(-40f, 40f));

            return EnemyFactory.Spawn(data, pos, difficultyMultiplier);
        }
    }
}
