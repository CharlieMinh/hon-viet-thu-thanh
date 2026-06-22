using System.Collections.Generic;
using HonVietThuThanh.Shared;
using UnityEngine;

namespace HonVietThuThanh.Combat
{
    public class ChuDongTu_Attack : HeroBase
    {
        private GameObject lastHealedTarget;

        protected override HeroType heroType => HeroType.ChuDongTu;
        protected override bool CanAttackWithoutTarget => true;

        protected override bool PerformAttack(ITargetable target)
        {
            var cols = Physics.OverlapSphere(transform.position, attackRange);
            var healedHeroes = new HashSet<HeroBase>();
            lastHealedTarget = null;

            foreach (var col in cols)
            {
                if (col.gameObject == gameObject)
                {
                    continue;
                }

                var hero = col.GetComponentInParent<HeroBase>();
                if (hero != null && hero != this && hero.NeedsHealing && healedHeroes.Add(hero))
                {
                    hero.Heal(attackDamage);
                    if (lastHealedTarget == null)
                    {
                        lastHealedTarget = hero.gameObject;
                    }
                }
            }

            bool healedAny = healedHeroes.Count > 0;
            if (healedAny)
            {
                Debug.Log($"[Dev3 Combat] ChuDongTu healed {healedHeroes.Count} ally hero(es).", this);
            }

            return healedAny;
        }

        protected override GameObject GetActionTarget(ITargetable target) => lastHealedTarget;
    }
}
