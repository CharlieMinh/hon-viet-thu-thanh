using System;
using HonVietThuThanh.Shared;
using UnityEngine;

namespace HonVietThuThanh.Dev2_EnemyWave
{
    /// <summary>
    /// Tunable runtime values for a spawned enemy instance.
    /// </summary>
    [Serializable]
    public class EnemySpawnProfile
    {
        [SerializeField] private EnemyType enemyType = EnemyType.LinhXamLuoc;
        [SerializeField] private float maxHealth = 15f;
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private int goldReward = 10;

        public EnemyType EnemyType => enemyType;
        public float MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public int GoldReward => goldReward;
    }
}
