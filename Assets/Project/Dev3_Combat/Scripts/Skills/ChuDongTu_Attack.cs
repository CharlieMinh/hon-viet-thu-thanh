// ChuDongTu_Attack.cs
using HonVietThuThanh.Shared;
using UnityEngine;

namespace HVTThanh.Combat
{
    public class ChuDongTu_Attack : HeroBase
    {

        protected override HeroType heroType => HeroType.ChuDongTu;

        protected override void PerformAttack(ITargetable target)
        {
            // Heal tất cả hero trong range (tag "Hero")
            var cols = Physics.OverlapSphere(transform.position, attackRange);
            foreach (var col in cols)
            {
                if (!col.CompareTag("Hero")) continue;
                if (col.gameObject == this.gameObject) continue;

                var hero = col.GetComponent<HeroBase>();
                if (hero != null)
                    hero.Heal(attackDamage);
            }
        }
    }
}