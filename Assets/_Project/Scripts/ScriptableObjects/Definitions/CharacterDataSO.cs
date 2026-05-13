using UnityEngine;

namespace SwarmProtocol.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "SwarmProtocol/Character")]
    public class CharacterDataSO : ScriptableObject
    {
        public string characterName;
        [TextArea] public string characterDescription;
        public Sprite portrait;

        [Header("Base Stats")]
        public float baseMoveSpeed  = 8f;
        public float baseMaxHP      = 100f;
        public float baseMight      = 1f;
        public float baseCooldown   = 1f;
        public float baseMagnet     = 3f;
        public float baseLuck       = 0f;
    }
}
