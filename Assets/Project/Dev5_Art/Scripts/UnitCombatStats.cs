using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Lưu trữ các chỉ số chiến đấu cơ bản của một quân cờ (Unit).
    /// </summary>
    public class UnitCombatStats : MonoBehaviour
    {
        [Header("Chỉ số chiến đấu")]
        [Tooltip("Sát thương gây ra mỗi hit đánh")]
        public int damage = 10;

        [Header("Base Stats (Lưu chỉ số gốc để scale sao - Phase 12)")]
        [SerializeField] private int baseDamage = -1;

        /// <summary>
        /// Nhân chỉ số damage gốc với hệ số sao và đồng bộ qua Health.
        /// </summary>
        public void ApplyStarMultiplier(int starLevel)
        {
            if (baseDamage <= 0)
            {
                baseDamage = damage;
            }

            float multiplier = Mathf.Pow(1.5f, starLevel - 1);
            damage = Mathf.RoundToInt(baseDamage * multiplier);

            Health hp = GetComponent<Health>();
            if (hp != null)
            {
                hp.ApplyStarMultiplier(starLevel);
            }
        }

        [Tooltip("Tầm đánh của quân cờ")]
        public float attackRange = 5f;

        [Tooltip("Thời gian giãn cách giữa các đòn đánh (giây)")]
        public float attackCooldown = 1.0f;

        [Tooltip("Số Vàng nhận được mỗi khi đánh trúng (Đã ngưng sử dụng ở Phase 11)")]
        [System.Obsolete("Gold is now awarded on enemy death, not per hit.")]
        public int goldPerHit = 1;

        [Header("Tốc độ & Di chuyển")]
        [Tooltip("Tốc độ di chuyển của quân cờ")]
        public float moveSpeed = 3f;

        [Tooltip("Tốc độ xoay mặt của quân cờ")]
        public float rotationSpeed = 10f;
    }
}
