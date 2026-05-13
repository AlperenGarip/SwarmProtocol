using UnityEngine;
using SwarmProtocol.Audio;
using SwarmProtocol.Core;
using SwarmProtocol.Pickups;

namespace SwarmProtocol.Progression
{
    /// <summary>
    /// Pooled gold coin. Inherits magnet movement from MagnetCollectible.
    /// Assign a GoldMagnetConfig SO (magnetRange ~5, usePlayerMagnetRange = false).
    /// </summary>
    public class GoldCoin : MagnetCollectible
    {
        private int      _value;
        private GoldCoin _prefabRef;

        public void Initialize(int value, GoldCoin prefabRef)
        {
            _value    = value;
            _prefabRef = prefabRef;
            Activate(transform.position);
        }

        protected override void OnCollect()
        {
            EventBus.GoldCollected(_value);
            AudioService.Instance?.PlaySfx(SfxId.GoldCollect);

            if (_prefabRef != null && ObjectPoolManager.Instance != null)
                ObjectPoolManager.Instance.Return(_prefabRef, this);
            else
                gameObject.SetActive(false);
        }
    }
}
