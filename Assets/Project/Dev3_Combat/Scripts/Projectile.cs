// Assets/Project/Dev3_Combat/Scripts/Projectile.cs
using HonVietThuThanh.Shared;
using UnityEngine;

namespace HVTThanh.Combat
{

    public class Projectile : MonoBehaviour
    {

        public float speed = 10f;
        public float damage = 10f;
        public bool isPierce = false;  // Thánh Gióng sẽ set true
        private Vector3 direction;

        public void Init(Vector3 dir, float dmg, bool pierce = false)
        {
            direction = dir.normalized;
            damage = dmg;
            this.isPierce = pierce;
        }

        void Update()
        {
            transform.position += direction * speed * Time.deltaTime;
            Destroy(gameObject, 5f); // tự huỷ sau 5s phòng leak
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Enemy")) return;
            var dmgable = other.GetComponent<IDamageable>();
            dmgable?.TakeDamage(damage);
            if (!isPierce) Destroy(gameObject);
        }
    }
}