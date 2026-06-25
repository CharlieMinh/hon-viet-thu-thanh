using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Component lưu trữ các chỉ số chiến đấu và di chuyển của Enemy (Phase 9).
    /// </summary>
    public class EnemyCombatStats : MonoBehaviour
    {
        [Header("Chỉ số chiến đấu")]
        [Tooltip("Sát thương gây ra trên mỗi đòn đánh")]
        public int damage = 5;

        [Tooltip("Tầm đánh cận chiến/tầm xa")]
        public float attackRange = 1.3f;

        [Tooltip("Giãn cách thời gian giữa các đòn đánh (giây)")]
        public float attackCooldown = 1.5f;

        [Header("Chỉ số di chuyển & định hướng")]
        [Tooltip("Tốc độ chạy tiếp cận tướng")]
        public float moveSpeed = 2.5f;

        [Tooltip("Tốc độ xoay mặt hướng tướng")]
        public float rotationSpeed = 10f;

        /// <summary>
        /// Khởi tạo các chỉ số chiến đấu (Phase 15).
        /// </summary>
        public void InitializeStats(int damage, float attackRange, float attackCooldown, float moveSpeed, float rotationSpeed)
        {
            this.damage = damage;
            this.attackRange = attackRange;
            this.attackCooldown = attackCooldown;
            this.moveSpeed = moveSpeed;
            this.rotationSpeed = rotationSpeed;
        }
    }
}
