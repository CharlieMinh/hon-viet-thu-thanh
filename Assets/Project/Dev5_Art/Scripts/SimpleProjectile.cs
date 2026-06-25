using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Component xử lý projectile bay từ Archer tới enemy target (Phase 14).
    /// </summary>
    public class SimpleProjectile : MonoBehaviour
    {
        private Transform target;
        private Health targetHealth;
        private int damage;
        private float speed = 10f;
        private string ownerName;
        private bool hasHit = false;

        /// <summary>
        /// Khởi tạo projectile.
        /// </summary>
        public void Initialize(Transform targetTransform, Health health, int dmg, float projSpeed, string owner)
        {
            target = targetTransform;
            targetHealth = health;
            damage = dmg;
            speed = projSpeed;
            ownerName = owner;
            hasHit = false;
        }

        private void Update()
        {
            // Nếu target null hoặc target đã chết -> huỷ projectile
            if (target == null || targetHealth == null || targetHealth.IsDead)
            {
                Destroy(gameObject);
                return;
            }

            // Di chuyển tới target
            Vector3 targetPos = target.position;
            // Bắn vào tầm giữa người target thay vì dưới chân (bù Y lên 1.0f cho capsule cao 2.0)
            targetPos.y += 1.0f;

            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

            // Kiểm tra khoảng cách để kích hoạt va chạm
            float distance = Vector3.Distance(transform.position, targetPos);
            if (distance < 0.2f)
            {
                HitTarget();
            }
        }

        private void HitTarget()
        {
            if (hasHit) return;
            hasHit = true;

            if (targetHealth != null && !targetHealth.IsDead)
            {
                targetHealth.TakeDamage(damage);
                Debug.Log($"[SimpleProjectile] Projectile từ '{ownerName}' đánh trúng '{targetHealth.gameObject.name}', gây {damage} sát thương.");
            }

            Destroy(gameObject);
        }
    }
}
