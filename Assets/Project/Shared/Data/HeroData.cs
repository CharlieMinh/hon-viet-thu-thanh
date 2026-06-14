using UnityEngine;

namespace HonVietThuThanh.Shared
{
    /// <summary>
    /// ScriptableObject chứa toàn bộ stats của một hero.
    /// Tạo asset: Right-click → Create → HonViet → HeroData
    /// Dev1 dùng cost để validate gold trước khi đặt.
    /// Dev3 dùng damage/attackRange/attackSpeed cho combat.
    /// Dev4 (EconomyManager) dùng cost để trừ gold.
    /// Dev4 (HeroSelectionUI) dùng heroName/heroIcon để hiển thị panel.
    /// </summary>
    [CreateAssetMenu(fileName = "HeroData_New", menuName = "HonViet/HeroData")]
    public class HeroData : ScriptableObject
    {
        [Header("Identity")]
        public HeroType heroType;
        public string heroName;

        [Header("Economy")]
        [Tooltip("Chi phí Linh Khí để đặt hero này")]
        public int cost = 50;

        [Header("Combat (dùng bởi Dev3)")]
        public float damage = 20f;
        public float attackRange = 3f;
        [Tooltip("Số lần tấn công mỗi giây")]
        public float attackSpeed = 1f;

        [Header("UI")]
        [Tooltip("Sprite hiển thị trong HeroSelectionUI")]
        public Sprite heroIcon;
        [TextArea(2, 4)]
        public string description;
    }
}
