using System.Collections.Generic;
using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.Events;
using SwarmProtocol.Progression;
using SwarmProtocol.ScriptableObjects;
using SwarmProtocol.Stats;

namespace SwarmProtocol.Player
{
    /// <summary>
    /// Manages player stats via an IStatProvider decorator chain.
    /// Base values come from serialized fields; each PassiveItemSO picked wraps
    /// the chain in a PassiveDecorator. Fires StatsChangedEvent after every change.
    /// All existing public properties (MoveSpeed, MaxHP, etc.) are preserved as facades.
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        [Header("Base Stats")]
        [SerializeField] private float baseMoveSpeed  = 8f;
        [SerializeField] private float baseMaxHP       = 100f;
        [SerializeField] private float baseMight       = 1f;
        [SerializeField] private float baseCooldown    = 1f;
        [SerializeField] private float baseMagnet      = 3f;
        [SerializeField] private float baseLuck        = 0f;
        [SerializeField] private float baseIFrameDuration = 0.5f;

        // ─── Backward-compatible public API ──────────────────────
        public float MoveSpeed          => Get(StatType.MoveSpeed);
        public float MaxHP              => Get(StatType.MaxHealth);
        public float DamageMultiplier   => Get(StatType.Might);
        public float FireRateMultiplier => Get(StatType.Cooldown);
        public float MagnetRange        => Get(StatType.Magnet);
        public float CritChance         => Get(StatType.Luck);
        public float Curse              => Get(StatType.Curse);
        public float Armor              => Get(StatType.Armor);
        public float AreaMultiplier     => Get(StatType.Area);
        public float RecoveryPerSecond  => Get(StatType.Recovery);
        public float IFrameDuration     => baseIFrameDuration;

        // ─── Decorator chain ─────────────────────────────────────
        private IStatProvider _chain;
        private readonly List<PassiveItemSO> _appliedPassives = new();

        public float Get(StatType stat) => _chain?.Get(stat) ?? 0f;

        /// <summary>True if this passive has been picked at least once (any rank).</summary>
        public bool HasPassive(PassiveItemSO p) => p != null && _appliedPassives.Contains(p);

        /// <summary>Distinct passives currently held — used to enforce maxPassiveSlots cap.</summary>
        public int UniquePassiveCount
        {
            get
            {
                var seen = new HashSet<PassiveItemSO>();
                foreach (var p in _appliedPassives) if (p != null) seen.Add(p);
                return seen.Count;
            }
        }

        private void Awake() => RebuildChain();

        private void OnEnable()
        {
            Event<PassiveAppliedEvent>.Subscribe(OnPassiveApplied);
            EventBus.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnDisable()
        {
            Event<PassiveAppliedEvent>.Unsubscribe(OnPassiveApplied);
            EventBus.OnGameStateChanged -= OnGameStateChanged;
        }

        private void OnPassiveApplied(PassiveAppliedEvent e)
        {
            if (e.Passive == null) return;
            _appliedPassives.Add(e.Passive);
            RebuildChain();
            Event<StatsChangedEvent>.Fire(new StatsChangedEvent { CurseLevel = Curse });
        }

        private bool _pendingNewRun;

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.Menu)
            {
                _pendingNewRun = true;
                ResetToBase();
            }
            else if (state == GameState.Playing && _pendingNewRun)
            {
                _pendingNewRun = false;
                RebuildChain(); // re-apply with selected character + PowerUp bonuses
            }
        }

        /// <summary>Resets all stats to base values. Called on new run.</summary>
        public void ResetToBase()
        {
            _appliedPassives.Clear();
            RebuildChain();
            Event<StatsChangedEvent>.Fire(new StatsChangedEvent { CurseLevel = 0f });
        }

        private void RebuildChain()
        {
            var ch = CharacterSelectManager.Instance?.SelectedCharacter;
            var baseValues = new Dictionary<StatType, float>
            {
                { StatType.MoveSpeed,  ch != null ? ch.baseMoveSpeed : baseMoveSpeed },
                { StatType.MaxHealth,  ch != null ? ch.baseMaxHP     : baseMaxHP     },
                { StatType.Might,      ch != null ? ch.baseMight     : baseMight     },
                { StatType.Cooldown,   ch != null ? ch.baseCooldown  : baseCooldown  },
                { StatType.Magnet,     ch != null ? ch.baseMagnet    : baseMagnet    },
                { StatType.Luck,       ch != null ? ch.baseLuck      : baseLuck      },
                { StatType.Curse,      0f },
                { StatType.Armor,      0f },
                { StatType.Area,       1f }, // 1.0 = unchanged; passives multiply via PassiveDecorator
                { StatType.Recovery,   0f },
            };

            MetaProgressionManager.Instance?.ApplyStatBonuses(baseValues);

            IStatProvider chain = new BaseStatProvider(baseValues);

            foreach (var passive in _appliedPassives)
                chain = new PassiveDecorator(chain, passive.statType, passive.value, passive.isPercentage);

            _chain = chain;
        }
    }
}
