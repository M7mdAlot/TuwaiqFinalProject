using UnityEngine;
using UnityEngine.UI;
using Aegis.Core;
using Aegis.Core.Events;

namespace Aegis.Systems
{
    /// <summary>
    /// Drives the on-screen HUD (GDD §14) by setting values on whatever UI widgets you drop
    /// in from the SCI-FI UI Pack Pro. It only READS/SETS standard Unity UI components on
    /// your scene instances (Image fill, Slider value, legacy Text) — it never modifies the
    /// purchased pack itself. Every reference is optional (null-safe), so the game runs even
    /// before the HUD is fully assembled.
    /// Tier 1 — depends only on Tier 0 (event channels) + Unity UI.
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        [Header("Health (assign a filled Image OR a Slider — whichever your bar uses)")]
        [Tooltip("An Image whose Image Type = Filled (most SCI-FI bars work this way).")]
        [SerializeField] private Image _healthFill;
        [Tooltip("Alternative: a Slider-style bar (Min Value 0, Max Value 1).")]
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private Text _healthText;
        [Tooltip("If set, the health bar updates automatically from this channel (0..1).")]
        [SerializeField] private FloatEventChannelSO _healthChannel;

        [Header("Ammo")]
        [SerializeField] private Text _ammoText;

        [Header("Prompts & banners")]
        [SerializeField] private GameObject _interactPrompt;
        [SerializeField] private Text _interactPromptText;
        [SerializeField] private GameObject _objectiveBanner;
        [SerializeField] private Text _objectiveText;

        [Header("Crisis timer (e.g. the pack's Timer01 widget)")]
        [SerializeField] private GameObject _crisisTimer;
        [SerializeField] private Text _crisisTimerText;
        [Tooltip("Optional: a filled Image that drains as time runs out.")]
        [SerializeField] private Image _crisisFill;

        [Header("Other")]
        [SerializeField] private GameObject _crosshair;

        protected override void OnAwake()
        {
            HideInteractPrompt();
            HideObjective();
            HideCrisisTimer();
        }

        private void OnEnable()
        {
            if (_healthChannel != null) _healthChannel.OnRaised += SetHealthNormalized;
        }

        private void OnDisable()
        {
            if (_healthChannel != null) _healthChannel.OnRaised -= SetHealthNormalized;
        }

        /// <summary>Set the health bar from a 0..1 value (0 = empty, 1 = full).</summary>
        public void SetHealthNormalized(float t)
        {
            t = Mathf.Clamp01(t);
            if (_healthFill != null) _healthFill.fillAmount = t;
            if (_healthSlider != null) _healthSlider.value = t;
            if (_healthText != null) _healthText.text = Mathf.RoundToInt(t * 100f) + "%";
        }

        public void SetAmmo(int current, int max)
        {
            if (_ammoText != null) _ammoText.text = $"{current} / {max}";
        }

        public void ShowInteractPrompt(string message)
        {
            if (_interactPromptText != null) _interactPromptText.text = message;
            if (_interactPrompt != null) _interactPrompt.SetActive(true);
        }

        public void HideInteractPrompt()
        {
            if (_interactPrompt != null) _interactPrompt.SetActive(false);
        }

        public void ShowObjective(string message)
        {
            if (_objectiveText != null) _objectiveText.text = message;
            if (_objectiveBanner != null) _objectiveBanner.SetActive(true);
        }

        public void HideObjective()
        {
            if (_objectiveBanner != null) _objectiveBanner.SetActive(false);
        }

        /// <summary>
        /// Show the crisis countdown. Pass <paramref name="totalSeconds"/> (the starting time)
        /// to also drain the optional fill image; leave it negative to skip the fill.
        /// </summary>
        public void SetCrisisTimer(float secondsRemaining, float totalSeconds = -1f)
        {
            if (_crisisTimer != null) _crisisTimer.SetActive(true);

            if (_crisisTimerText != null)
            {
                int s = Mathf.CeilToInt(Mathf.Max(0f, secondsRemaining));
                _crisisTimerText.text = $"{s / 60:00}:{s % 60:00}";
            }

            if (_crisisFill != null && totalSeconds > 0f)
                _crisisFill.fillAmount = Mathf.Clamp01(secondsRemaining / totalSeconds);
        }

        public void HideCrisisTimer()
        {
            if (_crisisTimer != null) _crisisTimer.SetActive(false);
        }

        public void ShowCrosshair(bool visible)
        {
            if (_crosshair != null) _crosshair.SetActive(visible);
        }
    }
}
