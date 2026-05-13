using UnityEngine;
using SwarmProtocol.Combat;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Progression.Commands
{
    public class AddWeaponCommand : IUpgradeCommand
    {
        private readonly WeaponDataSO _data;
        private readonly WeaponManager _wm;

        public string DisplayName        => _data.weaponName;
        public string DisplayDescription => _data.description;
        public Sprite Icon               => _data.icon;
        public UpgradeRarity Rarity      => UpgradeRarity.Common;
        public bool IsWeapon             => true;
        public int WeaponCurrentLevel    => 0;
        public int WeaponMaxLevel        => _data.maxLevel;
        public ScriptableObject SourceItem => _data;

        public AddWeaponCommand(WeaponDataSO data, WeaponManager wm) { _data = data; _wm = wm; }

        public void Execute() => _wm.AddOrLevelUpWeapon(_data);
    }
}
