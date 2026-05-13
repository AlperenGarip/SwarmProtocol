using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.Events;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Pickups
{
    public class OrologionPickup : MagnetCollectible
    {
        [SerializeField] private GameConfigSO gameConfig;

        private OrologionPickup _prefabRef;

        public void Initialize(OrologionPickup prefabRef)
        {
            _prefabRef = prefabRef;
            Activate(transform.position);
        }

        protected override void OnCollect()
        {
            float dur = gameConfig != null ? gameConfig.orologionFreezeDuration : 8f;
            ActiveEnemyRegistry.Instance?.FreezeAll(dur);
            Event<OrologionActivatedEvent>.Fire(new OrologionActivatedEvent { FreezeDuration = dur });

            if (_prefabRef != null && ObjectPoolManager.Instance != null)
                ObjectPoolManager.Instance.Return(_prefabRef, this);
            else
                gameObject.SetActive(false);
        }

        public override void OnSpawn() => base.OnSpawn();
    }
}
