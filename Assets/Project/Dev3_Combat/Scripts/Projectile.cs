using System;
using System.Collections.Generic;
using HonVietThuThanh.Shared;
using UnityEngine;

namespace HonVietThuThanh.Combat
{
    public class Projectile : MonoBehaviour
    {
        public enum CleanupReason
        {
            Hit,
            Timeout
        }

        public static event Action<CleanupReason> OnProjectileCleanedUp;

        public float speed = 10f;
        public float damage = 10f;
        public bool isPierce = false;
        [Min(0.1f)] public float lifetimeSeconds = 5f;

        private Vector3 direction;
        private readonly HashSet<GameObject> hitTargets = new HashSet<GameObject>();
        private bool cleanupStarted;

        public void Init(Vector3 dir, float dmg, bool pierce = false)
        {
            direction = dir.normalized;
            damage = dmg;
            isPierce = pierce;
            hitTargets.Clear();
            cleanupStarted = false;
        }

        private void OnEnable()
        {
            Invoke(nameof(Expire), Mathf.Max(0.1f, lifetimeSeconds));
        }

        private void OnDisable()
        {
            CancelInvoke();
        }

        private void Update()
        {
            transform.position += direction * speed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (cleanupStarted)
            {
                return;
            }

            var targetable = other.GetComponentInParent<ITargetable>();
            var damageable = other.GetComponentInParent<IDamageable>();
            if (targetable == null || damageable == null || !targetable.IsAlive())
            {
                return;
            }

            GameObject targetObject = targetable is Component component
                ? component.gameObject
                : other.transform.root.gameObject;
            if (!hitTargets.Add(targetObject))
            {
                return;
            }

            damageable.TakeDamage(damage);
            if (!isPierce)
            {
                Cleanup(CleanupReason.Hit);
            }
        }

        private void Expire()
        {
            Cleanup(CleanupReason.Timeout);
        }

        private void Cleanup(CleanupReason reason)
        {
            if (cleanupStarted)
            {
                return;
            }

            cleanupStarted = true;
            OnProjectileCleanedUp?.Invoke(reason);
            Destroy(gameObject);
        }
    }
}
