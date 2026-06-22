using HonVietThuThanh.Shared;
using UnityEngine;

namespace HonVietThuThanh.Combat
{
    [RequireComponent(typeof(SphereCollider))]
    public abstract class HeroBase : MonoBehaviour, IDamageable
    {
        [Header("Stats")]
        public float attackRange = 5f;
        public float attackDamage = 10f;
        public float attackSpeed = 1f;
        public float maxHp = 100f;

        protected float currentHp;
        protected float attackCooldown;
        protected ITargetable currentTarget;
        protected virtual bool CanAttackWithoutTarget => false;

        protected virtual void Awake()
        {
            currentHp = maxHp;
            var col = GetComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = attackRange;
        }

        protected virtual void Update()
        {
            attackCooldown -= Time.deltaTime;

            currentTarget = FindClosestEnemy();
            if (currentTarget == null && !CanAttackWithoutTarget)
            {
                return;
            }

            if (currentTarget != null)
            {
                Vector3 dir = currentTarget.GetPosition() - transform.position;
                dir.y = 0f;
                if (dir != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(dir);
                }
            }

            if (attackCooldown <= 0f)
            {
                attackCooldown = 1f / Mathf.Max(0.01f, attackSpeed);
                if (!PerformAttack(currentTarget))
                {
                    return;
                }

                GameObject targetObject = GetActionTarget(currentTarget);
                GameEvents.RaiseHeroAttacked(heroType, targetObject);
                Debug.Log($"[Dev3 Combat] {heroType} attacked {(targetObject != null ? targetObject.name : "target")}.", this);
            }
        }

        protected abstract HeroType heroType { get; }
        protected abstract bool PerformAttack(ITargetable target);
        protected virtual GameObject GetActionTarget(ITargetable target) => GetTargetGameObject(target);

        public bool NeedsHealing => currentHp < maxHp;

        public void TakeDamage(float amount)
        {
            currentHp -= amount;
            if (currentHp <= 0f)
            {
                Destroy(gameObject);
            }
        }

        public void Heal(float amount)
        {
            currentHp = Mathf.Min(currentHp + amount, maxHp);
            Debug.Log($"[Dev3 Combat] {name} healed {amount}, {currentHp} HP current.", this);
        }

        private ITargetable FindClosestEnemy()
        {
            var colliders = Physics.OverlapSphere(transform.position, attackRange);
            ITargetable best = null;
            float bestDist = float.MaxValue;

            foreach (var col in colliders)
            {
                var target = col.GetComponentInParent<ITargetable>();
                if (target == null || !target.IsAlive())
                {
                    continue;
                }

                float dist = Vector3.Distance(transform.position, target.GetPosition());
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = target;
                }
            }

            return best;
        }

        private static GameObject GetTargetGameObject(ITargetable target)
        {
            return target is Component component ? component.gameObject : null;
        }
    }
}
