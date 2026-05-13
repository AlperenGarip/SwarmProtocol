using System.Collections.Generic;
using UnityEngine;

namespace SwarmProtocol.Progression
{
    public class ItemPool
    {
        private readonly HashSet<ScriptableObject> _banished = new();

        public void Banish(ScriptableObject item) => _banished.Add(item);
        public bool IsBanished(ScriptableObject item) => item != null && _banished.Contains(item);
        public void Reset() => _banished.Clear();
    }
}
