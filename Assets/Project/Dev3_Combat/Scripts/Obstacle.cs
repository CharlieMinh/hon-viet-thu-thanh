using HonVietThuThanh.Shared;
using UnityEngine;

namespace HonVietThuThanh.Combat
{
    public class Obstacle : MonoBehaviour, IDamageable
    {
        public float hp = 50f;

        public void TakeDamage(float amount)
        {
            hp -= amount;
            if (hp <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}
