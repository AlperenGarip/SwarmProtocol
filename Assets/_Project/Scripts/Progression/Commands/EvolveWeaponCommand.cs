using UnityEngine;
using SwarmProtocol.Combat;
using SwarmProtocol.Events;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Progression.Commands
{
    public class EvolveWeaponCommand : IUpgradeCommand
    {
        private readonly WeaponDataSO  _from;
        private readonly WeaponDataSO  _to;
        private readonly WeaponManager _wm;

        public string          DisplayName        => $"{_from.weaponName} → {_to.weaponName}";
        public string          DisplayDescription => _to.description;
        public Sprite          Icon               => _to.icon;
        public UpgradeRarity   Rarity             => UpgradeRarity.Epic;
        public bool            IsWeapon           => true;
        public int             WeaponCurrentLevel => 0;
        public int             WeaponMaxLevel     => 0;
        public ScriptableObject SourceItem        => _from;

        public EvolveWeaponCommand(WeaponDataSO from, WeaponDataSO to, WeaponManager wm)
        {
            _from = from;
            _to   = to;
            _wm   = wm;
        }

        public void Execute()
        {
            _wm.ReplaceWeapon(_from, _to);
            Event<WeaponEvolvedEvent>.Fire(new WeaponEvolvedEvent { FromWeapon = _from, ToWeapon = _to });
        }
    }
}
