using HonVietThuThanh.Shared;
using UnityEngine;

namespace HonVietThuThanh.Dev2_EnemyWave
{
    /// <summary>
    /// Phase 1 placeholder enemy that supports movement, damage, death, and
    /// base-reach events without exposing Dev2 internals to other modules.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyMover))]
    public class Enemy : MonoBehaviour, IDamageable, ITargetable
    {
        [Header("Runtime Readonly")]
        [SerializeField] private EnemyType enemyType = EnemyType.LinhXamLuoc;
        [SerializeField] private float maxHealth = 15f;
        [SerializeField] private float currentHealth = 15f;
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private int goldReward = 10;
        [SerializeField] private bool countsTowardWave = true;
        [SerializeField] private bool isAlive = true;
        [SerializeField] private bool hasReachedBase;

        private EnemyMover mover;
        private EnemyPool ownerPool;
        private EnemySpawner ownerSpawner;

        public EnemyType EnemyType => enemyType;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public int GoldReward => goldReward;
        public bool CountsTowardWave => countsTowardWave;

        private void Awake()
        {
            mover = GetComponent<EnemyMover>();
        }

        public void Initialize(
            EnemySpawnProfile profile,
            LanePath lanePath,
            EnemyPool pool,
            EnemySpawner spawner,
            bool shouldCountTowardWave)
        {
            enemyType = profile != null ? profile.EnemyType : EnemyType.LinhXamLuoc;
            maxHealth = profile != null ? Mathf.Max(1f, profile.MaxHealth) : 15f;
            currentHealth = maxHealth;
            moveSpeed = profile != null ? Mathf.Max(0.1f, profile.MoveSpeed) : 2.5f;
            goldReward = profile != null ? Mathf.Max(0, profile.GoldReward) : 10;
            countsTowardWave = shouldCountTowardWave;
            isAlive = true;
            hasReachedBase = false;
            ownerPool = pool;
            ownerSpawner = spawner;

            gameObject.name = $"Enemy_{enemyType}";
            mover.Initialize(lanePath, this, moveSpeed);
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive())
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - Mathf.Max(0f, amount));

            if (currentHealth <= 0f)
            {
                HandleDeath();
            }
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public bool IsAlive()
        {
            return isAlive && !hasReachedBase;
        }

        public void MarkReachedBase()
        {
            if (!IsAlive())
            {
                return;
            }

            isAlive = false;
            hasReachedBase = true;
            mover.StopMovement();
            GameEvents.RaiseEnemyReachedBase(gameObject);
            ReleaseSelf();
        }

        public void ForceDespawnWithoutEvents()
        {
            isAlive = false;
            hasReachedBase = false;
            currentHealth = 0f;
            mover.StopMovement();
        }

        private void HandleDeath()
        {
            if (!IsAlive())
            {
                return;
            }

            isAlive = false;
            mover.StopMovement();
            GameEvents.RaiseEnemyDied(gameObject, goldReward);
            ReleaseSelf();
        }

        private void ReleaseSelf()
        {
            if (ownerSpawner != null)
            {
                ownerSpawner.NotifyEnemyReleased(this, countsTowardWave);
            }

            if (ownerPool != null)
            {
                ownerPool.Release(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
