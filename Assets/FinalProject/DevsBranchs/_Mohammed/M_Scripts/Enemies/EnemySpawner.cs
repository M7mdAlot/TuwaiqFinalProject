using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Aegis.Core;
using Aegis.Core.Events;
using Aegis.Player;

namespace Aegis.Enemies
{
    /// <summary>
    /// Spawns waves of enemies at a set of spawn points. Watches each spawned enemy's
    /// <see cref="HealthSystem"/> for death, so it always knows how many are still alive
    /// and when a wave has been fully cleared. Fires events for wave start, wave cleared,
    /// and all-waves cleared — used by <see cref="Aegis.Systems.CrisisManager"/> and by
    /// scripted sequences.
    /// Tier 3 — depends on Tier 1 (HealthSystem) + Tier 0 (events).
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn points (random pick per enemy)")]
        [Tooltip("Enemies spawn at a random point in this list. Leave empty to spawn on this GameObject.")]
        [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();

        [Header("Waves")]
        [SerializeField] private List<EnemyWave> _waves = new List<EnemyWave>();
        [Tooltip("Start the first wave automatically when the scene loads.")]
        [SerializeField] private bool _autoStartOnAwake = false;

        [Header("Broadcast (optional)")]
        [SerializeField] private VoidEventChannelSO _onAllWavesClearedChannel;

        // ---- Wave data (edit in Inspector) ----
        [Serializable]
        public class EnemyWave
        {
            [Tooltip("Inspector label only.")]
            public string label = "Wave";
            [Tooltip("Seconds to wait before this wave starts spawning.")]
            public float delayBeforeStart = 0f;
            [Tooltip("Wait for all this wave's enemies to die before advancing to the next wave.")]
            public bool waitForClearBeforeNext = true;
            [Tooltip("List of (enemy prefab, count) pairs to spawn for this wave.")]
            public List<EnemySpawnGroup> spawns = new List<EnemySpawnGroup>();
        }

        [Serializable]
        public class EnemySpawnGroup
        {
            [Tooltip("A prefab that has EnemyController + HealthSystem etc.")]
            public GameObject enemyPrefab;
            [Min(1)] public int count = 1;
        }

        // ---- Runtime state ----
        public int CurrentWaveIndex { get; private set; } = -1;
        public int AliveCount => _aliveEnemies.Count;
        public int TotalSpawned { get; private set; }
        public int TotalKilled { get; private set; }
        public float FractionCleared => TotalSpawned > 0 ? (float)TotalKilled / TotalSpawned : 0f;
        public bool AllWavesDone { get; private set; }

        public event Action<int> WaveStarted;
        public event Action<int> WaveCleared;
        public event Action AllWavesCleared;

        private readonly HashSet<HealthSystem> _aliveEnemies = new HashSet<HealthSystem>();
        private Coroutine _running;

        private void Start()
        {
            if (_autoStartOnAwake) StartWaves();
        }

        /// <summary>Kick off the wave loop from wave 0.</summary>
        public void StartWaves()
        {
            if (_running != null) return;
            _running = StartCoroutine(RunWaves());
        }

        public void StopWaves()
        {
            if (_running != null) StopCoroutine(_running);
            _running = null;
        }

        private IEnumerator RunWaves()
        {
            for (int i = 0; i < _waves.Count; i++)
            {
                CurrentWaveIndex = i;
                EnemyWave wave = _waves[i];

                if (wave.delayBeforeStart > 0f) yield return new WaitForSeconds(wave.delayBeforeStart);

                SpawnWave(wave);
                WaveStarted?.Invoke(i);

                if (wave.waitForClearBeforeNext)
                {
                    // Wait until every enemy in the tracking set dies.
                    yield return new WaitUntil(() => _aliveEnemies.Count == 0);
                    WaveCleared?.Invoke(i);
                }
            }

            AllWavesDone = true;
            AllWavesCleared?.Invoke();
            _onAllWavesClearedChannel?.Raise();
            _running = null;
        }

        private void SpawnWave(EnemyWave wave)
        {
            foreach (EnemySpawnGroup group in wave.spawns)
            {
                if (group.enemyPrefab == null) continue;
                for (int i = 0; i < group.count; i++) SpawnOne(group.enemyPrefab);
            }
        }

        private void SpawnOne(GameObject prefab)
        {
            Transform point = PickSpawnPoint();
            GameObject go = Instantiate(prefab, point.position, point.rotation);

            HealthSystem hp = go.GetComponent<HealthSystem>();
            if (hp != null) TrackEnemy(hp);

            TotalSpawned++;
        }

        private Transform PickSpawnPoint()
        {
            if (_spawnPoints.Count == 0) return transform;
            return _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Count)];
        }

        private void TrackEnemy(HealthSystem hp)
        {
            _aliveEnemies.Add(hp);

            // Self-unsubscribing handler: when this enemy dies, remove it and detach.
            Action handler = null;
            handler = () =>
            {
                if (_aliveEnemies.Remove(hp)) TotalKilled++;
                hp.Died -= handler;
            };
            hp.Died += handler;
        }
    }
}
