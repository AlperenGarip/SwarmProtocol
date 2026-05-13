namespace SwarmProtocol.Audio
{
    /// <summary>
    /// Enum of every SFX in the game. Each value maps to one AudioClip slot
    /// on AudioLibrarySO. Adding a new SFX = add an enum value + a slot.
    /// </summary>
    public enum SfxId
    {
        WeaponFire,
        EnemyHit,
        EnemyDeath,
        XPCollect,
        GoldCollect,
        LevelUp,
        PlayerDamage,
        PlayerDeath,
        ChestOpen,
        UIClick,
    }

    public enum MusicId
    {
        None,
        Menu,
        Gameplay,
    }
}
