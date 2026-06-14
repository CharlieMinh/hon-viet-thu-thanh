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
            [SerializeField] private EnemySpawnProfile enemyProfile = new();
            [SerializeField] private int enemyCount = 3;
            [SerializeField] private float spawnInterval = 1.25f;

            public EnemySpawnProfile EnemyProfile => enemyProfile;
            public int EnemyCount => Mathf.Max(1, enemyCount);
            public float SpawnInterval => Mathf.Max(0.1f, spawnInterval);
        }

        [SerializeField] private bool autoStartOnPlay = true;
        [SerializeField] private bool autoAdvanceToNextWave = true;
        [SerializeField] private float nextWaveDelay = 1.5f;
        [SerializeField] private List<WaveDefinition> waves = new()
        {
            new WaveDefinition(),
            new WaveDefinition()
        };

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
                return;
            }

            int nextWaveIndex = currentWaveIndex < 0 ? 0 : currentWaveIndex + 1;
            StartWave(nextWaveIndex);
        }

        public EnemySpawnProfile GetDebugSpawnProfile()
        {
            if (waves.Count > 0 && waves[0] != null)
            {
                return waves[0].EnemyProfile;
            }

            return new EnemySpawnProfile();
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

            for (int i = 0; i < waveDefinition.EnemyCount; i++)
            {
                spawner.SpawnWaveEnemy(waveDefinition.EnemyProfile);
                yield return new WaitForSeconds(waveDefinition.SpawnInterval);
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
