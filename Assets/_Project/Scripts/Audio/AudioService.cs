using UnityEngine;
using SwarmProtocol.Core;

namespace SwarmProtocol.Audio
{
    /// <summary>
    /// Singleton audio playback service. Pools 8 SFX AudioSources for one-shot playback,
    /// keeps a separate looping music source, and auto-swaps menu/gameplay tracks on
    /// game-state change. Safe to call before/after scene loads — never throws on missing clips.
    /// </summary>
    public class AudioService : MonoBehaviour
    {
        public static AudioService Instance { get; private set; }

        [SerializeField] private AudioLibrarySO library;
        [SerializeField] private int sfxVoiceCount = 8;

        private AudioSource[] _sfxSources;
        private int           _nextSfx;
        private AudioSource   _musicSource;
        private MusicId       _currentMusic = MusicId.None;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            _sfxSources = new AudioSource[Mathf.Max(1, sfxVoiceCount)];
            for (int i = 0; i < _sfxSources.Length; i++)
            {
                var go = new GameObject($"SfxSource_{i}");
                go.transform.SetParent(transform, false);
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 0f; // 2D
                _sfxSources[i] = src;
            }

            var musicGo = new GameObject("MusicSource");
            musicGo.transform.SetParent(transform, false);
            _musicSource = musicGo.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop        = true;
            _musicSource.spatialBlend = 0f;
        }

        private void OnEnable()  => EventBus.OnGameStateChanged += OnGameStateChanged;
        private void OnDisable() => EventBus.OnGameStateChanged -= OnGameStateChanged;

        private void OnGameStateChanged(GameState state)
        {
            // Menu state plays the menu loop; everything else uses the gameplay loop.
            // Pause/LevelUp/Chest/StageTransition keep gameplay music playing in the background.
            if (state == GameState.Menu) PlayMusic(MusicId.Menu);
            else                          PlayMusic(MusicId.Gameplay);
        }

        public void PlaySfx(SfxId id, float volumeScale = 1f)
        {
            if (library == null) return;
            var clip = library.Get(id);
            if (clip == null) return;

            var src = _sfxSources[_nextSfx];
            _nextSfx = (_nextSfx + 1) % _sfxSources.Length;
            src.PlayOneShot(clip, library.sfxVolume * Mathf.Clamp01(volumeScale));
        }

        public void PlayMusic(MusicId id)
        {
            if (library == null || id == _currentMusic) return;
            var clip = library.Get(id);
            if (clip == null) return;

            _currentMusic        = id;
            _musicSource.clip    = clip;
            _musicSource.volume  = library.musicVolume;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            _currentMusic = MusicId.None;
            _musicSource?.Stop();
        }
    }
}
