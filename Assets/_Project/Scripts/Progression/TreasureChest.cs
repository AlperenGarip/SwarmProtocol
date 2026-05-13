using UnityEngine;
using SwarmProtocol.Core;
using SwarmProtocol.Events;

namespace SwarmProtocol.Progression
{
    [RequireComponent(typeof(Collider))]
    public class TreasureChest : MonoBehaviour, IPoolable
    {
        private TreasureChest _prefabRef;
        private bool  _isActive;
        private float _stageElapsedTime;

        public void Initialize(TreasureChest prefabRef, float stageElapsedTime = 0f)
        {
            _prefabRef        = prefabRef;
            _isActive         = true;
            _stageElapsedTime = stageElapsedTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isActive) return;
            if (other.CompareTag("Player")) Open();
        }

        private void Open()
        {
            if (!_isActive) return;
            _isActive = false;

            Event<ChestOpenedEvent>.Fire(new ChestOpenedEvent
            {
                StageElapsedTime = _stageElapsedTime,
                ChestPosition    = transform.position
            });

            GameManager.Instance?.EnterChestOpen();
            // ChestOpenUI subscribes to ChestOpen state, runs the slot machine,
            // then calls GameManager.ResumeFromChestOpen() on Collect.

            ReturnToPool();
        }

        private void ReturnToPool()
        {
            if (_prefabRef != null && ObjectPoolManager.Instance != null)
                ObjectPoolManager.Instance.Return(_prefabRef, this);
            else
                gameObject.SetActive(false);
        }

        public void OnSpawn()  { _isActive = true;  }
        public void OnDespawn(){ _isActive = false; }
    }
}
