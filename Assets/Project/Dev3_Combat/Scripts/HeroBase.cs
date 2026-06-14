// Assets/Project/Dev3_Combat/Scripts/HeroBase.cs
using HonVietThuThanh.Shared;
using UnityEngine;

namespace HVTThanh.Combat
{

    [RequireComponent(typeof(SphereCollider))]
    public abstract class HeroBase : MonoBehaviour, IDamageable
    {

        [Header("Stats — gán qua HeroData sau")]
        public float attackRange = 5f;
        public float attackDamage = 10f;
        public float attackSpeed = 1f;   // lần/giây
        public float maxHp = 100f;

        protected float currentHp;
        protected float attackCooldown;
        protected ITargetable currentTarget;

        protected virtual void Awake()
        {
            currentHp = maxHp;
            // Dùng SphereCollider làm trigger để detect range
            var col = GetComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = attackRange;
        }

        protected virtual void Update()
        {
            attackCooldown -= Time.deltaTime;

            currentTarget = FindClosestEnemy();
            if (currentTarget == null) return;

            // Quay về phía enemy
            Vector3 dir = currentTarget.GetPosition() - transform.position;
            dir.y = 0;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(dir);

            // Tấn công khi hết cooldown
            if (attackCooldown <= 0f)
            {
                attackCooldown = 1f / attackSpeed;
                PerformAttack(currentTarget);
                GameEvents.InvokeOnHeroAttacked(heroType, gameObject); // phát event
            }
        }

        // Subclass override cái này
        protected abstract HeroType heroType { get; }
        protected abstract void PerformAttack(ITargetable target);

        // --- IDamageable ---
        public void TakeDamage(float amount)
        {
            currentHp -= amount;
            if (currentHp <= 0) Destroy(gameObject);
        }

        // --- Tìm enemy gần BASE nhất trong range ---
        private ITargetable FindClosestEnemy()
        {
            // Dev 2 enemy có tag "Enemy"
            var enemies = Physics.OverlapSphere(transform.position, attackRange);
            ITargetable best = null;
            float bestDist = float.MaxValue;

            // "Gần base nhất" = z nhỏ nhất (hoặc tùy hướng lane, điều chỉnh sau)
            foreach (var col in enemies)
            {
                if (!col.CompareTag("Enemy")) continue;
                var t = col.GetComponent<ITargetable>();
                if (t == null || !t.IsAlive()) continue;
                float dist = col.transform.position.z; // điểm z gần base = nhỏ hơn
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = t;
                }
            }
            return best;
        }
        public void Heal(float amount)
        {
            currentHp = Mathf.Min(currentHp + amount, maxHp);
        }
    }
}