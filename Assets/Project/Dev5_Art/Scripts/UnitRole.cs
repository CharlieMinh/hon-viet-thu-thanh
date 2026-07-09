using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    public enum UnitClassRole
    {
        Knight,
        Archer,
        Tank
    }

    public enum AttackType
    {
        Melee,
        RangedProjectile
    }

    /// <summary>
    /// Component lưu trữ vai trò và kiểu tấn công của quân cờ (Phase 14).
    /// </summary>
    public class UnitRole : MonoBehaviour
    {
        [Header("Cấu hình vai trò")]
        public UnitClassRole role;
        public AttackType attackType;
        
        [Header("Chỉ số đặc biệt")]
        [Tooltip("Nếu là Tank, kẻ địch xung quanh sẽ ưu tiên tấn công")]
        public bool isTank = false;
        
        [Tooltip("Phạm vi thu hút kẻ địch (Taunt) của Tank")]
        public float tauntRadius = 3f;
    }
}
