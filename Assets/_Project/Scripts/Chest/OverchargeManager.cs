using UnityEngine;
using SwarmProtocol.Combat;
using SwarmProtocol.Core;
using SwarmProtocol.Events;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Chest
{
    public class OverchargeManager : MonoBehaviour
    {
        public static OverchargeManager Instance { get; private set; }

        [SerializeField] private WeaponManager weaponManager;
        [SerializeField] private GameConfigSO  config;

        private float _timer;
        private bool  _isActive;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            Event<OverchargeTriggeredEvent>.Subscribe(OnOverchargeTriggered);
            EventBus.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnDisable()
        {
            Event<OverchargeTriggeredEvent>.Unsubscribe(OnOverchargeTriggered);
            EventBus.OnGameStateChanged -= OnGameStateChanged;
        }

        private void OnOverchargeTriggered(OverchargeTriggeredEvent e)
        {
            _timer = e.Duration;
            if (!_isActive) ApplyMultipliers();
            _isActive = true;
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.Menu && _isActive)
            {
                ResetMultipliers();
                _isActive = false;
            }
        }

        private void Update()
        {
            if (!_isActive) return;
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                ResetMultipliers();
                _isActive = false;
            }
        }

        private void ApplyMultipliers()
        {
            if (weaponManager == null || config == null) return;
            weaponManager.OverchargeFireRateMultiplier = config.overchargeFireRateMultiplier;
            foreach (var w in weaponManager.ActiveWeapons)
                w?.SetOverchargeDamageMultiplier(config.overchargeDamageMultiplier);
        }

        private void ResetMultipliers()
        {
            if (weaponManager == null) return;
            weaponManager.OverchargeFireRateMultiplier = 1f;
            foreach (var w in weaponManager.ActiveWeapons)
                w?.ResetOvercharge();
        }
    }
}
