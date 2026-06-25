using System.Collections.Generic;
using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Quản lý việc lưu trữ snapshot đội hình cờ trên bàn trước trận đấu 
    /// và đưa tất cả quân cờ còn sống trở lại ô cờ gốc khi wave đấu kết thúc (Phase 10).
    /// </summary>
    public class BattleResetManager : MonoBehaviour
    {
        public static BattleResetManager Instance { get; private set; }

        [System.Serializable]
        public struct UnitPositionSnapshot
        {
            public PlaceableUnit unit;
            public BoardCell originalCell;
        }

        [Header("Danh sách Snapshot")]
        [SerializeField] private List<UnitPositionSnapshot> snapshots = new List<UnitPositionSnapshot>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[BattleResetManager] Phát hiện instance trùng lặp, tự huỷ.");
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
        /// Chụp lại đội hình cờ trên bàn trước khi bắt đầu Combat (Phase 10).
        /// </summary>
        public void CaptureCurrentBoardFormation()
        {
            ClearSnapshot();

            if (PlayerUnitManager.Instance == null)
            {
                Debug.LogWarning("[BattleResetManager] Không tìm thấy PlayerUnitManager để lưu đội hình!");
                return;
            }

            List<PlaceableUnit> aliveUnits = PlayerUnitManager.Instance.GetAlivePlacedUnits();
            foreach (var unit in aliveUnits)
            {
                if (unit != null && unit.CurrentCell != null)
                {
                    UnitPositionSnapshot snapshot = new UnitPositionSnapshot
                    {
                        unit = unit,
                        originalCell = unit.CurrentCell
                    };
                    snapshots.Add(snapshot);
                    Debug.Log($"[BattleResetManager] Lưu vị trí cờ '{unit.unitName}' tại ô ({unit.CurrentCell.row}, {unit.CurrentCell.column}).");
                }
            }
        }

        /// <summary>
        /// Đưa toàn bộ cờ còn sống quay về ô cũ và reset trạng thái chiến đấu (Phase 10).
        /// </summary>
        public void ReturnSurvivingUnitsToFormation()
        {
            foreach (var snapshot in snapshots)
            {
                if (snapshot.unit == null)
                {
                    // Cờ đã chết trong combat
                    continue;
                }

                Health hp = snapshot.unit.GetComponent<Health>();
                if (hp != null && hp.IsDead)
                {
                    continue;
                }

                if (snapshot.originalCell == null)
                {
                    Debug.LogWarning($"[BattleResetManager] Ô cờ gốc của cờ '{snapshot.unit.unitName}' bị null!");
                    continue;
                }

                // Reset vị trí và ô occupied
                snapshot.unit.ForcePlaceBackToCell(snapshot.originalCell);

                // Reset trạng thái combat (cooldown, target, movement)
                UnitAutoAttack autoAttack = snapshot.unit.GetComponent<UnitAutoAttack>();
                if (autoAttack != null)
                {
                    autoAttack.ResetCombatState();
                }
            }
        }

        /// <summary>
        /// Xóa dữ liệu snapshot (Phase 10).
        /// </summary>
        public void ClearSnapshot()
        {
            snapshots.Clear();
        }
    }
}
