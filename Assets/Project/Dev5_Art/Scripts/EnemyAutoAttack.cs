using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Component xử lý hành vi của Enemy: tự động tìm quân cờ của người chơi gần nhất, di chuyển lại gần và tấn công.
    /// Hoạt động trong Combat phase (Phase 9).
    /// </summary>
    [RequireComponent(typeof(EnemyController))]
    [RequireComponent(typeof(EnemyCombatStats))]
    public class EnemyAutoAttack : MonoBehaviour
    {
        private EnemyController enemyController;
        private EnemyCombatStats combatStats;
        private Health myHealth;
        private EnemyRole enemyRole;

        private float cooldownTimer = 0f;
        private bool hasLoggedNoTargets = false;
        private PlaceableUnit target = null;

        private void Awake()
        {
            enemyController = GetComponent<EnemyController>();
            combatStats = GetComponent<EnemyCombatStats>();
            myHealth = GetComponent<Health>();
            enemyRole = GetComponent<EnemyRole>();
        }

        private void Update()
        {
            // 0. Đảm bảo bản thân chưa chết
            if (myHealth != null && myHealth.IsDead)
            {
                target = null;
                return;
            }

            // 1. Chỉ hoạt động trong giai đoạn Combat Phase
            if (GamePhaseManager.Instance != null && !GamePhaseManager.Instance.IsCombatPhase)
            {
                target = null;
                return;
            }

            // Tăng thời gian cooldown (cho phép tích lũy cooldown trong khi di chuyển)
            cooldownTimer += Time.deltaTime;

            // 2. Tìm target mới nếu chưa có hoặc target cũ đã chết/bị huỷ
            if (target == null)
            {
                target = GetNearestPlayerTarget();
            }
            else
            {
                Health targetHealth = target.GetComponent<Health>();
                if (targetHealth == null || targetHealth.IsDead)
                {
                    target = null;
                    target = GetNearestPlayerTarget();
                }
            }

            // 3. Nếu không tìm thấy mục tiêu nào
            if (target == null)
            {
                if (!hasLoggedNoTargets)
                {
                    Debug.Log($"[{gameObject.name}] Không phát hiện quân cờ chiến đấu nào của người chơi trên bàn. Chờ đợi...");
                    hasLoggedNoTargets = true;
                }
                return;
            }

            // Đã tìm thấy mục tiêu -> Reset trạng thái log
            hasLoggedNoTargets = false;

            // 4. Xử lý xoay hướng về phía cờ người chơi (Giữ nguyên độ cao Y)
            Vector3 targetPos = target.transform.position;
            targetPos.y = transform.position.y; // Không đổi cao độ Y

            Vector3 direction = (targetPos - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, combatStats.rotationSpeed * Time.deltaTime);
            }

            // 5. Kiểm tra tầm đánh và di chuyển
            float distance = Vector3.Distance(transform.position, targetPos);

            if (distance > combatStats.attackRange)
            {
                // Mục tiêu ngoài tầm đánh -> Di chuyển lại gần bằng Vector3.MoveTowards
                transform.position = Vector3.MoveTowards(transform.position, targetPos, combatStats.moveSpeed * Time.deltaTime);
            }
            else
            {
                // Mục tiêu đã vào tầm đánh -> Đứng yên và tấn công theo cooldown
                if (cooldownTimer >= combatStats.attackCooldown)
                {
                    ExecuteAttack(target);
                }
            }
        }

        /// <summary>
        /// Gây sát thương lên quân cờ của người chơi.
        /// </summary>
        private void ExecuteAttack(PlaceableUnit targetUnit)
        {
            Health targetHealth = targetUnit.GetComponent<Health>();
            if (targetHealth != null && !targetHealth.IsDead)
            {
                if (enemyRole != null && enemyRole.attackType == EnemyAttackType.RangedProjectile)
                {
                    if (enemyRole.projectilePrefab != null)
                    {
                        // Spawn projectile tại spawn point hoặc hơi lệch lên (cao độ Y thêm 1.0f)
                        Vector3 spawnPos = enemyRole.projectileSpawnPoint != null ? enemyRole.projectileSpawnPoint.position : transform.position + Vector3.up * 1.0f;
                        GameObject projGO = Instantiate(enemyRole.projectilePrefab, spawnPos, Quaternion.identity);
                        SimpleProjectile proj = projGO.GetComponent<SimpleProjectile>();
                        if (proj != null)
                        {
                            proj.Initialize(targetUnit.transform, targetHealth, combatStats.damage, 12f, gameObject.name);
                            Debug.Log($"[{gameObject.name}] Bắn projectile vào '{targetUnit.gameObject.name}' với sát thương {combatStats.damage}.");
                        }
                        else
                        {
                            targetHealth.TakeDamage(combatStats.damage);
                            Debug.LogWarning($"[{gameObject.name}] Prefab projectile thiếu component SimpleProjectile. Gây damage trực tiếp!");
                        }
                    }
                    else
                    {
                        targetHealth.TakeDamage(combatStats.damage);
                        Debug.LogWarning($"[{gameObject.name}] Chưa gán Projectile Prefab cho Enemy Archer! Gây damage trực tiếp!");
                    }
                }
                else
                {
                    targetHealth.TakeDamage(combatStats.damage);
                    Debug.Log($"[{gameObject.name}] Tấn công '{targetUnit.gameObject.name}' gây {combatStats.damage} sát thương. (HP cờ: {targetHealth.CurrentHealth}/{targetHealth.MaxHealth})");
                }
            }

            // Reset cooldown
            cooldownTimer = 0f;
        }

        private PlaceableUnit GetNearestPlayerTarget()
        {
            if (PlayerUnitManager.Instance == null) return null;
            return PlayerUnitManager.Instance.GetPriorityTargetForEnemy(transform.position);
        }
    }
}
