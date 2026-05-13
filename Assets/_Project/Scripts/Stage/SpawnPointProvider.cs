using UnityEngine;

namespace SwarmProtocol.Stage
{
    /// <summary>
    /// Returns random arena spawn positions that are outside the minimum radius from the player.
    /// </summary>
    public class SpawnPointProvider : MonoBehaviour
    {
        [SerializeField] private float arenaHalfSize  = 40f;
        [SerializeField] private float minSpawnRadius = 10f;

        public Vector3 GetSpawnPoint(Transform player)
        {
            float minRadiusSq = minSpawnRadius * minSpawnRadius;

            for (int attempt = 0; attempt < 30; attempt++)
            {
                Vector3 pos = new Vector3(
                    Random.Range(-arenaHalfSize, arenaHalfSize),
                    0f,
                    Random.Range(-arenaHalfSize, arenaHalfSize)
                );

                if (player == null || (pos - player.position).sqrMagnitude >= minRadiusSq)
                    return pos;
            }

            return new Vector3(arenaHalfSize, 0f, 0f);
        }
    }
}
