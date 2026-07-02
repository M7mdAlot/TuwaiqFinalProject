using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Aegis.Core;
using Aegis.Input;

namespace Aegis.Systems
{
    /// <summary>
    /// The convergent ending (GDD §7): both campaigns reach the lab entrance and are gunned
    /// down by the military. Same script, per-side dialogue and text driven by CampaignConfig.
    ///
    /// This is the *scriptable* stub for the ending — it plays the right dialogue beat,
    /// disables player input, waits, then advances to Results. When you're ready for a real
    /// cutscene, wire a Timeline PlayableDirector into <see cref="OnEndingStarted"/> and let
    /// this script just gatekeep the flow.
    /// Tier 4 — depends on Tier 0 (GameManager, InputReader) + Tier 1 (DialogueManager).
    /// </summary>
    public class EndingSequencer : MonoBehaviour
    {
        [Header("Timing")]
        [Tooltip("Play the ending as soon as this GameObject is enabled.")]
        [SerializeField] private bool _autoStartOnEnable = false;
        [Tooltip("How long the ending stays on-screen before advancing to Results.")]
        [SerializeField] private float _totalDuration = 8f;

        [Header("Dialogue")]
        [Tooltip("The beat key inside the active DialogueData to play (e.g. \"ending\").")]
        [SerializeField] private string _dialogueBeat = "ending";

        [Header("Player control")]
        [Tooltip("Input to disable during the ending so the player can't move.")]
        [SerializeField] private InputReader _inputReader;

        [Header("Inspector hooks")]
        public UnityEvent OnEndingStarted;
        public UnityEvent OnEndingFinished;

        public bool IsPlaying { get; private set; }

        private void OnEnable()
        {
            if (_autoStartOnEnable) PlayEnding();
        }

        /// <summary>Start the ending. Safe to call from a UnityEvent (e.g. player reaches exit).</summary>
        public void PlayEnding()
        {
            if (IsPlaying) return;
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            IsPlaying = true;

            if (GameManager.Instance != null) GameManager.Instance.GoToEnding();
            if (_inputReader != null) _inputReader.DisableGameplay();
            OnEndingStarted?.Invoke();

            // Play the per-side ending dialogue (dialogue set was assigned earlier by CampaignManager).
            if (DialogueManager.Instance != null && !string.IsNullOrEmpty(_dialogueBeat))
                DialogueManager.Instance.PlayBeat(_dialogueBeat);

            yield return new WaitForSeconds(_totalDuration);

            OnEndingFinished?.Invoke();
            if (GameManager.Instance != null) GameManager.Instance.GoToResults();

            IsPlaying = false;
        }
    }
}
