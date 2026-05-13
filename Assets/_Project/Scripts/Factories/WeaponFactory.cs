using UnityEngine;
using SwarmProtocol.Combat;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Factories
{
    /// <summary>
    /// Centralizes weapon instantiation. WeaponManager delegates here so
    /// the prefab reference lives in WeaponDataSO, not scattered at call sites.
    /// </summary>
    public static class WeaponFactory
    {
        public static WeaponBase Create(WeaponDataSO data, Transform container)
        {
            if (data?.weaponPrefab == null)
            {
                Debug.LogWarning($"[WeaponFactory] WeaponDataSO '{data?.weaponName}' has no weaponPrefab.");
                return null;
            }

            var go     = Object.Instantiate(data.weaponPrefab, container);
            var weapon = go.GetComponent<WeaponBase>();

            if (weapon == null)
            {
                Debug.LogWarning($"[WeaponFactory] {data.weaponPrefab.name} is missing a WeaponBase component.");
                Object.Destroy(go);
                return null;
            }

            return weapon;
        }
    }
}
