using System.Collections.Generic;
using UnityEngine;

namespace HonVietThuThanh.Dev2_EnemyWave
{
    /// <summary>
    /// Small pool for Phase 1 placeholder enemies.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyPool : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Transform pooledRoot;
        [SerializeField] private int initialSize = 4;

        private readonly Queue<Enemy> availableEnemies = new();
        private readonly List<Enemy> allEnemies = new();

        public void Configure(GameObject prefab, Transform root)
        {
            if (prefab != null)
            {
                enemyPrefab = prefab;
            }

            pooledRoot = root != null ? root : transform;
            EnsureWarmPool();
        }

        public Enemy Get()
        {
            EnsureWarmPool();

            Enemy enemy = availableEnemies.Count > 0 ? availableEnemies.Dequeue() : CreateInstance();
            if (enemy == null)
            {
                return null;
            }

            enemy.gameObject.SetActive(true);
            return enemy;
        }

        public void Release(Enemy enemy)
        {
            if (enemy == null)
            {
                return;
            }

            enemy.transform.SetParent(pooledRoot != null ? pooledRoot : transform, false);
            enemy.gameObject.SetActive(false);

            if (!availableEnemies.Contains(enemy))
            {
                availableEnemies.Enqueue(enemy);
            }
        }

        public void ResetPool()
        {
            foreach (Enemy enemy in allEnemies)
            {
                if (enemy == null)
                {
                    continue;
                }

                enemy.transform.SetParent(pooledRoot != null ? pooledRoot : transform, false);
                enemy.gameObject.SetActive(false);
            }

            availableEnemies.Clear();

            foreach (Enemy enemy in allEnemies)
            {
                if (enemy != null)
                {
                    availableEnemies.Enqueue(enemy);
                }
            }
        }

        private void Awake()
        {
            if (pooledRoot == null)
            {
                pooledRoot = transform;
            }
        }

        private void EnsureWarmPool()
        {
            if (enemyPrefab == null)
            {
                return;
            }

            if (pooledRoot == null)
            {
                pooledRoot = transform;
            }

            while (allEnemies.Count < Mathf.Max(1, initialSize))
            {
                Enemy createdEnemy = CreateInstance();
                if (createdEnemy == null)
                {
                    break;
                }

                availableEnemies.Enqueue(createdEnemy);
            }
        }

        private Enemy CreateInstance()
        {
            if (enemyPrefab == null)
            {
                return CreateFallbackInstance();
            }

            GameObject instance = Instantiate(enemyPrefab, pooledRoot != null ? pooledRoot : transform);
            instance.name = enemyPrefab.name;
            instance.SetActive(false);

            Enemy enemy = instance.GetComponent<Enemy>();
            if (enemy == null)
            {
                Debug.LogError("Enemy prefab must contain an Enemy component.", enemyPrefab);
                Destroy(instance);
                return null;
            }

            allEnemies.Add(enemy);
            return enemy;
        }

        private Enemy CreateFallbackInstance()
        {
            GameObject instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            instance.name = "Enemy_Fallback";
            instance.transform.SetParent(pooledRoot != null ? pooledRoot : transform, false);
            instance.transform.localScale = Vector3.one * 0.8f;
            instance.transform.localPosition = new Vector3(0f, 0.5f, 0f);

            EnemyMover mover = instance.GetComponent<EnemyMover>();
            if (mover == null)
            {
                mover = instance.AddComponent<EnemyMover>();
            }

            Enemy enemy = instance.GetComponent<Enemy>();
            if (enemy == null)
            {
                enemy = instance.AddComponent<Enemy>();
            }

            instance.SetActive(false);
            allEnemies.Add(enemy);
            return enemy;
        }
    }
}
