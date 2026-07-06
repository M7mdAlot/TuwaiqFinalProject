using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Aegis.Core;
using Aegis.Core.Events;

namespace Aegis.Systems
{
    /// <summary>
    /// Drives the in-game HUD (GDD §14). Text slots are <see cref="UILabel"/> so they accept
    /// the SCI-FI UI Pack Pro's legacy Text OR the teammate's TMP. Covers: health + ammo,
    /// a damage screen overlay, the interact prompt, the objective/crisis timer, a crosshair,
    /// and TWO objective displays — a permanent one and an animated popup.
    ///
    /// Objectives: if Jury's <see cref="ObjectiveUIManager"/> is assigned, everything routes
    /// through it (it already does both the popup and the persistent line). Otherwise our own
    /// permanent label + popup are used. Every reference is optional (null-safe).
    /// (Pause + settings are handled separately by PauseMenuController.)
    /// Tier 1/5.
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        [Header("Health (filled Image OR Slider)")]
        [SerializeField] private Image _healthFill;
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private UILabel _healthText;
        [Tooltip("If set, the health bar updates automatically from this channel (0..1).")]
        [SerializeField] private FloatEventChannelSO _healthChannel;

        [Header("Ammo & weapon")]
        [SerializeField] private UILabel _ammoText;
        [SerializeField] private UILabel _weaponNameText;

        [Header("Damage screen overlay")]
        [Tooltip("A full-screen 'got hit' overlay with a CanvasGroup — flashes on damage, then fades.")]
        [SerializeField] private CanvasGroup _damageOverlay;
        [Range(0f, 1f)][SerializeField] private float _damageFlashAlpha = 0.7f;
        [SerializeField] private float _damageFadeSpeed = 2f;

        [Header("Interaction prompt")]
        [SerializeField] private GameObject _interactPrompt;
        [SerializeField] private UILabel _interactPromptText;

        [Header("Objectives — Jury's ObjectiveUIManager (does both if set)")]
        [SerializeField] private ObjectiveUIManager _objectiveUI;

        [Header("...or our own two objectives (used if the above is empty)")]
        [Tooltip("Permanent objective, usually top-left.")]
        [SerializeField] private UILabel _persistentObjectiveText;
        [Tooltip("Animated popup shown in the middle when the objective changes.")]
        [SerializeField] private GameObject _objectivePopup;
        [SerializeField] private UILabel _objectivePopupTitle;
        [SerializeField] private UILabel _objectivePopupText;
        [SerializeField] private float _objectivePopupDuration = 3f;

        [Header("Objective / crisis timer")]
        [SerializeField] private GameObject _crisisTimer;
        [SerializeField] private UILabel _crisisTimerText;
        [Tooltip("Optional: a filled Image that drains as time runs out.")]
        [SerializeField] private Image _crisisFill;

        [Header("Other")]
        [SerializeField] private GameObject _crosshair;

        private Coroutine _popupRoutine;

        protected override void OnAwake()
        {
            HideInteractPrompt();
            HideCrisisTimer();
            if (_objectivePopup != null) _objectivePopup.SetActive(false);
            if (_damageOverlay != null) _damageOverlay.alpha = 0f;
        }

        private void OnEnable()
        {
            if (_healthChannel != null) _healthChannel.OnRaised += SetHealthNormalized;
        }

        private void OnDisable()
        {
            if (_healthChannel != null) _healthChannel.OnRaised -= SetHealthNormalized;
        }

        private void Update()
        {
            // Fade the damage overlay back out.
            if (_damageOverlay != null && _damageOverlay.alpha > 0f)
                _damageOverlay.alpha = Mathf.MoveTowards(_damageOverlay.alpha, 0f, _damageFadeSpeed * Time.deltaTime);
        }

        // ---- Health ----
        public void SetHealthNormalized(float t)
        {
            t = Mathf.Clamp01(t);
            if (_healthFill != null) _healthFill.fillAmount = t;
            if (_healthSlider != null) _healthSlider.value = t;
            _healthText?.Set(Mathf.RoundToInt(t * 100f) + "%");
        }

        // ---- Ammo / weapon ----
        public void SetAmmo(int current, int max) => _ammoText?.Set(max < 0 ? "∞" : $"{current} / {max}");
        public void SetWeaponName(string weaponName) => _weaponNameText?.Set(weaponName);

        // ---- Damage overlay ----
        /// <summary>Flash the "got hit" overlay. Intensity 0..1 scales how strong the flash is.</summary>
        public void FlashDamage(float intensity = 1f)
        {
            if (_damageOverlay == null) return;
            float target = _damageFlashAlpha * Mathf.Clamp01(intensity);
            _damageOverlay.alpha = Mathf.Max(_damageOverlay.alpha, target);
        }

        // ---- Interaction ----
        public void ShowInteractPrompt(string message)
        {
            _interactPromptText?.Set(message);
            if (_interactPrompt != null) _interactPrompt.SetActive(true);
        }

        public void HideInteractPrompt()
        {
            if (_interactPrompt != null) _interactPrompt.SetActive(false);
        }

        // ---- Objectives ----
        public void ShowObjective(string message) => ShowObjective("NEW OBJECTIVE", message);

        public void ShowObjective(string title, string message)
        {
            if (_objectiveUI != null)
            {
                _objectiveUI.ShowObjective(title, message); // Jury's UI does popup + persistent
                return;
            }

            SetPersistentObjective(message);
            ShowObjectivePopup(title, message);
        }

        /// <summary>Set only the permanent objective line (no popup).</summary>
        public void SetPersistentObjective(string message)
        {
            if (_objectiveUI != null && _objectiveUI.persistentObjectiveText != null)
                _objectiveUI.persistentObjectiveText.text = message;
            else
                _persistentObjectiveText?.Set(message);
        }

        /// <summary>Play the middle-screen objective popup for a few seconds.</summary>
        public void ShowObjectivePopup(string title, string message)
        {
            if (_objectivePopup == null) return;

            _objectivePopupTitle?.Set(title);
            _objectivePopupText?.Set(message);

            if (_popupRoutine != null) StopCoroutine(_popupRoutine);
            _popupRoutine = StartCoroutine(PopupRoutine());
        }

        private IEnumerator PopupRoutine()
        {
            _objectivePopup.SetActive(true);                 // its own Animator (if any) plays on enable
            yield return new WaitForSecondsRealtime(_objectivePopupDuration);
            _objectivePopup.SetActive(false);
            _popupRoutine = null;
        }

        public void HideObjective()
        {
            SetPersistentObjective(string.Empty);
            if (_objectivePopup != null) _objectivePopup.SetActive(false);
        }

        // ---- Crisis / objective timer ----
        public void SetCrisisTimer(float secondsRemaining, float totalSeconds = -1f)
        {
            if (_crisisTimer != null) _crisisTimer.SetActive(true);

            int s = Mathf.CeilToInt(Mathf.Max(0f, secondsRemaining));
            _crisisTimerText?.Set($"{s / 60:00}:{s % 60:00}");

            if (_crisisFill != null && totalSeconds > 0f)
                _crisisFill.fillAmount = Mathf.Clamp01(secondsRemaining / totalSeconds);
        }

        public void HideCrisisTimer()
        {
            if (_crisisTimer != null) _crisisTimer.SetActive(false);
        }

        // ---- Crosshair ----
        public void ShowCrosshair(bool visible)
        {
            if (_crosshair != null) _crosshair.SetActive(visible);
        }
    }
}
