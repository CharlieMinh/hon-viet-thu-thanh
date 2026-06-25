using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Component quản lý dữ liệu sao gắn trên từng quân cờ (Phase 12).
    /// </summary>
    public class UnitStarData : MonoBehaviour
    {
        [Header("Thông tin sao")]
        public string unitId;
        public int starLevel = 1;

        private void Start()
        {
            // Tự động đồng bộ với PlaceableUnit's unitName làm unitId nếu unitId trống
            if (string.IsNullOrEmpty(unitId))
            {
                PlaceableUnit pu = GetComponent<PlaceableUnit>();
                if (pu != null)
                {
                    unitId = pu.unitName;
                }
            }
        }

        /// <summary>
        /// Gán cấp sao mới và cập nhật chỉ số cũng như visual tương ứng.
        /// </summary>
        public void SetStarLevel(int newStarLevel)
        {
            starLevel = Mathf.Max(1, newStarLevel);

            // Cập nhật chỉ số chiến đấu
            UnitCombatStats stats = GetComponent<UnitCombatStats>();
            if (stats != null)
            {
                stats.ApplyStarMultiplier(starLevel);
            }

            // Cập nhật hiển thị visual sao
            UnitStarVisual visual = GetComponent<UnitStarVisual>();
            if (visual != null)
            {
                visual.RefreshVisual();
            }
        }
    }
}
