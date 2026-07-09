using HonVietThuThanh.Shared;
using UnityEngine;

namespace HonVietThuThanh.Combat
{
    public class EnemyStub : MonoBehaviour, IDamageable, ITargetable
    {
        public float hp = 50f;
        public float speed = 0f;

        public float CurrentHp => hp;

        public Vector3 GetPosition() => transform.position;
        public bool IsAlive() => hp > 0f;

        public void TakeDamage(float amount)
        {
            hp = Mathf.Max(0f, hp - Mathf.Max(0f, amount));
            Debug.Log($"[Dev3 Combat] EnemyStub took {amount} damage, {hp} HP remaining.", this);

            if (hp <= 0f)
            {
                Debug.Log("[Dev3 Combat] EnemyStub died.", this);
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            transform.position += Vector3.back * speed * Time.deltaTime;

            if (transform.position.z < -10f)
            {
                Debug.Log("[Dev3 Combat] EnemyStub reached base preview endpoint.", this);
                Destroy(gameObject);
            }
        }
    }
}
