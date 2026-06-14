// Obstacle.cs
using HonVietThuThanh.Shared;
using UnityEngine;

namespace HVTThanh.Combat
{
    public class Obstacle : MonoBehaviour, IDamageable
    {
        public float hp = 50f;
        public void TakeDamage(float amount)
        {
            hp -= amount;
            if (hp <= 0) Destroy(gameObject);
        }
    }
}