using HonVietThuThanh.Shared;
using UnityEngine;

namespace HonVietThuThanh.Combat
{
    public class ThanhGiong_Attack : HeroBase
    {
        public GameObject projectilePrefab;

        protected override HeroType heroType => HeroType.ThanhGiong;

        protected override bool PerformAttack(ITargetable target)
        {
            if (projectilePrefab == null)
            {
                Debug.LogWarning("[Dev3 Combat] ThanhGiong has no projectile prefab assigned.", this);
                return false;
            }

            Vector3 direction = target.GetPosition() - transform.position;
            direction.y = 0f;
            if (direction == Vector3.zero)
            {
                direction = transform.forward;
            }

            var go = Instantiate(projectilePrefab, transform.position + direction.normalized * 0.75f, Quaternion.identity);
            go.SetActive(true);

            var proj = go.GetComponent<Projectile>();
            if (proj == null)
            {
                Debug.LogError("[Dev3 Combat] Assigned projectile prefab is missing the Projectile component.", go);
                Destroy(go);
                return false;
            }

            // Phase 1 uses a regular projectile. Pierce remains available for a later skill upgrade.
            proj.Init(direction, attackDamage, false);
            return true;
        }
    }
}
