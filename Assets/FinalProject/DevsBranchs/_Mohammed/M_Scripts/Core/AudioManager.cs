using UnityEngine;

namespace Aegis.Core
{
    /// <summary>
    /// Plays music and one-shot SFX, with master/music/SFX volume control.
    /// Creates its own <see cref="AudioSource"/>s at runtime if none are assigned,
    /// so it works dropped into an empty GameObject.
    /// Tier 0 — no dependencies on other game scripts.
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Sources (auto-created if left empty)")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;

        [Header("Volumes [TUNABLE]")]
        [Range(0f, 1f)][SerializeField] private float _masterVolume = 1f;
        [Range(0f, 1f)][SerializeField] private float _musicVolume = 0.7f;
        [Range(0f, 1f)][SerializeField] private float _sfxVolume = 1f;

        public float MasterVolume => _masterVolume;
        public float MusicVolume => _musicVolume;
        public float SfxVolume => _sfxVolume;

        protected override void OnAwake()
        {
            if (_musicSource == null) _musicSource = CreateSource("Music Source", loop: true);
            if (_sfxSource == null) _sfxSource = CreateSource("SFX Source", loop: false);
            ApplyVolumes();
        }

        private AudioSource CreateSource(string sourceName, bool loop)
        {
            var go = new GameObject(sourceName);
            go.transform.SetParent(transform);
            var source = go.AddComponent<AudioSource>();
            source.loop = loop;
            source.playOnAwake = false;
            return source;
        }

        /// <summary>Start (or replace) the background music track.</summary>
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null) return;
            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.Play();
        }

        public void StopMusic() => _musicSource.Stop();

        /// <summary>Fire a one-shot sound effect. <paramref name="volumeScale"/> is multiplied by the SFX + master volume.</summary>
        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null) return;
            _sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale) * _sfxVolume * _masterVolume);
        }

        public void SetMasterVolume(float value) { _masterVolume = Mathf.Clamp01(value); ApplyVolumes(); }
        public void SetMusicVolume(float value) { _musicVolume = Mathf.Clamp01(value); ApplyVolumes(); }
        public void SetSfxVolume(float value) { _sfxVolume = Mathf.Clamp01(value); }

        private void ApplyVolumes()
        {
            if (_musicSource != null) _musicSource.volume = _musicVolume * _masterVolume;
            // SFX volume is applied per one-shot in PlaySFX.
        }
    }
}
