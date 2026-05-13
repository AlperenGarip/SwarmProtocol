using System.Collections.Generic;

namespace SwarmProtocol.Stats
{
    public class BaseStatProvider : IStatProvider
    {
        private readonly Dictionary<StatType, float> _values;

        public BaseStatProvider(Dictionary<StatType, float> values) => _values = values;

        public float Get(StatType stat) => _values.TryGetValue(stat, out var v) ? v : 0f;
    }
}
