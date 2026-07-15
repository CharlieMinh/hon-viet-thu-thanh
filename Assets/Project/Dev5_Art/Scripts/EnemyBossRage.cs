using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(EnemyCombatStats))]
    public class EnemyBossRage : MonoBehaviour
    {
        [SerializeField] private float healthThresholdPercent = 0.5f;
        [SerializeField] private float damageMultiplier = 1.2f;
        [SerializeField] private float attackCooldownMultiplier = 0.8f;
        [SerializeField] private float moveSpeedMultiplier = 1.15f;

        private Health health;
        private EnemyCombatStats stats;
        private bool rageActivated;

        private void Awake()
        {
            health = GetComponent<Health>();
            stats = GetComponent<EnemyCombatStats>();
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.OnHealthChanged += HandleHealthChanged;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnHealthChanged -= HandleHealthChanged;
            }
        }

        public void Configure(float thresholdPercent, float damageMul, float cooldownMul, float speedMul)
        {
            healthThresholdPercent = Mathf.Clamp01(thresholdPercent);
            damageMultiplier = damageMul;
            attackCooldownMultiplier = cooldownMul;
            moveSpeedMultiplier = speedMul;
            rageActivated = false;
        }

        private void HandleHealthChanged(int currentHealth, int maxHealth)
        {
            if (rageActivated || stats == null || maxHealth <= 0)
            {
                return;
            }

            if (currentHealth > maxHealth * healthThresholdPercent)
            {
                return;
            }

            rageActivated = true;
            stats.damage = Mathf.RoundToInt(stats.damage * damageMultiplier);
            stats.attackCooldown *= attackCooldownMultiplier;
            stats.moveSpeed *= moveSpeedMultiplier;

            Debug.Log($"[EnemyBossRage] {gameObject.name} entered Rage: damage={stats.damage}, cooldown={stats.attackCooldown:F2}, speed={stats.moveSpeed:F2}");
        }
    }
}
