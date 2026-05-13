namespace SwarmProtocol.Stats
{
    public class PassiveDecorator : IStatProvider
    {
        private readonly IStatProvider _inner;
        private readonly StatType _stat;
        private readonly float _value;
        private readonly bool _isPercentage;

        public PassiveDecorator(IStatProvider inner, StatType stat, float value, bool isPercentage)
        {
            _inner       = inner;
            _stat        = stat;
            _value       = value;
            _isPercentage = isPercentage;
        }

        public float Get(StatType stat)
        {
            float v = _inner.Get(stat);
            if (stat != _stat) return v;
            return _isPercentage ? v * (1f + _value) : v + _value;
        }
    }
}
