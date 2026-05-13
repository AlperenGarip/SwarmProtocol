using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Progression.Commands
{
    public class CoinBagCommand : IUpgradeCommand
    {
        private readonly int _goldAmount;

        public string DisplayName        => "Coin Bag";
        public string DisplayDescription => $"Gain {_goldAmount} gold";
        public Sprite Icon               => null;
        public UpgradeRarity Rarity      => UpgradeRarity.Common;
        public bool IsWeapon             => false;
        public int WeaponCurrentLevel    => 0;
        public int WeaponMaxLevel        => 0;
        public ScriptableObject SourceItem => null;

        public CoinBagCommand(int goldAmount) => _goldAmount = goldAmount;

        public void Execute() => EventBus.GoldCollected(_goldAmount);
    }
}
