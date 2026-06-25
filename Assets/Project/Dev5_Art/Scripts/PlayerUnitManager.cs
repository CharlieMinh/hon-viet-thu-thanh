using System.Collections.Generic;
using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Quản lý danh sách các quân cờ đang chiến đấu trên bàn cờ (placed units).
    /// Hỗ trợ tìm kiếm quân cờ gần nhất cho Enemy AI phản công và kiểm soát trạng thái thua trận (Lose).
    /// </summary>
    public class PlayerUnitManager : MonoBehaviour
    {
        public static PlayerUnitManager Instance { get; private set; }

        [Header("Danh sách theo dõi")]
        [SerializeField] private List<PlaceableUnit> activeUnits = new List<PlaceableUnit>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[PlayerUnitManager] Phát hiện instance trùng lặp, tự huỷ.");
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
        /// Đăng ký quân cờ khi đặt lên bàn cờ.
        /// </summary>
        public void RegisterUnit(PlaceableUnit unit)
        {
            if (unit == null) return;
            if (!activeUnits.Contains(unit))
            {
                activeUnits.Add(unit);
                Debug.Log($"[PlayerUnitManager] Đăng ký cờ: {unit.unitName}. Tổng số cờ trên bàn: {activeUnits.Count}");
            }
        }

        /// <summary>
        /// Huỷ đăng ký quân cờ khỏi danh sách chiến đấu.
        /// </summary>
        public void UnregisterUnit(PlaceableUnit unit)
        {
            if (unit == null) return;
            if (activeUnits.Contains(unit))
            {
                activeUnits.Remove(unit);
                Debug.Log($"[PlayerUnitManager] Huỷ đăng ký cờ: {unit.unitName}. Số cờ còn sống: {activeUnits.Count}");

                // Chỉ kiểm tra thua trận trong phase Combat
                if (GamePhaseManager.Instance != null && GamePhaseManager.Instance.IsCombatPhase)
                {
                    CheckAllPlayerUnitsDefeated();
                }
            }
        }

        /// <summary>
        /// Tìm quân cờ còn sống gần nhất với toạ độ truyền vào.
        /// </summary>
        public PlaceableUnit GetNearestPlacedUnit(Vector3 position)
        {
            PlaceableUnit nearest = null;
            float minDistance = float.MaxValue;

            for (int i = activeUnits.Count - 1; i >= 0; i--)
            {
                var unit = activeUnits[i];
                if (unit == null) continue;

                Health hp = unit.GetComponent<Health>();
                if (hp == null || hp.IsDead) continue;

                float dist = Vector3.Distance(position, unit.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearest = unit;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Lấy số lượng quân cờ còn sống trên bàn cờ.
        /// </summary>
        public int GetAlivePlacedUnitCount()
        {
            int count = 0;
            for (int i = 0; i < activeUnits.Count; i++)
            {
                var unit = activeUnits[i];
                if (unit != null)
                {
                    Health hp = unit.GetComponent<Health>();
                    if (hp != null && !hp.IsDead)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Kiểm tra xem có cờ nào của người chơi còn sống không.
        /// </summary>
        public bool HasAlivePlacedUnits()
        {
            return GetAlivePlacedUnitCount() > 0;
        }

        /// <summary>
        /// Lấy danh sách các quân cờ đang chiến đấu và còn sống trên board (Phase 10).
        /// </summary>
        public List<PlaceableUnit> GetAlivePlacedUnits()
        {
            List<PlaceableUnit> list = new List<PlaceableUnit>();
            for (int i = 0; i < activeUnits.Count; i++)
            {
                var unit = activeUnits[i];
                if (unit != null && unit.IsPlacedOnBoard)
                {
                    Health hp = unit.GetComponent<Health>();
                    if (hp != null && !hp.IsDead)
                    {
                        list.Add(unit);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Lấy toàn bộ quân cờ của người chơi (gồm cả trên board và bench) còn sống (Phase 12).
        /// </summary>
        public List<PlaceableUnit> GetAllPlayerUnits()
        {
            List<PlaceableUnit> list = new List<PlaceableUnit>();
            PlaceableUnit[] units = Object.FindObjectsByType<PlaceableUnit>(FindObjectsSortMode.None);
            foreach (var unit in units)
            {
                if (unit != null)
                {
                    Health hp = unit.GetComponent<Health>();
                    if (hp != null && hp.IsDead) continue;
                    list.Add(unit);
                }
            }
            return list;
        }

        /// <summary>
        /// Kiểm tra và kích hoạt thua trận nếu không còn quân cờ nào trên bàn cờ.
        /// </summary>
        public void CheckAllPlayerUnitsDefeated()
        {
            if (GamePhaseManager.Instance != null && GamePhaseManager.Instance.IsCombatPhase)
            {
                if (GetAlivePlacedUnitCount() == 0)
                {
                    Debug.Log("[PlayerUnitManager] All player units defeated - Lose");
                    GamePhaseManager.Instance.LoseGame();
                }
            }
        }

        /// <summary>
        /// Tìm mục tiêu ưu tiên cho Enemy AI dựa trên cơ chế Taunt/Threat của Tank (Phase 14).
        /// Ưu tiên các cờ có isTank = true nằm trong tầm tauntRadius của kẻ địch.
        /// Nếu không có Tank nào thỏa mãn, trả về quân cờ gần nhất như bình thường.
        /// </summary>
        public PlaceableUnit GetPriorityTargetForEnemy(Vector3 enemyPosition)
        {
            PlaceableUnit nearestTank = null;
            float minTankDistance = float.MaxValue;

            // 1. Tìm các cờ Tank hợp lệ trong tầm taunt
            for (int i = activeUnits.Count - 1; i >= 0; i--)
            {
                var unit = activeUnits[i];
                if (unit == null || !unit.IsPlacedOnBoard) continue;

                Health hp = unit.GetComponent<Health>();
                if (hp == null || hp.IsDead) continue;

                UnitRole role = unit.GetComponent<UnitRole>();
                if (role == null || !role.isTank) continue;

                float dist = Vector3.Distance(enemyPosition, unit.transform.position);
                if (dist <= role.tauntRadius)
                {
                    if (dist < minTankDistance)
                    {
                        minTankDistance = dist;
                        nearestTank = unit;
                    }
                }
            }

            // Nếu tìm thấy Tank đang taunt kẻ địch -> Ưu tiên đánh Tank này
            if (nearestTank != null)
            {
                return nearestTank;
            }

            // 2. Fallback: Nếu không có Tank nào trong tầm taunt -> Chọn cờ gần nhất bình thường
            return GetNearestPlacedUnit(enemyPosition);
        }
    }
}
