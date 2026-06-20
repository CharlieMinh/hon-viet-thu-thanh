using System.Collections.Generic;
using HonVietThuThanh.Shared;
using UnityEngine;

namespace HonVietThuThanh.Dev2_EnemyWave
{
    /// <summary>
    /// Scene-level entrypoint for the Dev2 enemy wave prototype.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private List<LanePath> lanePaths = new();
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private EnemyPool enemyPool;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Transform enemyRoot;

        private readonly List<Enemy> activeEnemies = new();
        private bool isResetting;

        public IReadOnlyList<Enemy> ActiveEnemies => activeEnemies;

        private void Awake()
        {
            AutoAssignReferences();

            if (enemyPool != null)
            {
                enemyPool.Configure(enemyPrefab, enemyRoot);
            }

            if (waveManager != null)
            {
                waveManager.BindSpawner(this);
            }
        }

        public void StartPrototype()
        {
            waveManager?.StartPrototype();
        }

        public void SpawnSingleEnemy()
        {
            SpawnEnemy(waveManager != null ? waveManager.GetDebugEnemyData() : null, false);
        }

        public void SpawnWaveEnemy(EnemyData data)
        {
            if (SpawnEnemy(data, true) != null)
            {
                waveManager?.NotifyWaveEnemySpawned();
            }
        }

        public void ResetPrototype()
        {
            isResetting = true;
            waveManager?.ResetPrototypeState();

            Enemy[] snapshot = activeEnemies.ToArray();
            for (int i = 0; i < snapshot.Length; i++)
            {
                Enemy enemy = snapshot[i];
                if (enemy == null)
                {
                    continue;
                }

                enemy.ForceDespawnWithoutEvents();
                enemyPool?.Release(enemy);
            }

            activeEnemies.Clear();
            enemyPool?.ResetPool();
            isResetting = false;
        }

        public void DamageFirstAliveEnemy(float damageAmount)
        {
            for (int i = 0; i < activeEnemies.Count; i++)
            {
                Enemy enemy = activeEnemies[i];
                if (enemy != null && enemy.IsAlive())
                {
                    enemy.TakeDamage(damageAmount);
                    return;
                }
            }
        }

        public void NotifyEnemyReleased(Enemy enemy, bool countedTowardWave)
        {
            activeEnemies.Remove(enemy);

            if (!isResetting && countedTowardWave)
            {
                waveManager?.NotifyWaveEnemyResolved();
            }
        }

        private Enemy SpawnEnemy(EnemyData data, bool countTowardWave)
        {
            if (lanePaths == null || lanePaths.Count == 0)
            {
                Debug.LogError("EnemySpawner requires at least one LanePath.", this);
                return null;
            }

            // Pick a random lane
            LanePath lanePath = lanePaths[Random.Range(0, lanePaths.Count)];

            if (lanePath == null || !lanePath.HasValidPath)
            {
                Debug.LogError("Selected LanePath is invalid.", this);
                return null;
            }

            if (enemyPool == null)
            {
                Debug.LogError("EnemySpawner requires an EnemyPool.", this);
                return null;
            }

            Enemy enemy = enemyPool.Get();
            if (enemy == null)
            {
                return null;
            }

            Enemy preparedEnemy = EnemyBootstrapper.PrepareEnemy(
                enemy,
                data,
                lanePath,
                enemyPool,
                this,
                enemyRoot,
                countTowardWave);

            if (preparedEnemy == null)
            {
                return null;
            }

            activeEnemies.Add(preparedEnemy);
            GameEvents.RaiseEnemySpawned(preparedEnemy.EnemyType, preparedEnemy.gameObject);
            return preparedEnemy;
        }

        private void AutoAssignReferences()
        {
            if (lanePaths == null || lanePaths.Count == 0)
            {
                LanePath foundPath = transform.root.GetComponentInChildren<LanePath>(true);
                if (foundPath != null)
                {
                    lanePaths = new List<LanePath> { foundPath };
                }
            }

            if (waveManager == null)
            {
                waveManager = transform.root.GetComponentInChildren<WaveManager>(true);
            }

            if (enemyPool == null)
            {
                enemyPool = GetComponent<EnemyPool>();
            }

            if (enemyRoot == null)
            {
                Transform foundRoot = transform.root.Find("Dev2_PrototypeRoot/EnemyRoot");
                if (foundRoot != null)
                {
                    enemyRoot = foundRoot;
                }
            }
        }
    }
}
