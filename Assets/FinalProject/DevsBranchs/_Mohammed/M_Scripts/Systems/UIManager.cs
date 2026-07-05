using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Aegis.Core;
using Aegis.Core.Events;

namespace Aegis.Systems
{
    /// <summary>
    /// Drives the on-screen HUD (GDD §14) by setting values on your UI widgets. Text uses
    /// TextMeshPro (<see cref="TMP_Text"/>) to match the teammate's (Jury's) UI, and bars use
    /// a filled Image or a Slider from the SCI-FI UI Pack Pro. Every reference is optional
    /// (null-safe), so the game runs before the HUD is fully assembled — and it never edits
    /// any purchased/teammate asset, only reads/sets their components.
    ///
    /// Objectives can delegate to Jury's animated <see cref="ObjectiveUIManager"/> if assigned;
    /// otherwise a simple built-in banner is used as a fallback.
    /// Tier 1/5 — depends on Tier 0 (event channels) + Unity UI + TMP.
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        [Header("Health (assign a filled Image OR a Slider — whichever your bar uses)")]
        [Tooltip("An Image whose Image Type = Filled (most SCI-FI bars work this way).")]
        [SerializeField] private Image _healthFill;
        [Tooltip("Alternative: a Slider-style bar (Min Value 0, Max Value 1).")]
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private TMP_Text _healthText;
        [Tooltip("If set, the health bar updates automatically from this channel (0..1).")]
        [SerializeField] private FloatEventChannelSO _healthChannel;

        [Header("Ammo & weapon")]
        [SerializeField] private TMP_Text _ammoText;
        [SerializeField] private TMP_Text _weaponNameText;

        [Header("Interact prompt")]
        [SerializeField] private GameObject _interactPrompt;
        [SerializeField] private TMP_Text _interactPromptText;

        [Header("Objective (delegates to Jury's ObjectiveUIManager if set)")]
        [Tooltip("Optional: the teammate's animated objective UI. If set, objectives route through it.")]
        [SerializeField] private ObjectiveUIManager _objectiveUI;
        [Tooltip("Fallback banner used only when the ObjectiveUIManager above is empty.")]
        [SerializeField] private GameObject _objectiveBanner;
        [SerializeField] private TMP_Text _objectiveText;

        [Header("Crisis timer (e.g. the pack's Timer01 widget)")]
        [SerializeField] private GameObject _crisisTimer;
        [SerializeField] private TMP_Text _crisisTimerText;
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
            if (_ammoText != null)
                _ammoText.text = max < 0 ? "∞" : $"{current} / {max}"; // ∞ for infinite-ammo weapons
        }

        public void SetWeaponName(string weaponName)
        {
            if (_weaponNameText != null) _weaponNameText.text = weaponName;
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

        /// <summary>Show a new objective. Routes to Jury's animated UI if assigned.</summary>
        public void ShowObjective(string message) => ShowObjective("NEW OBJECTIVE", message);

        public void ShowObjective(string title, string message)
        {
            if (_objectiveUI != null)
            {
                _objectiveUI.ShowObjective(title, message);
                return;
            }

            // Fallback: our own simple banner.
            if (_objectiveText != null) _objectiveText.text = message;
            if (_objectiveBanner != null) _objectiveBanner.SetActive(true);
        }

        public void HideObjective()
        {
            // Jury's UI auto-hides; just clear its persistent line if present.
            if (_objectiveUI != null && _objectiveUI.persistentObjectiveText != null)
                _objectiveUI.persistentObjectiveText.text = string.Empty;

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
