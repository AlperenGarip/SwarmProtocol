using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.Events;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Pickups
{
    public class NdujaFrittaPickup : MagnetCollectible
    {
        [SerializeField] private GameConfigSO gameConfig;

        private NdujaFrittaPickup _prefabRef;

        public void Initialize(NdujaFrittaPickup prefabRef)
        {
            _prefabRef = prefabRef;
            Activate(transform.position);
        }

        protected override void OnCollect()
        {
            float dur = gameConfig != null ? gameConfig.ndujaFrittaDuration : 10f;
            Event<NdujaActivatedEvent>.Fire(new NdujaActivatedEvent { OverrideDuration = dur });

            if (_prefabRef != null && ObjectPoolManager.Instance != null)
                ObjectPoolManager.Instance.Return(_prefabRef, this);
            else
                gameObject.SetActive(false);
        }

        public override void OnSpawn() => base.OnSpawn();
    }
}
