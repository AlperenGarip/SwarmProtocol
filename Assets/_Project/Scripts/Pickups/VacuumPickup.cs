using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.Progression;

namespace SwarmProtocol.Pickups
{
    public class VacuumPickup : MagnetCollectible
    {
        private VacuumPickup _prefabRef;

        public void Initialize(VacuumPickup prefabRef)
        {
            _prefabRef = prefabRef;
            Activate(transform.position);
        }

        protected override void OnCollect()
        {
            foreach (var gem in XPGem.ActiveInstances)
                gem?.ForceAttract();

            if (_prefabRef != null && ObjectPoolManager.Instance != null)
                ObjectPoolManager.Instance.Return(_prefabRef, this);
            else
                gameObject.SetActive(false);
        }

        public override void OnSpawn() => base.OnSpawn();
    }
}
