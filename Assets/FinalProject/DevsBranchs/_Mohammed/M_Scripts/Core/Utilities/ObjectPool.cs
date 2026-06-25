using System.Collections.Generic;
using UnityEngine;

namespace Aegis.Core
{
    /// <summary>
    /// Reuses spawned objects instead of constantly creating and destroying them
    /// (much cheaper for things spawned a lot, like projectiles and hit effects).
    /// <typeparamref name="T"/> is a Component on a prefab (e.g. a Projectile script).
    /// Tier 0 — no dependencies.
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _available = new Queue<T>();

        public ObjectPool(T prefab, int prewarm = 0, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;

            // Optionally create some up front so the first uses don't cause a hitch.
            for (int i = 0; i < prewarm; i++)
            {
                T instance = CreateNew();
                instance.gameObject.SetActive(false);
                _available.Enqueue(instance);
            }
        }

        /// <summary>Take an object from the pool (creates one if none are free).</summary>
        public T Get()
        {
            T instance = _available.Count > 0 ? _available.Dequeue() : CreateNew();
            instance.gameObject.SetActive(true);
            return instance;
        }

        /// <summary>Return an object to the pool so it can be reused.</summary>
        public void Release(T instance)
        {
            instance.gameObject.SetActive(false);
            _available.Enqueue(instance);
        }

        private T CreateNew() => Object.Instantiate(_prefab, _parent);
    }
}
