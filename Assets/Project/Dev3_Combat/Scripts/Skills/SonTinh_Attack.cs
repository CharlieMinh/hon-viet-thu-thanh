using HonVietThuThanh.Shared;
using UnityEngine;

namespace HonVietThuThanh.Combat
{
    public class SonTinh_Attack : HeroBase
    {
        public GameObject obstaclePrefab;

        protected override HeroType heroType => HeroType.SonTinh;

        protected override bool PerformAttack(ITargetable target)
        {
            Vector3 spawnPos = transform.position + transform.forward * 1.5f;
            spawnPos.y = 0.5f;

            GameObject obstacle = obstaclePrefab != null
                ? Instantiate(obstaclePrefab, spawnPos, Quaternion.identity)
                : GameObject.CreatePrimitive(PrimitiveType.Cube);

            obstacle.name = "SonTinh_Obstacle";
            obstacle.transform.position = spawnPos;
            obstacle.SetActive(true);

            if (obstacle.GetComponent<Obstacle>() == null)
            {
                obstacle.AddComponent<Obstacle>();
            }

            Debug.Log("[Dev3 Combat] SonTinh spawned an obstacle.", obstacle);
            return true;
        }
    }
}
