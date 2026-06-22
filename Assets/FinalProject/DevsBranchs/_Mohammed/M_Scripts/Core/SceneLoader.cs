using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Aegis.Core
{
    /// <summary>
    /// Loads and unloads scenes (single or additive) and reports progress via events,
    /// so a loading screen / progress bar can react. Persists across scene loads (singleton).
    /// Tier 0 — no dependencies on other game scripts.
    /// </summary>
    public class SceneLoader : Singleton<SceneLoader>
    {
        public event Action<string> LoadStarted;
        public event Action<float> LoadProgress;   // 0..1
        public event Action<string> LoadCompleted;

        /// <summary>Load a scene, replacing the current one.</summary>
        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadRoutine(sceneName, LoadSceneMode.Single));
        }

        /// <summary>Load a scene on top of the current one (e.g. a streamed sub-area).</summary>
        public void LoadSceneAdditive(string sceneName)
        {
            StartCoroutine(LoadRoutine(sceneName, LoadSceneMode.Additive));
        }

        /// <summary>Unload an additively-loaded scene.</summary>
        public void UnloadScene(string sceneName)
        {
            SceneManager.UnloadSceneAsync(sceneName);
        }

        private IEnumerator LoadRoutine(string sceneName, LoadSceneMode mode)
        {
            LoadStarted?.Invoke(sceneName);

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, mode);
            while (!op.isDone)
            {
                // Unity reports 0 -> 0.9 while loading, so rescale to a clean 0..1.
                LoadProgress?.Invoke(Mathf.Clamp01(op.progress / 0.9f));
                yield return null;
            }

            LoadCompleted?.Invoke(sceneName);
        }
    }
}
