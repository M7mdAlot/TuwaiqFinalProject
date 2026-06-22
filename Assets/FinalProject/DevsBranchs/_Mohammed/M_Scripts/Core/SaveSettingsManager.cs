using System;
using UnityEngine;

namespace Aegis.Core
{
    /// <summary>
    /// Stores and remembers player options (volumes, mouse sensitivity, invert-Y) using
    /// Unity's PlayerPrefs, so they survive between play sessions. Raises
    /// <see cref="SettingsChanged"/> so systems (AudioManager, camera) can apply them.
    /// Tier 0 — no dependencies on other game scripts. (Key rebinding comes later.)
    /// </summary>
    public class SaveSettingsManager : Singleton<SaveSettingsManager>
    {
        private const string MasterKey = "aegis.volume.master";
        private const string MusicKey = "aegis.volume.music";
        private const string SfxKey = "aegis.volume.sfx";
        private const string SensitivityKey = "aegis.look.sensitivity";
        private const string InvertYKey = "aegis.look.invertY";

        public float MasterVolume { get; private set; } = 1f;
        public float MusicVolume { get; private set; } = 0.7f;
        public float SfxVolume { get; private set; } = 1f;
        public float MouseSensitivity { get; private set; } = 0.1f;
        public bool InvertY { get; private set; }

        /// <summary>Raised whenever any setting changes (and once after loading on startup).</summary>
        public event Action SettingsChanged;

        protected override void OnAwake() => Load();

        public void SetMasterVolume(float v) { MasterVolume = Mathf.Clamp01(v); SaveAndNotify(); }
        public void SetMusicVolume(float v) { MusicVolume = Mathf.Clamp01(v); SaveAndNotify(); }
        public void SetSfxVolume(float v) { SfxVolume = Mathf.Clamp01(v); SaveAndNotify(); }
        public void SetMouseSensitivity(float v) { MouseSensitivity = Mathf.Max(0.001f, v); SaveAndNotify(); }
        public void SetInvertY(bool v) { InvertY = v; SaveAndNotify(); }

        public void Load()
        {
            MasterVolume = PlayerPrefs.GetFloat(MasterKey, MasterVolume);
            MusicVolume = PlayerPrefs.GetFloat(MusicKey, MusicVolume);
            SfxVolume = PlayerPrefs.GetFloat(SfxKey, SfxVolume);
            MouseSensitivity = PlayerPrefs.GetFloat(SensitivityKey, MouseSensitivity);
            InvertY = PlayerPrefs.GetInt(InvertYKey, 0) == 1;
            SettingsChanged?.Invoke();
        }

        private void SaveAndNotify()
        {
            PlayerPrefs.SetFloat(MasterKey, MasterVolume);
            PlayerPrefs.SetFloat(MusicKey, MusicVolume);
            PlayerPrefs.SetFloat(SfxKey, SfxVolume);
            PlayerPrefs.SetFloat(SensitivityKey, MouseSensitivity);
            PlayerPrefs.SetInt(InvertYKey, InvertY ? 1 : 0);
            PlayerPrefs.Save();
            SettingsChanged?.Invoke();
        }
    }
}
