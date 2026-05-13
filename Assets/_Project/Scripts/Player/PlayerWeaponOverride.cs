using System.Collections;
using UnityEngine;
using SwarmProtocol.Combat;
using SwarmProtocol.Events;

namespace SwarmProtocol.Player
{
    public class PlayerWeaponOverride : MonoBehaviour
    {
        [SerializeField] private WeaponManager  weaponManager;
        [SerializeField] private StrategyWeapon ndujaWeapon;
        [SerializeField] private PlayerStats    playerStats;

        private bool      _isOverriding;
        private Coroutine _overrideCoroutine;

        private void OnEnable()  => Event<NdujaActivatedEvent>.Subscribe(OnNdujaActivated);
        private void OnDisable() => Event<NdujaActivatedEvent>.Unsubscribe(OnNdujaActivated);

        private void OnNdujaActivated(NdujaActivatedEvent e)
        {
            if (_overrideCoroutine != null) StopCoroutine(_overrideCoroutine);
            _overrideCoroutine = StartCoroutine(Override(e.OverrideDuration));
        }

        private IEnumerator Override(float duration)
        {
            weaponManager.IsOverrideActive = true;
            ndujaWeapon.gameObject.SetActive(true);
            _isOverriding = true;

            yield return new WaitForSeconds(duration);

            _isOverriding = false;
            ndujaWeapon.gameObject.SetActive(false);
            weaponManager.IsOverrideActive = false;
            _overrideCoroutine = null;
        }

        private void Update()
        {
            if (!_isOverriding || ndujaWeapon == null) return;
            if (playerStats != null)
                ndujaWeapon.SetFireRateMultiplier(playerStats.FireRateMultiplier);
            ndujaWeapon.TryFire();
        }
    }
}
