using System.Collections.Generic;
using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Singleton quản lý việc nâng cấp sao cho các quân cờ cùng cấp/cùng loại (Phase 12).
    /// </summary>
    public class UnitStarUpgradeManager : MonoBehaviour
    {
        public static UnitStarUpgradeManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[UnitStarUpgradeManager] Phát hiện instance trùng lặp, tự huỷ.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Thử gộp toàn bộ quân cờ của người chơi.
        /// Chạy vòng lặp để gộp liên tục (chain upgrade) cho đến khi không còn nhóm nào đủ 3 cờ cùng loại + cùng sao.
        /// </summary>
        public void TryUpgradeAllUnits()
        {
            // Chỉ nâng sao trong giai đoạn chuẩn bị hoặc wave hoàn thành
            if (GamePhaseManager.Instance != null && !GamePhaseManager.Instance.IsPreparationPhase)
            {
                Debug.LogWarning("[UnitStarUpgradeManager] Không thể nâng cấp cờ khi đang trong trận đấu!");
                return;
            }

            if (PlayerUnitManager.Instance == null)
            {
                Debug.LogWarning("[UnitStarUpgradeManager] Không tìm thấy PlayerUnitManager để kiểm tra nâng cấp!");
                return;
            }

            bool upgradedThisStep = true;
            int iterations = 0;
            const int MAX_ITERATIONS = 50; // Giới hạn an toàn chống vòng lặp vô hạn

            while (upgradedThisStep && iterations < MAX_ITERATIONS)
            {
                upgradedThisStep = false;
                iterations++;

                // Lấy tất cả quân cờ còn sống
                List<PlaceableUnit> allUnits = PlayerUnitManager.Instance.GetAllPlayerUnits();

                // Nhóm theo key: {unitId}_{starLevel}
                Dictionary<string, List<PlaceableUnit>> groups = new Dictionary<string, List<PlaceableUnit>>();

                foreach (var unit in allUnits)
                {
                    if (unit == null) continue;

                    Health hp = unit.GetComponent<Health>();
                    if (hp != null && hp.IsDead) continue; // Bỏ qua cờ đã chết

                    UnitStarData starData = unit.GetComponent<UnitStarData>();
                    if (starData == null) continue;

                    string key = $"{starData.unitId.Trim()}_{starData.starLevel}";
                    if (!groups.ContainsKey(key))
                    {
                        groups[key] = new List<PlaceableUnit>();
                    }
                    groups[key].Add(unit);
                }

                // Kiểm tra xem nhóm nào có ít nhất 3 quân cờ cùng loại/cùng sao
                foreach (var kvp in groups)
                {
                    if (kvp.Value.Count >= 3)
                    {
                        UnitStarData starData = kvp.Value[0].GetComponent<UnitStarData>();
                        if (starData == null || starData.starLevel >= UnitStarData.MaxStarLevel)
                        {
                            continue;
                        }

                        MergeThreeUnits(kvp.Value[0], kvp.Value[1], kvp.Value[2]);
                        upgradedThisStep = true;
                        break; // Thoát ra ngoài vòng foreach để reload danh sách mới, tiếp tục đệ quy nâng sao
                    }
                }
            }
        }

        /// <summary>
        /// Thực hiện gộp 3 quân cờ.
        /// </summary>
        private void MergeThreeUnits(PlaceableUnit u1, PlaceableUnit u2, PlaceableUnit u3)
        {
            UnitStarData sourceStarData = u1.GetComponent<UnitStarData>();
            if (sourceStarData == null || sourceStarData.starLevel >= UnitStarData.MaxStarLevel)
            {
                Debug.Log($"[UnitStarUpgradeManager] Bỏ qua gộp sao vì '{u1.unitName}' đã đạt cấp sao tối đa.");
                return;
            }

            List<PlaceableUnit> candidates = new List<PlaceableUnit> { u1, u2, u3 };

            // Xác định quân cờ chính (main unit) để giữ lại dựa trên thứ tự ưu tiên:
            // 1. Quân cờ đang đặt trên board (IsPlacedOnBoard == true)
            // 2. Quân cờ ở bench
            PlaceableUnit mainUnit = null;

            // Tìm quân cờ đầu tiên đang đứng trên board
            foreach (var u in candidates)
            {
                if (u.IsPlacedOnBoard)
                {
                    mainUnit = u;
                    break;
                }
            }

            // Nếu cả 3 đều ở bench, chọn quân cờ đầu tiên
            if (mainUnit == null)
            {
                mainUnit = candidates[0];
            }

            candidates.Remove(mainUnit);

            // Hai quân cờ còn lại là nguyên liệu và sẽ bị huỷ bỏ
            PlaceableUnit material1 = candidates[0];
            PlaceableUnit material2 = candidates[1];

            int currentStar = sourceStarData.starLevel;
            Debug.Log($"[UnitStarUpgradeManager] Gộp 3 quân cờ {u1.unitName} (Cấp sao hiện tại: {currentStar}). Giữ lại: {mainUnit.gameObject.name}");

            // Dọn dẹp lựa chọn trong UnitPlacementManager nếu quân cờ bị huỷ đang được chọn
            if (UnitPlacementManager.Instance != null)
            {
                UnitPlacementManager.Instance.ClearSelectionIfDestroyed(material1);
                UnitPlacementManager.Instance.ClearSelectionIfDestroyed(material2);
            }

            // Giải phóng các ô cờ trên board của quân cờ nguyên liệu (nếu có)
            material1.ReleaseCurrentCell();
            material2.ReleaseCurrentCell();

            // Huỷ bỏ các đối tượng quân cờ nguyên liệu
            Destroy(material1.gameObject);
            Destroy(material2.gameObject);

            // Tăng sao cho quân cờ chính lên +1 cấp sao
            UnitStarData mainStarData = mainUnit.GetComponent<UnitStarData>();
            if (mainStarData != null)
            {
                mainStarData.SetStarLevel(mainStarData.starLevel + 1);
                Debug.Log($"[UnitStarUpgradeManager] Quân cờ {mainUnit.unitName} đã nâng lên sao cấp: {mainStarData.starLevel}");
            }
        }
    }
}
