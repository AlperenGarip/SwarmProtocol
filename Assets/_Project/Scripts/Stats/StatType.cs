namespace SwarmProtocol.Stats
{
    public enum StatType
    {
        Might,    // damage multiplier
        Armor,    // incoming damage reduction (reserved)
        MaxHealth,
        MoveSpeed,
        Cooldown, // fire rate multiplier
        Area,     // weapon range multiplier (reserved)
        Magnet,   // pickup magnet range
        Luck,     // crit chance
        Growth,   // XP multiplier (reserved)
        Greed,    // gold multiplier (reserved)
        Recovery, // HP regen/sec (reserved)
        Curse,    // risk/reward: faster spawns + harder enemies
        None,     // sentinel for charge-type PowerUps (Reroll, Skip, Banish, Revival)
    }
}
