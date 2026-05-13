using System;
using System.Collections.Generic;
using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.Combat;
using SwarmProtocol.Events;
using SwarmProtocol.Player;
using SwarmProtocol.ScriptableObjects;
using SwarmProtocol.Stage;
using SwarmProtocol.Progression.Commands;

namespace SwarmProtocol.Progression
{
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        [Header("Available Content")]
        [SerializeField] private List<WeaponDataSO>  availableWeapons;
        [SerializeField] private List<PassiveItemSO> availablePassives;

        [Header("References")]
        [SerializeField] private WeaponManager weaponManager;
        [SerializeField] private PlayerHealth  playerHealth;
        [SerializeField] private PlayerStats   playerStats;
        [SerializeField] private PlayerXP      playerXP;
        [SerializeField] private GameConfigSO  config;
        [SerializeField] private StageTimer    stageTimer;

        public event Action OnOptionsRolled;
        public event Action OnChargesChanged;

        public IReadOnlyList<IUpgradeCommand> CurrentOptions => _currentOptions;
        public int RerollCharges => _rerollCharges;
        public int SkipCharges   => _skipCharges;
        public int BanishCharges => _banishCharges;

        private readonly ItemPool _itemPool = new();
        private List<IUpgradeCommand> _currentOptions = new();
        private int _rerollCharges, _skipCharges, _banishCharges;
        private StageDataSO _currentStageData;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start() => ResetCharges();

        private void OnEnable()
        {
            EventBus.OnLevelUp          += HandleLevelUp;
            EventBus.OnGameStateChanged += OnGameStateChanged;
            Event<StageStartEvent>.Subscribe(OnStageStart);
        }

        private void OnDisable()
        {
            EventBus.OnLevelUp          -= HandleLevelUp;
            EventBus.OnGameStateChanged -= OnGameStateChanged;
            Event<StageStartEvent>.Unsubscribe(OnStageStart);
        }

        private void HandleLevelUp(int newLevel) => RollOptions();

        private void OnStageStart(StageStartEvent e) => _currentStageData = e.StageData;

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.Menu)
            {
                _itemPool.Reset();
                ResetCharges();
            }
        }

        private void ResetCharges()
        {
            _rerollCharges = config != null ? config.startingRerollCharges : 1;
            _skipCharges   = config != null ? config.startingSkipCharges   : 1;
            _banishCharges = config != null ? config.startingBanishCharges : 1;

            if (MetaProgressionManager.Instance != null)
            {
                _rerollCharges += MetaProgressionManager.Instance.ExtraRerollCharges;
                _skipCharges   += MetaProgressionManager.Instance.ExtraSkipCharges;
                _banishCharges += MetaProgressionManager.Instance.ExtraBanishCharges;
            }

            OnChargesChanged?.Invoke();
        }

        public void RollOptions()
        {
            float elapsed       = stageTimer != null ? stageTimer.ElapsedTime : 0f;
            float evolutionGate = _currentStageData?.evolutionAllowedAfterTime ?? float.MaxValue;
            var roller = new UpgradeRoller(_itemPool, availableWeapons, availablePassives,
                                           weaponManager, playerHealth, playerStats, config, elapsed, evolutionGate);
            _currentOptions = roller.Roll(config != null ? config.upgradeOptionsCount : 3);
            OnOptionsRolled?.Invoke();
        }

        public void SelectOption(int index)
        {
            if (index < 0 || index >= _currentOptions.Count) return;
            _currentOptions[index].Execute();
            playerHealth?.GrantIFrames(config != null ? config.postLevelUpIFrameDuration : 1.5f);
            GameManager.Instance?.ResumeFromLevelUp();
        }

        public bool TryReroll()
        {
            if (_rerollCharges <= 0) return false;
            _rerollCharges--;
            OnChargesChanged?.Invoke();
            RollOptions();
            return true;
        }

        public bool TrySkip()
        {
            if (_skipCharges <= 0) return false;
            _skipCharges--;
            OnChargesChanged?.Invoke();
            GameManager.Instance?.ResumeFromLevelUp();
            int xpGrant = Mathf.RoundToInt(
                (playerXP != null ? playerXP.XPToNextLevel : 0) *
                (config != null ? config.skipXPPercent : 0.2f));
            if (xpGrant > 0) EventBus.XPCollected(xpGrant);
            return true;
        }

        public bool TryBanish(int index)
        {
            if (_banishCharges <= 0 || index < 0 || index >= _currentOptions.Count) return false;
            var cmd = _currentOptions[index];
            if (cmd.SourceItem == null) return false;
            _banishCharges--;
            OnChargesChanged?.Invoke();
            _itemPool.Banish(cmd.SourceItem);
            RollOptions();
            return true;
        }

        public List<IUpgradeCommand> RollChestOptions(int count)
        {
            float elapsed       = stageTimer != null ? stageTimer.ElapsedTime : 0f;
            float evolutionGate = _currentStageData?.evolutionAllowedAfterTime ?? float.MaxValue;
            var roller = new UpgradeRoller(_itemPool, availableWeapons, availablePassives,
                                           weaponManager, playerHealth, playerStats, config, elapsed, evolutionGate);
            return roller.Roll(count);
        }
    }
}
