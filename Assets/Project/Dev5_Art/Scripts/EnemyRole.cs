using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    public enum EnemyClassRole
    {
        Goblin,
        Orc,
        Archer
    }

    public enum EnemyAttackType
    {
        Melee,
        RangedProjectile
    }

    /// <summary>
    /// Component lưu trữ vai trò và kiểu tấn công của kẻ địch (Phase 15).
    /// </summary>
    public class EnemyRole : MonoBehaviour
    {
        [Header("Cấu hình vai trò")]
        public EnemyClassRole role;
        public EnemyAttackType attackType;

        [Header("Projectile Setup")]
        public GameObject projectilePrefab;
        public Transform projectileSpawnPoint;
    }
}
