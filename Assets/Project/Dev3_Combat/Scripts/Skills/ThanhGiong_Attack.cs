// ThanhGiong_Attack.cs
using HonVietThuThanh.Shared;
using UnityEngine;

namespace HVTThanh.Combat
{
    public class ThanhGiong_Attack : HeroBase
    {

        public GameObject projectilePrefab;
        protected override HeroType heroType => HeroType.ThanhGiong;

        protected override void PerformAttack(ITargetable target)
        {
            // Bắn thẳng theo hướng lane (trục Z âm)
            var go = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            var proj = go.GetComponent<Projectile>();
            proj.Init(Vector3.back, attackDamage, true);
        }
    }
}