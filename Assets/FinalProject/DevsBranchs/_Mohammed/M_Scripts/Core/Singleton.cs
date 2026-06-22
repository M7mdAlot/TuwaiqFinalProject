using UnityEngine;

namespace Aegis.Core
{
    /// <summary>
    /// Generic base for persistent, single-instance MonoBehaviours (managers).
    /// Usage: <c>public class GameManager : Singleton&lt;GameManager&gt; { ... }</c>.
    /// Override <see cref="OnAwake"/> in subclasses instead of Awake().
    /// Tier 0 utility — depends on nothing else in the project.
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        /// <summary>The live instance, or null before any has woken / after it is destroyed.</summary>
        public static T Instance { get; private set; }

        [Tooltip("Survive scene loads (DontDestroyOnLoad). Disable for scene-local managers.")]
        [SerializeField] private bool _persistAcrossScenes = true;

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // A duplicate (e.g. carried over from a previous scene) — discard it.
                Destroy(gameObject);
                return;
            }

            Instance = (T)this;

            if (_persistAcrossScenes)
            {
                // DontDestroyOnLoad only works on root objects.
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }

            OnAwake();
        }

        /// <summary>Subclass initialisation hook. Runs once, only for the surviving instance.</summary>
        protected virtual void OnAwake() { }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
