using UnityEngine;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Progression.Commands
{
    public interface IUpgradeCommand
    {
        string DisplayName { get; }
        string DisplayDescription { get; }
        Sprite Icon { get; }
        UpgradeRarity Rarity { get; }
        bool IsWeapon { get; }
        int WeaponCurrentLevel { get; }
        int WeaponMaxLevel { get; }
        ScriptableObject SourceItem { get; } // null → consumable, cannot be banished
        void Execute();
    }
}
