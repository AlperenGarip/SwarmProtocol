using UnityEngine;
using SwarmProtocol.ScriptableObjects;

namespace SwarmProtocol.Progression
{
    public class CharacterSelectManager : MonoBehaviour
    {
        public static CharacterSelectManager Instance { get; private set; }

        public CharacterDataSO SelectedCharacter { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Select(CharacterDataSO character) => SelectedCharacter = character;
    }
}
