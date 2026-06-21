using System.Collections.Generic;
using HonVietThuThanh.Shared;
using UnityEngine;

namespace HonVietThuThanh.Combat
{
    public class ChuDongTu_Attack : HeroBase
    {
        protected override HeroType heroType => HeroType.ChuDongTu;

        protected override bool PerformAttack(ITargetable target)
        {
            var cols = Physics.OverlapSphere(transform.position, attackRange);
            var healedHeroes = new HashSet<HeroBase>();

            foreach (var col in cols)
            {
                if (col.gameObject == gameObject)
                {
                    continue;
                }

                var hero = col.GetComponentInParent<HeroBase>();
                if (hero != null && healedHeroes.Add(hero))
                {
                    hero.Heal(attackDamage);
                }
            }

            Debug.Log("[Dev3 Combat] ChuDongTu heal pulse completed.", this);
            return true;
        }
    }
}
