using HonVietThuThanh.Shared;
using UnityEngine;

namespace HonVietThuThanh.Combat
{
    public class Obstacle : MonoBehaviour, IDamageable
    {
        public float hp = 50f;
        [Min(0f)] public float lifetimeSeconds = 5f;

        private void OnEnable()
        {
            if (lifetimeSeconds > 0f)
            {
                Invoke(nameof(Expire), lifetimeSeconds);
            }
        }

        private void OnDisable()
        {
            CancelInvoke();
        }

        private void Expire()
        {
            Destroy(gameObject);
        }

        public void TakeDamage(float amount)
        {
            hp -= Mathf.Max(0f, amount);
            if (hp <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}
