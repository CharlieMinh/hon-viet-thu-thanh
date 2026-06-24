using System;
using System.Collections;
using System.Collections.Generic;
using HonVietThuThanh.Shared;
using UnityEngine;

namespace HonVietThuThanh.Dev2_EnemyWave
{
    /// <summary>
    /// Owns lightweight Phase 1 wave sequencing and wave lifecycle events.
    /// </summary>
    [DisallowMultipleComponent]
    public class WaveManager : MonoBehaviour
    {
        [Serializable]
        public class WaveDefinition
        {
            [Serializable]
            public class EnemyGroup
            {
                public EnemyData enemyData;
                public int count = 3;
                public float spawnInterval = 1.25f;
            }

            [SerializeField] private List<EnemyGroup> enemyGroups = new();

            public List<EnemyGroup> EnemyGroups => enemyGroups;
            public int TotalEnemyCount
            {
                get
                {
                    int total = 0;
                    foreach (var group in enemyGroups) total += group.count;
                    return total;
                }
            }
        }

        [SerializeField] private bool autoStartOnPlay = false;
        [SerializeField] private bool autoAdvanceToNextWave = false;
        [SerializeField] private float nextWaveDelay = 2.5f;
        [SerializeField] private List<WaveDefinition> waves = new();

        private EnemySpawner spawner;
        private Coroutine activeWaveRoutine;
        private Coroutine delayedNextWaveRoutine;
        private int currentWaveIndex = -1;
        private int activeWaveEnemyCount;
        private bool hasSpawnedAllForCurrentWave;

        public bool IsWaveRunning => activeWaveRoutine != null;
        public int CurrentWaveIndex => currentWaveIndex;

        private void Start()
        {
            if (autoStartOnPlay)
            {
                StartPrototype();
            }
            else
            {
                Debug.Log("[WaveManager] Auto start disabled. Waiting for Start Wave request.", this);
            }
        }

        private void OnEnable()
        {
            GameEvents.OnWaveStartRequested += HandleWaveStartRequested;
        }

        private void OnDisable()
        {
            GameEvents.OnWaveStartRequested -= HandleWaveStartRequested;
        }

        private void HandleWaveStartRequested()
        {
            Debug.Log("[WaveManager] Wave start requested via GameEvents.", this);
            if (!IsWaveRunning)
            {
                StartNextWaveManually();
            }
        }

        public void BindSpawner(EnemySpawner enemySpawner)
        {
            spawner = enemySpawner;
        }

        public void StartPrototype()
        {
            if (waves.Count == 0)
            {
                Debug.LogWarning("WaveManager has no waves configured.", this);
                return;
            }

            if (currentWaveIndex < 0 && !IsWaveRunning)
            {
                StartWave(0);
            }
        }

        public void ResetPrototypeState()
        {
            if (activeWaveRoutine != null)
            {
                StopCoroutine(activeWaveRoutine);
                activeWaveRoutine = null;
            }

            if (delayedNextWaveRoutine != null)
            {
                StopCoroutine(delayedNextWaveRoutine);
                delayedNextWaveRoutine = null;
            }

            currentWaveIndex = -1;
            activeWaveEnemyCount = 0;
            hasSpawnedAllForCurrentWave = false;
        }

        public void StartNextWaveManually()
        {
            if (IsWaveRunning)
            {
                Debug.Log("[WaveManager] Cannot start next wave manually: A wave is already running or spawning.", this);
                return;
            }

            int nextWaveIndex = currentWaveIndex < 0 ? 0 : currentWaveIndex + 1;
            Debug.Log($"[WaveManager] Manually requesting wave {nextWaveIndex}.", this);
            StartWave(nextWaveIndex);
        }

        public EnemyData GetDebugEnemyData()
        {
            if (waves.Count > 0 && waves[0].EnemyGroups.Count > 0)
            {
                return waves[0].EnemyGroups[0].enemyData;
            }

            return null;
        }

        public void NotifyWaveEnemySpawned()
        {
            activeWaveEnemyCount++;
        }

        public void NotifyWaveEnemyResolved()
        {
            activeWaveEnemyCount = Mathf.Max(0, activeWaveEnemyCount - 1);
            TryCompleteWave();
        }

        private void StartWave(int waveIndex)
        {
            if (spawner == null)
            {
                Debug.LogError("WaveManager requires an EnemySpawner binding.", this);
                return;
            }

            if (waveIndex < 0 || waveIndex >= waves.Count)
            {
                Debug.Log("WaveManager has no more waves to start.", this);
                return;
            }

            currentWaveIndex = waveIndex;
            activeWaveEnemyCount = 0;
            hasSpawnedAllForCurrentWave = false;
            activeWaveRoutine = StartCoroutine(RunWaveRoutine(waves[waveIndex], waveIndex));
        }

        private IEnumerator RunWaveRoutine(WaveDefinition waveDefinition, int waveIndex)
        {
            GameEvents.RaiseWaveStarted(waveIndex);

            foreach (var group in waveDefinition.EnemyGroups)
            {
                if (group.enemyData == null) continue;

                for (int i = 0; i < group.count; i++)
                {
                    spawner.SpawnWaveEnemy(group.enemyData);
                    yield return new WaitForSeconds(group.spawnInterval);
                }
            }

            hasSpawnedAllForCurrentWave = true;
            activeWaveRoutine = null;
            TryCompleteWave();
        }

        private void TryCompleteWave()
        {
            if (!hasSpawnedAllForCurrentWave || activeWaveEnemyCount > 0)
            {
                return;
            }

            int completedWaveIndex = currentWaveIndex;
            hasSpawnedAllForCurrentWave = false;
            GameEvents.RaiseWaveCompleted(completedWaveIndex);

            if (autoAdvanceToNextWave && completedWaveIndex + 1 < waves.Count)
            {
                delayedNextWaveRoutine = StartCoroutine(StartNextWaveAfterDelay(completedWaveIndex + 1));
            }
            else
            {
                Debug.Log($"[WaveManager] Wave {completedWaveIndex} completed. Waiting for manual next wave request.", this);
            }
        }

        private IEnumerator StartNextWaveAfterDelay(int nextWaveIndex)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, nextWaveDelay));
            delayedNextWaveRoutine = null;

            if (!IsWaveRunning)
            {
                StartWave(nextWaveIndex);
            }
        }
    }
}
