// SonTinh_Attack.cs
using HonVietThuThanh.Shared;
using UnityEngine;

namespace HVTThanh.Combat
{
    public class SonTinh_Attack : HeroBase
    {

        public GameObject obstaclePrefab;
        protected override HeroType heroType => HeroType.SonTinh;

        protected override void PerformAttack(ITargetable target)
        {
            // Đặt khối núi trước mặt Sơn Tinh
            Vector3 spawnPos = transform.position + transform.forward * 1.5f;
            spawnPos.y = 0;
            Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
        }
    }
}