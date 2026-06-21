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
        [SerializeField] private float damageToBase = 10f;
        [SerializeField] private bool countsTowardWave = true;
        [SerializeField] private bool isAlive = true;
        [SerializeField] private bool hasReachedBase;

        [Header("Visual Feedback")]
        [SerializeField] private Color flashColor = Color.red;
        [SerializeField] private float flashDuration = 0.1f;

        private EnemyMover mover;
        private EnemyPool ownerPool;
        private EnemySpawner ownerSpawner;
        private Renderer _renderer;
        private MaterialPropertyBlock _propBlock;
        private Coroutine _flashRoutine;
        private static readonly int ColorPropId = Shader.PropertyToID("_BaseColor"); // URP default

        public EnemyType EnemyType => enemyType;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public int GoldReward => goldReward;
        public float DamageToBase => damageToBase;
        public bool CountsTowardWave => countsTowardWave;

        private void Awake()
        {
            mover = GetComponent<EnemyMover>();
            _renderer = GetComponentInChildren<Renderer>();
            _propBlock = new MaterialPropertyBlock();
        }

        public void Initialize(
            EnemyData data,
            LanePath lanePath,
            EnemyPool pool,
            EnemySpawner spawner,
            bool shouldCountTowardWave)
        {
            if (mover == null) mover = GetComponent<EnemyMover>();
            
            enemyType = data != null ? data.EnemyType : EnemyType.LinhXamLuoc;
            maxHealth = data != null ? Mathf.Max(1f, data.MaxHealth) : 15f;
            currentHealth = maxHealth;
            moveSpeed = data != null ? Mathf.Max(0.1f, data.MoveSpeed) : 2.5f;
            goldReward = data != null ? Mathf.Max(0, data.GoldReward) : 10;
            damageToBase = data != null ? data.DamageToBase : 10f;
            countsTowardWave = shouldCountTowardWave;
            isAlive = true;
            hasReachedBase = false;
            ownerPool = pool;
            ownerSpawner = spawner;

            gameObject.name = $"Enemy_{enemyType}";
            mover.Initialize(lanePath, this, moveSpeed);
            ResetVisuals();
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive())
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - Mathf.Max(0f, amount));
            
            TriggerFlash();

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
            ResetVisuals();
        }

        private void TriggerFlash()
        {
            if (!gameObject.activeInHierarchy) return;
            if (_flashRoutine != null) StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRoutine());
        }

        private System.Collections.IEnumerator FlashRoutine()
        {
            if (_renderer != null)
            {
                _renderer.GetPropertyBlock(_propBlock);
                _propBlock.SetColor(ColorPropId, flashColor);
                _renderer.SetPropertyBlock(_propBlock);
            }

            yield return new WaitForSeconds(flashDuration);

            ResetVisuals();
            _flashRoutine = null;
        }

        private void ResetVisuals()
        {
            if (_flashRoutine != null)
            {
                StopCoroutine(_flashRoutine);
                _flashRoutine = null;
            }

            if (_renderer != null)
            {
                _renderer.SetPropertyBlock(null);
            }
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
            ResetVisuals();

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
