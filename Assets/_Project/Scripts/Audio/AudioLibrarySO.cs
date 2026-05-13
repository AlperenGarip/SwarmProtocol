using UnityEngine;

namespace SwarmProtocol.Audio
{
    /// <summary>
    /// Single source of truth for every audio clip in the game.
    /// Drop your generated WAV/MP3 clips into the matching slot in the Inspector.
    /// AudioService reads from here at startup.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioLibrary", menuName = "SwarmProtocol/Audio Library")]
    public class AudioLibrarySO : ScriptableObject
    {
        [Header("SFX")]
        public AudioClip weaponFire;
        public AudioClip enemyHit;
        public AudioClip enemyDeath;
        public AudioClip xpCollect;
        public AudioClip goldCollect;
        public AudioClip levelUp;
        public AudioClip playerDamage;
        public AudioClip playerDeath;
        public AudioClip chestOpen;
        public AudioClip uiClick;

        [Header("Music")]
        public AudioClip musicMenu;
        public AudioClip musicGameplay;

        [Header("Mix")]
        [Range(0f, 1f)] public float sfxVolume   = 0.8f;
        [Range(0f, 1f)] public float musicVolume = 0.4f;

        public AudioClip Get(SfxId id) => id switch
        {
            SfxId.WeaponFire   => weaponFire,
            SfxId.EnemyHit     => enemyHit,
            SfxId.EnemyDeath   => enemyDeath,
            SfxId.XPCollect    => xpCollect,
            SfxId.GoldCollect  => goldCollect,
            SfxId.LevelUp      => levelUp,
            SfxId.PlayerDamage => playerDamage,
            SfxId.PlayerDeath  => playerDeath,
            SfxId.ChestOpen    => chestOpen,
            SfxId.UIClick      => uiClick,
            _                  => null,
        };

        public AudioClip Get(MusicId id) => id switch
        {
            MusicId.Menu     => musicMenu,
            MusicId.Gameplay => musicGameplay,
            _                => null,
        };
    }
}
