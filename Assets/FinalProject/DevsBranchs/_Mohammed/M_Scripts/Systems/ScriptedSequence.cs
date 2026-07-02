using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Aegis.Systems
{
    /// <summary>
    /// A timeline of scripted steps for a story beat. Each step waits some seconds, then
    /// fires a UnityEvent you wire up in the Inspector — spawn a wave, play a dialogue beat,
    /// activate a GameObject, ring a siren, whatever. Great for one-off scenes like the
    /// AEGIS.2 hallway encounter or X's failsafe moment.
    ///
    /// Start it manually via <see cref="StartSequence"/>, from a TriggerZone's UnityEvent,
    /// or by ticking Auto Start.
    /// Tier 3 — depends only on Unity.
    /// </summary>
    public class ScriptedSequence : MonoBehaviour
    {
        [Serializable]
        public class Step
        {
            [Tooltip("Inspector label only.")]
            public string label = "Step";
            [Tooltip("Seconds to wait before running this step.")]
            public float delayBeforeStep = 0f;
            [Tooltip("What to do — wire up in the Inspector (Debug.Log, activate object, call a method, etc.).")]
            public UnityEvent onExecute;
        }

        [SerializeField] private List<Step> _steps = new List<Step>();
        [Tooltip("Play the sequence automatically when the scene starts.")]
        [SerializeField] private bool _autoStart = false;
        [Tooltip("Stop the sequence if this object is disabled mid-run.")]
        [SerializeField] private bool _stopOnDisable = true;

        [Header("Optional overall hooks")]
        public UnityEvent OnSequenceStarted;
        public UnityEvent OnSequenceFinished;

        public bool IsPlaying => _running != null;
        public int CurrentStepIndex { get; private set; } = -1;

        private Coroutine _running;

        private void Start()
        {
            if (_autoStart) StartSequence();
        }

        private void OnDisable()
        {
            if (_stopOnDisable) StopSequence();
        }

        /// <summary>Play from step 0. If already running, restarts.</summary>
        public void StartSequence()
        {
            StopSequence();
            _running = StartCoroutine(Run());
        }

        public void StopSequence()
        {
            if (_running != null) StopCoroutine(_running);
            _running = null;
        }

        private IEnumerator Run()
        {
            OnSequenceStarted?.Invoke();

            for (int i = 0; i < _steps.Count; i++)
            {
                CurrentStepIndex = i;
                Step step = _steps[i];

                if (step.delayBeforeStep > 0f) yield return new WaitForSeconds(step.delayBeforeStep);
                step.onExecute?.Invoke();
            }

            CurrentStepIndex = -1;
            _running = null;
            OnSequenceFinished?.Invoke();
        }
    }
}
