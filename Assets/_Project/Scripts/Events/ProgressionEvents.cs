using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Events
{
    public struct PassiveAppliedEvent
    {
        public PassiveItemSO Passive;
    }

    public struct XPCollectedEvent
    {
        public int Amount;
        public int TotalXP;
    }

    public struct LevelUpEvent
    {
        public int NewLevel;
    }

    public struct GoldCollectedEvent
    {
        public int Amount;
        public int TotalGold;
    }

    public struct WeaponLeveledUpEvent
    {
        public WeaponDataSO WeaponData;
        public int NewLevel;
    }

    public struct WeaponEvolvedEvent
    {
        public WeaponDataSO FromWeapon;
        public WeaponDataSO ToWeapon;
    }

    public struct PassiveAddedEvent
    {
        public UpgradeDataSO UpgradeData;
        public int NewLevel;
    }

    public struct OverchargeTriggeredEvent
    {
        public float Duration;
    }
}
