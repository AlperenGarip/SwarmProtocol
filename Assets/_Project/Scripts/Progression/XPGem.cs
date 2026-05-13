using System.Collections.Generic;
using UnityEngine;
using SwarmProtocol.Audio;
using SwarmProtocol.Core;
using SwarmProtocol.Pickups;

namespace SwarmProtocol.Progression
{
    public class XPGem : MagnetCollectible
    {
        public static readonly HashSet<XPGem> ActiveInstances = new();

        private int   _xpValue;
        private XPGem _prefabRef;

        public void Initialize(int value, XPGem prefabRef)
        {
            ActiveInstances.Add(this);
            _xpValue  = value;
            _prefabRef = prefabRef;
            Activate(transform.position);
        }

        protected override void OnCollect()
        {
            ActiveInstances.Remove(this);
            EventBus.XPCollected(_xpValue);
            AudioService.Instance?.PlaySfx(SfxId.XPCollect);

            if (_prefabRef != null && ObjectPoolManager.Instance != null)
                ObjectPoolManager.Instance.Return(_prefabRef, this);
            else
                gameObject.SetActive(false);
        }

        public override void OnDespawn()
        {
            ActiveInstances.Remove(this);
            base.OnDespawn();
        }
    }
}
