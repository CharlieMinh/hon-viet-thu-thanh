using UnityEngine;
using UnityEngine.InputSystem;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Quản lý luồng chọn quân → chọn ô → đặt quân.
    /// Singleton đơn giản, không dùng DontDestroyOnLoad.
    /// Sử dụng New Input System (com.unity.inputsystem).
    /// </summary>
    public class UnitPlacementManager : MonoBehaviour
    {
        // ---- Singleton ----
        public static UnitPlacementManager Instance { get; private set; }

        // ---- State ----
        /// <summary>Quân cờ đang được người chơi chọn (null = chưa chọn quân nào).</summary>
        public PlaceableUnit SelectedUnit { get; private set; }

        // ---------------------------------------------------------------
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[UnitPlacementManager] Phát hiện instance trùng lặp, huỷ bỏ bản mới.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ---------------------------------------------------------------
        #region Public API

        /// <summary>
        /// Chọn một quân cờ. Nếu click lại quân đang chọn → bỏ chọn.
        /// </summary>
        public void SelectUnit(PlaceableUnit unit)
        {
            if (unit == null) return;

            // Kiểm tra trạng thái game trước khi chọn quân (Phase 6)
            if (GamePhaseManager.Instance != null && !GamePhaseManager.Instance.IsPreparationPhase)
            {
                Debug.LogWarning("[UnitPlacementManager] Cannot select/place units during Combat.");
                return;
            }

            if (SelectedUnit == unit)
            {
                // Click lại quân đang chọn → bỏ chọn
                DeselectUnit();
                return;
            }

            // Bỏ highlight quân cũ
            if (SelectedUnit != null)
            {
                SelectedUnit.SetSelected(false);
            }

            SelectedUnit = unit;
            SelectedUnit.SetSelected(true);
            Debug.Log($"[UnitPlacementManager] Đã chọn quân: '{unit.unitName}'.");
        }

        /// <summary>
        /// Bỏ chọn quân hiện tại, không di chuyển về khu vực chờ.
        /// </summary>
        public void DeselectUnit()
        {
            if (SelectedUnit == null) return;

            SelectedUnit.SetSelected(false);
            Debug.Log($"[UnitPlacementManager] Bỏ chọn quân: '{SelectedUnit.unitName}'.");
            SelectedUnit = null;
        }

        /// <summary>
        /// Xóa bỏ lựa chọn quân cờ một cách an toàn nếu quân cờ đó bị hủy do gộp sao (Phase 12).
        /// </summary>
        public void ClearSelectionIfDestroyed(PlaceableUnit unit)
        {
            if (SelectedUnit == unit)
            {
                SelectedUnit = null;
            }
        }

        /// <summary>
        /// Thử đặt quân đang được chọn vào ô cờ chỉ định.
        /// Gọi từ BoardCell.OnMouseDown().
        /// </summary>
        /// <returns>True nếu đặt thành công.</returns>
        public bool TryPlaceSelectedUnit(BoardCell targetCell)
        {
            // Kiểm tra trạng thái game trước khi đặt quân (Phase 6)
            if (GamePhaseManager.Instance != null && !GamePhaseManager.Instance.IsPreparationPhase)
            {
                Debug.LogWarning("[UnitPlacementManager] Cannot place units during Combat.");
                return false;
            }

            if (SelectedUnit == null)
            {
                Debug.Log("[UnitPlacementManager] Chưa chọn quân nào.");
                return false;
            }

            if (targetCell == null || targetCell.isOccupied)
            {
                Debug.LogWarning($"[UnitPlacementManager] Ô ({targetCell?.row}, {targetCell?.column}) đã bị chiếm hoặc không hợp lệ.");
                return false;
            }

            PlaceableUnit unit = SelectedUnit;
            DeselectUnit(); // Bỏ chọn TRƯỚC khi đặt để màu unit cập nhật đúng

            unit.PlaceOnCell(targetCell);
            return true;
        }

        #endregion

        // ---------------------------------------------------------------
        #region Input Handling

        private void Update()
        {
            // Nhấn ESC → hủy chọn (Giao chuột phải cho RightClickInspectController xử lý)
            bool escPressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;

            if (escPressed)
            {
                if (SelectedUnit != null)
                {
                    Debug.Log("[UnitPlacementManager] Hủy chọn quân.");
                    DeselectUnit();
                }
            }
        }

        #endregion
    }
}

