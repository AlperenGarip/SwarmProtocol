using UnityEngine;
using SwarmProtocol.Player;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Progression.Commands
{
    public class FloorChickenCommand : IUpgradeCommand
    {
        private readonly PlayerHealth _health;
        private readonly float _healPercent;

        public string DisplayName        => "Floor Chicken";
        public string DisplayDescription => $"Restore {_healPercent * 100f:0}% of max HP";
        public Sprite Icon               => null;
        public UpgradeRarity Rarity      => UpgradeRarity.Common;
        public bool IsWeapon             => false;
        public int WeaponCurrentLevel    => 0;
        public int WeaponMaxLevel        => 0;
        public ScriptableObject SourceItem => null;

        public FloorChickenCommand(PlayerHealth health, float healPercent)
        {
            _health      = health;
            _healPercent = healPercent;
        }

        public void Execute()
        {
            if (_health != null)
                _health.Heal(_health.MaxHP * _healPercent);
        }
    }
}
