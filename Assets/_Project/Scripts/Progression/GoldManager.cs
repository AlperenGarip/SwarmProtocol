using UnityEngine;
using SwarmProtocol.Core;

namespace SwarmProtocol.Progression
{
    /// <summary>
    /// Singleton that tracks the player's gold total.
    /// Listens to OnGoldCollected to accumulate; exposes TrySpend() for costs like rerolls.
    /// </summary>
    public class GoldManager : MonoBehaviour
    {
        public static GoldManager Instance { get; private set; }

        public int Gold { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()  { EventBus.OnGoldCollected += OnGoldCollected; }
        private void OnDisable() { EventBus.OnGoldCollected -= OnGoldCollected; }

        private void OnGoldCollected(int amount) => Gold += amount;

        /// <summary>
        /// Deducts <paramref name="amount"/> from gold if affordable.
        /// Returns false and makes no change if insufficient gold.
        /// </summary>
        public bool TrySpend(int amount)
        {
            if (Gold < amount) return false;
            Gold -= amount;
            return true;
        }

        public void Reset() => Gold = 0;
    }
}
