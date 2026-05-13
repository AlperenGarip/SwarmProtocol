using UnityEngine;
using SwarmProtocol.Events;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Progression.Commands
{
    public class AddPassiveCommand : IUpgradeCommand
    {
        private readonly PassiveItemSO _data;

        public string DisplayName        => _data.passiveName;
        public string DisplayDescription => _data.description;
        public Sprite Icon               => _data.icon;
        public UpgradeRarity Rarity      => _data.rarity;
        public bool IsWeapon             => false;
        public int WeaponCurrentLevel    => 0;
        public int WeaponMaxLevel        => 0;
        public ScriptableObject SourceItem => _data;

        public AddPassiveCommand(PassiveItemSO data) => _data = data;

        public void Execute() =>
            Event<PassiveAppliedEvent>.Fire(new PassiveAppliedEvent { Passive = _data });
    }
}
