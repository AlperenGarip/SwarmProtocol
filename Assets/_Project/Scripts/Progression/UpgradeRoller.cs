using System.Collections.Generic;
using UnityEngine;
using SwarmProtocol.Combat;
using SwarmProtocol.Player;
using SwarmProtocol.ScriptableObjects;
using SwarmProtocol.Progression.Commands;

namespace SwarmProtocol.Progression
{
    public class UpgradeRoller
    {
        private readonly ItemPool _pool;
        private readonly List<WeaponDataSO>  _weapons;
        private readonly List<PassiveItemSO> _passives;
        private readonly WeaponManager _wm;
        private readonly PlayerHealth  _ph;
        private readonly PlayerStats   _ps;
        private readonly GameConfigSO  _config;
        private readonly float _elapsedTime;
        private readonly float _evolutionTimeGate;

        public UpgradeRoller(ItemPool pool, List<WeaponDataSO> weapons, List<PassiveItemSO> passives,
                             WeaponManager wm, PlayerHealth ph, PlayerStats ps, GameConfigSO config,
                             float elapsedTime = 0f, float evolutionTimeGate = float.MaxValue)
        {
            _pool              = pool;
            _weapons           = weapons;
            _passives          = passives;
            _wm                = wm;
            _ph                = ph;
            _ps                = ps;
            _config            = config;
            _elapsedTime       = elapsedTime;
            _evolutionTimeGate = evolutionTimeGate;
        }

        public List<IUpgradeCommand> Roll(int count)
        {
            var weighted = BuildWeightedCandidates();
            Shuffle(weighted);

            var result = new List<IUpgradeCommand>(count);
            var seen   = new HashSet<ScriptableObject>();

            foreach (var cmd in weighted)
            {
                if (result.Count >= count) break;
                if (cmd.SourceItem != null && !seen.Add(cmd.SourceItem)) continue;
                result.Add(cmd);
            }

            // Consumable fallback when pool is exhausted
            if (result.Count < count)
                result.Add(new FloorChickenCommand(_ph, _config != null ? _config.floorChickenHealPercent : 0.3f));
            if (result.Count < count)
                result.Add(new CoinBagCommand(_config != null ? _config.coinBagGoldAmount : 100));

            return result;
        }

        private List<IUpgradeCommand> BuildWeightedCandidates()
        {
            var candidates = new List<IUpgradeCommand>();

            foreach (var wd in _weapons)
            {
                if (wd == null || _pool.IsBanished(wd)) continue;

                if (_wm != null && _wm.HasWeapon(wd))
                {
                    var wb = _wm.FindWeapon(wd);
                    if (wb != null && !wb.IsMaxLevel)
                        AddWeighted(candidates, new LevelUpWeaponCommand(wd, _wm, wb.CurrentLevel), UpgradeRarity.Rare);
                }
                else if (_wm != null && _wm.HasFreeSlot)
                {
                    AddWeighted(candidates, new AddWeaponCommand(wd, _wm), UpgradeRarity.Common);
                }
            }

            foreach (var wd in _weapons)
            {
                if (wd == null || wd.evolutionTarget == null) continue;
                if (_wm == null || !_wm.HasWeapon(wd)) continue;
                var wb = _wm.FindWeapon(wd);
                if (wb == null || !wb.IsMaxLevel) continue;
                if (_elapsedTime < _evolutionTimeGate) continue;
                if (_wm.HasWeapon(wd.evolutionTarget)) continue;
                if (_pool.IsBanished(wd)) continue;
                candidates.Add(new EvolveWeaponCommand(wd, wd.evolutionTarget, _wm));
            }

            // Cap on distinct passives held — same shape as weapon-slot cap.
            // If the player already has maxPassiveSlots distinct passives, only offer
            // upgrades to ones they already hold (which still stack via the decorator chain).
            int maxPassives    = _config != null ? _config.maxPassiveSlots : 6;
            int uniqueHeld     = _ps != null ? _ps.UniquePassiveCount : 0;
            bool passiveSlotsFull = uniqueHeld >= maxPassives;

            foreach (var pd in _passives)
            {
                if (pd == null || _pool.IsBanished(pd)) continue;
                bool alreadyHeld = _ps != null && _ps.HasPassive(pd);
                if (passiveSlotsFull && !alreadyHeld) continue; // skip new passives when full
                AddWeighted(candidates, new AddPassiveCommand(pd), pd.rarity);
            }

            return candidates;
        }

        private static void AddWeighted(List<IUpgradeCommand> list, IUpgradeCommand cmd, UpgradeRarity rarity)
        {
            int copies = rarity switch
            {
                UpgradeRarity.Epic => 1,
                UpgradeRarity.Rare => 2,
                _                  => 3,
            };
            for (int i = 0; i < copies; i++) list.Add(cmd);
        }

        private static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
