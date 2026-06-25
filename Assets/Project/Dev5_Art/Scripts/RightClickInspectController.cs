using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Controller lắng nghe sự kiện nhấn chuột phải để inspect quân cờ hoặc kẻ địch (Phase 16).
    /// </summary>
    public class RightClickInspectController : MonoBehaviour
    {
        private void Update()
        {
            // Kiểm tra nhấn chuột phải sử dụng New Input System
            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                // Ngăn chặn tương tác nếu người chơi click lên UI
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                HandleRightClick();
            }
        }

        private void HandleRightClick()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null) return;

            // Bắn Raycast từ vị trí chuột
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = mainCam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // 1. Kiểm tra nếu trúng Player Unit
                PlaceableUnit unit = hit.collider.GetComponentInParent<PlaceableUnit>();
                if (unit != null)
                {
                    if (InspectPanel.Instance != null)
                    {
                        Debug.Log($"[RightClickInspect] Nhấp chuột phải trúng Player Unit: '{unit.unitName}'. Mở bảng Inspect.");
                        InspectPanel.Instance.ShowPlayerUnit(unit);
                    }
                    return;
                }

                // 2. Kiểm tra nếu trúng Enemy
                EnemyController enemy = hit.collider.GetComponentInParent<EnemyController>();
                if (enemy != null)
                {
                    if (InspectPanel.Instance != null)
                    {
                        Debug.Log($"[RightClickInspect] Nhấp chuột phải trúng Enemy: '{enemy.enemyName}'. Mở bảng Inspect.");
                        InspectPanel.Instance.ShowEnemy(enemy);
                    }
                    return;
                }
            }

            // 3. Nếu chuột phải trúng nền trống (hoặc ngoài đối tượng tương tác) -> Huỷ chọn cờ đang cầm và ẩn Inspect Panel
            Debug.Log("[RightClickInspect] Nhấp chuột phải vào nền trống. Huỷ chọn cờ hiện tại và ẩn Inspect Panel.");
            
            if (UnitPlacementManager.Instance != null)
            {
                UnitPlacementManager.Instance.DeselectUnit();
            }

            if (InspectPanel.Instance != null)
            {
                InspectPanel.Instance.Hide();
            }
        }
    }
}
