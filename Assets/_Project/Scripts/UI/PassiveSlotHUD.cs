using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SwarmProtocol.Core;
using SwarmProtocol.Events;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.UI
{
    public class PassiveSlotHUD : MonoBehaviour
    {
        [SerializeField] private List<Image> iconSlots;
        [SerializeField] private Color emptySlotColor = new Color(1f, 1f, 1f, 0.15f);

        private readonly List<PassiveItemSO> _activePassives = new();

        private void Start() => Refresh();

        private void OnEnable()
        {
            Event<PassiveAppliedEvent>.Subscribe(OnPassiveApplied);
            EventBus.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnDisable()
        {
            Event<PassiveAppliedEvent>.Unsubscribe(OnPassiveApplied);
            EventBus.OnGameStateChanged -= OnGameStateChanged;
        }

        private void OnPassiveApplied(PassiveAppliedEvent e)
        {
            if (e.Passive == null) return;
            // Dedupe: same passive picked multiple times still shows one icon
            // (rank/stack info lives in the stats overlay, not the slot bar).
            if (!_activePassives.Contains(e.Passive))
                _activePassives.Add(e.Passive);
            Refresh();
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.Menu)
            {
                _activePassives.Clear();
                Refresh();
            }
        }

        private void Refresh()
        {
            for (int i = 0; i < iconSlots.Count; i++)
            {
                if (iconSlots[i] == null) continue;
                bool hasPassive = i < _activePassives.Count;
                var  pd         = hasPassive ? _activePassives[i] : null;

                iconSlots[i].sprite = pd?.icon;
                iconSlots[i].color  = hasPassive && pd?.icon != null ? Color.white : emptySlotColor;
            }
        }
    }
}
