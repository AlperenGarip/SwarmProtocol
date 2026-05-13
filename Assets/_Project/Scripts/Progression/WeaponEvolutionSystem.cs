using UnityEngine;
using SwarmProtocol.Combat;
using SwarmProtocol.Events;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Progression
{
    public class WeaponEvolutionSystem : MonoBehaviour
    {
        public static WeaponEvolutionSystem Instance { get; private set; }

        [SerializeField] private WeaponManager weaponManager;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public bool CanEvolve(WeaponDataSO source, float elapsedTime, float evolutionTimeGate)
        {
            if (source == null || source.evolutionTarget == null) return false;
            if (!weaponManager.HasWeapon(source)) return false;
            var wb = weaponManager.FindWeapon(source);
            if (wb == null || !wb.IsMaxLevel) return false;
            if (elapsedTime < evolutionTimeGate) return false;
            if (weaponManager.HasWeapon(source.evolutionTarget)) return false;
            return true;
        }

        public void Evolve(WeaponDataSO from, WeaponDataSO to)
        {
            weaponManager.ReplaceWeapon(from, to);
            Event<WeaponEvolvedEvent>.Fire(new WeaponEvolvedEvent { FromWeapon = from, ToWeapon = to });
        }

        public void ForceEvolve(WeaponDataSO from)
        {
            if (from == null || from.evolutionTarget == null) return;
            if (!weaponManager.HasWeapon(from)) return;
            Evolve(from, from.evolutionTarget);
        }
    }
}
