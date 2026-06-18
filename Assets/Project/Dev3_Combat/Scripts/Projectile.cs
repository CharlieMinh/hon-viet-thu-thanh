using HonVietThuThanh.Shared;
using UnityEngine;

namespace HonVietThuThanh.Combat
{
    public class Projectile : MonoBehaviour
    {
        public float speed = 10f;
        public float damage = 10f;
        public bool isPierce = false;

        private Vector3 direction;

        public void Init(Vector3 dir, float dmg, bool pierce = false)
        {
            direction = dir.normalized;
            damage = dmg;
            isPierce = pierce;
        }

        private void Start()
        {
            Destroy(gameObject, 5f);
        }

        private void Update()
        {
            transform.position += direction * speed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            var targetable = other.GetComponentInParent<ITargetable>();
            var damageable = other.GetComponentInParent<IDamageable>();
            if (targetable == null || damageable == null || !targetable.IsAlive())
            {
                return;
            }

            damageable.TakeDamage(damage);
            if (!isPierce)
            {
                Destroy(gameObject);
            }
        }
    }
}
