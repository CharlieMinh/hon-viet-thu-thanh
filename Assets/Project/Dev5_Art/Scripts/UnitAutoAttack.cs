using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Component xử lý hành vi tự động tìm quái gần nhất, di chuyển lại gần và tấn công theo cooldown.
    /// Kích hoạt trong Combat Phase đối với các quân cờ đã được đặt trên bàn cờ.
    /// </summary>
    [RequireComponent(typeof(PlaceableUnit))]
    [RequireComponent(typeof(UnitCombatStats))]
    public class UnitAutoAttack : MonoBehaviour
    {
        [Header("Projectile Setup (Phase 14)")]
        public GameObject projectilePrefab;

        private PlaceableUnit placeableUnit;
        private UnitCombatStats combatStats;
        private Health myHealth;
        private UnitRole unitRole;
        private CharacterAnimationController animationController;

        private float cooldownTimer = 0f;
        private bool hasLoggedNoEnemies = false;
        private EnemyController target = null;

        private void Awake()
        {
            placeableUnit = GetComponent<PlaceableUnit>();
            combatStats = GetComponent<UnitCombatStats>();
            myHealth = GetComponent<Health>();
            unitRole = GetComponent<UnitRole>();
            animationController = GetComponent<CharacterAnimationController>();
        }

        private void Update()
        {
            // 0. Đảm bảo bản thân chưa chết
            if (myHealth != null && myHealth.IsDead)
            {
                target = null;
                if (animationController != null) animationController.SetMoving(false);
                return;
            }

            // 1. Chỉ hoạt động nếu quân cờ đã được đặt trên bàn cờ (không ở hàng chờ)
            if (!placeableUnit.IsPlacedOnBoard)
            {
                target = null;
                if (animationController != null) animationController.SetMoving(false);
                return;
            }

            // 2. Chỉ hoạt động trong giai đoạn Combat Phase
            if (GamePhaseManager.Instance != null && !GamePhaseManager.Instance.IsCombatPhase)
            {
                target = null;
                if (animationController != null) animationController.SetMoving(false);
                return;
            }

            // Tăng thời gian cooldown (cho phép tích lũy cooldown trong khi di chuyển)
            cooldownTimer += Time.deltaTime;

            // 3. Tìm target mới nếu chưa có hoặc target cũ đã chết/bị hủy
            if (target == null || target.Health == null || target.Health.IsDead)
            {
                target = GetNearestEnemyTarget();
            }

            // 4. Nếu không tìm thấy bất kỳ kẻ địch nào
            if (target == null)
            {
                if (animationController != null) animationController.SetMoving(false);
                if (!hasLoggedNoEnemies)
                {
                    Debug.Log($"[{gameObject.name}] Không phát hiện kẻ địch nào trên bản đồ. Dừng di chuyển và chờ đợi...");
                    hasLoggedNoEnemies = true;
                }
                return;
            }

            // Đã tìm thấy kẻ địch -> Reset trạng thái log
            hasLoggedNoEnemies = false;

            // 5. Xử lý xoay hướng về phía kẻ địch (Giữ nguyên độ cao Y)
            Vector3 targetPos = target.transform.position;
            targetPos.y = transform.position.y; // Không đổi cao độ Y

            Vector3 direction = (targetPos - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, combatStats.rotationSpeed * Time.deltaTime);
            }

            // 6. Kiểm tra khoảng cách
            float distance = Vector3.Distance(transform.position, targetPos);

            if (distance > combatStats.attackRange)
            {
                // Kẻ địch ngoài tầm đánh -> Di chuyển lại gần
                transform.position = Vector3.MoveTowards(transform.position, targetPos, combatStats.moveSpeed * Time.deltaTime);
                if (animationController != null) animationController.SetMoving(true);
            }
            else
            {
                // Kẻ địch đã vào tầm đánh -> Dừng di chuyển và tấn công
                if (animationController != null) animationController.SetMoving(false);

                if (cooldownTimer >= combatStats.attackCooldown)
                {
                    if (animationController != null) animationController.PlayAttack();
                    ExecuteAttack(target);
                }
            }
        }

        /// <summary>
        /// Thực hiện đòn đánh và gây sát thương.
        /// </summary>
        private void ExecuteAttack(EnemyController enemy)
        {
            if (unitRole != null && unitRole.attackType == AttackType.RangedProjectile)
            {
                if (projectilePrefab != null)
                {
                    // Spawn projectile lửng lơ trên tầm ngực (cao độ Y bù thêm 1.0f)
                    GameObject projGO = Instantiate(projectilePrefab, transform.position + Vector3.up * 1.0f, Quaternion.identity);
                    SimpleProjectile proj = projGO.GetComponent<SimpleProjectile>();
                    if (proj != null)
                    {
                        proj.Initialize(enemy.transform, enemy.Health, combatStats.damage, 12f, gameObject.name);
                        Debug.Log($"[{gameObject.name}] Bắn projectile vào '{enemy.gameObject.name}' với sát thương {combatStats.damage}.");
                    }
                    else
                    {
                        // Fallback
                        enemy.Health.TakeDamage(combatStats.damage);
                        Debug.LogWarning($"[{gameObject.name}] Prefab projectile thiếu component SimpleProjectile. Gây damage trực tiếp!");
                    }
                }
                else
                {
                    // Fallback
                    enemy.Health.TakeDamage(combatStats.damage);
                    Debug.LogWarning($"[{gameObject.name}] Chưa gán Projectile Prefab cho Archer! Gây damage trực tiếp!");
                }
            }
            else
            {
                // Tấn công cận chiến (Melee) trực tiếp gây sát thương
                enemy.Health.TakeDamage(combatStats.damage);
                Debug.Log($"[{gameObject.name}] Đang tấn công cận chiến '{enemy.gameObject.name}' gây {combatStats.damage} sát thương. (HP quái: {enemy.Health.CurrentHealth}/{enemy.Health.MaxHealth})");
            }

            // Reset cooldown
            cooldownTimer = 0f;
        }

        /// <summary>
        /// Helper tìm kẻ địch gần nhất trong EnemyManager.
        /// </summary>
        private EnemyController GetNearestEnemyTarget()
        {
            if (EnemyManager.Instance == null) return null;
            return EnemyManager.Instance.GetNearestEnemy(transform.position);
        }

        /// <summary>
        /// Khôi phục trạng thái chuẩn bị trước trận đấu (Phase 10).
        /// </summary>
        public void ResetCombatState()
        {
            target = null;
            cooldownTimer = 0f;
            hasLoggedNoEnemies = false;
            if (animationController != null) animationController.SetMoving(false);
        }
    }
}
