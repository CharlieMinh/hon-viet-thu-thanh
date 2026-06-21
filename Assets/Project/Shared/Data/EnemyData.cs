using HonVietThuThanh.Shared;
using UnityEngine;

namespace HonVietThuThanh.Dev2_EnemyWave
{
    /// <summary>
    /// Centralized data for enemy types, allowing designers to tune stats in assets.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData_", menuName = "Hon Viet/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private EnemyType enemyType = EnemyType.LinhXamLuoc;
        
        [Header("Stats")]
        [SerializeField] private float maxHealth = 20f;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private int goldReward = 15;
        [SerializeField] private float damageToBase = 10f;

        public EnemyType EnemyType => enemyType;
        public float MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public int GoldReward => goldReward;
        public float DamageToBase => damageToBase;
    }
}
