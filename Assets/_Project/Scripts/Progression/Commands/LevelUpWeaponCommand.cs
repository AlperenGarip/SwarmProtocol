using UnityEngine;
using SwarmProtocol.Combat;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Progression.Commands
{
    public class LevelUpWeaponCommand : IUpgradeCommand
    {
        private readonly WeaponDataSO _data;
        private readonly WeaponManager _wm;
        private readonly int _currentLevel;

        public string DisplayName        => $"{_data.weaponName} Lv.{_currentLevel + 1}";
        public string DisplayDescription => _data.description;
        public Sprite Icon               => _data.icon;
        public UpgradeRarity Rarity      => UpgradeRarity.Rare;
        public bool IsWeapon             => true;
        public int WeaponCurrentLevel    => _currentLevel;
        public int WeaponMaxLevel        => _data.maxLevel;
        public ScriptableObject SourceItem => _data;

        public LevelUpWeaponCommand(WeaponDataSO data, WeaponManager wm, int currentLevel)
        {
            _data         = data;
            _wm           = wm;
            _currentLevel = currentLevel;
        }

        public void Execute() => _wm.AddOrLevelUpWeapon(_data);
    }
}
