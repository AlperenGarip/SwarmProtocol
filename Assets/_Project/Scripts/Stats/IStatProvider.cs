namespace SwarmProtocol.Stats
{
    public interface IStatProvider
    {
        float Get(StatType stat);
    }
}
