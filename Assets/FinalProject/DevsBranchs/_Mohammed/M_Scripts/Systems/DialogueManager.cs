using System;
using System.Collections;
using UnityEngine;
using Aegis.Core;

namespace Aegis.Systems
{
    /// <summary>
    /// Plays the lines of a <see cref="DialogueData"/> set for a given story "beat", one after
    /// another (GDD §7). Speaks the optional voice clip through the <see cref="AudioManager"/>
    /// and raises events so a subtitle UI can show the text. The active dialogue set is chosen
    /// per campaign (AEGIS.2 vs X).
    /// Tier 1 — depends only on Tier 0 (DialogueData, AudioManager).
    /// </summary>
    public class DialogueManager : Singleton<DialogueManager>
    {
        [SerializeField] private DialogueData _activeSet;
        [Tooltip("How long to show a line that has no voice clip.")]
        [SerializeField] private float _defaultLineDuration = 3f;

        public event Action<DialogueLine> LineStarted;
        public event Action LineFinished;
        public event Action BeatFinished;

        private Coroutine _running;

        /// <summary>Choose which side's dialogue to use (called by the campaign setup).</summary>
        public void SetDialogue(DialogueData data) => _activeSet = data;

        /// <summary>Play every line tagged with <paramref name="beat"/>, in order.</summary>
        public void PlayBeat(string beat)
        {
            if (_activeSet == null)
            {
                Debug.LogWarning("DialogueManager: no DialogueData set.", this);
                return;
            }

            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(PlayRoutine(beat));
        }

        public void StopDialogue()
        {
            if (_running != null) StopCoroutine(_running);
            _running = null;
        }

        private IEnumerator PlayRoutine(string beat)
        {
            foreach (DialogueLine line in _activeSet.lines)
            {
                if (line.beat != beat) continue;

                LineStarted?.Invoke(line);

                float wait = _defaultLineDuration;
                if (line.voiceClip != null)
                {
                    wait = line.voiceClip.length;
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(line.voiceClip);
                }

                yield return new WaitForSeconds(wait);
                LineFinished?.Invoke();
            }

            BeatFinished?.Invoke();
            _running = null;
        }
    }
}
