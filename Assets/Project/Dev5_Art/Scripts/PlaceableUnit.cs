using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Đại diện cho một quân cờ placeholder có thể được chọn và đặt lên board.
    /// Gắn script này lên GameObject quân cờ (Capsule placeholder).
    /// </summary>
    public class PlaceableUnit : MonoBehaviour
    {
        [Header("Thông tin quân cờ")]
        [Tooltip("Tên/loại quân cờ để dễ nhận biết")]
        public string unitName = "Unit";

        [Header("Màu sắc quân cờ")]
        public Color normalColor   = new Color(0.2f, 0.6f, 1.0f, 1f); // Xanh dương
        public Color selectedColor = new Color(1.0f, 0.8f, 0.1f, 1f); // Vàng
        public Color placedColor   = new Color(0.3f, 0.9f, 0.3f, 1f); // Xanh lá

        // ---- State ----
        /// <summary>Ô hiện tại đang chiếm (null nếu đang ở khu vực chờ).</summary>
        public BoardCell OccupiedCell { get; private set; }

        public BoardCell CurrentCell => OccupiedCell;

        /// <summary>Trả về true nếu quân cờ đã được đặt trên bàn cờ.</summary>
        public bool IsPlacedOnBoard => OccupiedCell != null;

        /// <summary>Vị trí gốc trong khu vực chờ, dùng để khôi phục nếu cần.</summary>
        private Vector3 _waitingPosition;
        private bool _initialized = false;

        private MeshRenderer _renderer;
        private MaterialPropertyBlock _propBlock;
        private Health _health;

        private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorID     = Shader.PropertyToID("_Color");

        // ---------------------------------------------------------------
        private void Awake()
        {
            // Tìm MeshRenderer ở Placeholder để tránh đổi màu Model thật dưới ModelSlot
            Transform visualTrans = transform.Find("Visual");
            if (visualTrans != null)
            {
                Transform placeholderTrans = visualTrans.Find("Placeholder");
                if (placeholderTrans != null)
                {
                    _renderer = placeholderTrans.GetComponent<MeshRenderer>();
                }
            }

            if (_renderer == null)
            {
                _renderer = GetComponentInChildren<MeshRenderer>();
            }
            _propBlock = new MaterialPropertyBlock();
        }

        private void Start()
        {
            _waitingPosition = transform.position;
            _initialized = true;
            RefreshColor();

            _health = GetComponent<Health>();
            if (_health != null)
            {
                _health.OnDeath += HandleDeath;
            }

            // Nếu đã được đặt trên ô cờ ngay từ đầu, đăng ký với PlayerUnitManager
            if (OccupiedCell != null && PlayerUnitManager.Instance != null)
            {
                PlayerUnitManager.Instance.RegisterUnit(this);
            }
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.OnDeath -= HandleDeath;
            }
            if (PlayerUnitManager.Instance != null)
            {
                PlayerUnitManager.Instance.UnregisterUnit(this);
            }
        }

        private void HandleDeath()
        {
            if (OccupiedCell != null)
            {
                OccupiedCell.SetOccupied(false, null);
                OccupiedCell = null;
            }
            if (PlayerUnitManager.Instance != null)
            {
                PlayerUnitManager.Instance.UnregisterUnit(this);
            }
        }

        // ---------------------------------------------------------------
        #region Public API

        /// <summary>Cập nhật hiển thị khi trạng thái chọn thay đổi.</summary>
        public void SetSelected(bool selected)
        {
            RefreshColor(selected);
        }

        /// <summary>
        /// Đặt quân lên ô cờ chỉ định.
        /// - Giải phóng ô cũ (nếu có).
        /// - Đánh dấu ô mới là occupied.
        /// - Di chuyển GameObject lên tâm ô.
        /// </summary>
        public void PlaceOnCell(BoardCell targetCell)
        {
            if (targetCell == null) return;

            // Giải phóng ô cũ
            if (OccupiedCell != null)
            {
                OccupiedCell.SetOccupied(false, null);
            }

            // Chiếm ô mới
            OccupiedCell = targetCell;
            targetCell.SetOccupied(true, this);

            // Di chuyển tới tâm ô bằng cách sử dụng helper của BoardManager (offset Y = 1.0f cho Capsule cao 2 đơn vị)
            if (targetCell.Manager != null)
            {
                transform.position = targetCell.Manager.GetUnitPlacementPosition(targetCell, 1.0f);
            }
            else
            {
                Vector3 cellCenter = targetCell.transform.position;
                cellCenter.y += targetCell.transform.localScale.y * 0.5f + 1.0f;
                transform.position = cellCenter;
            }

            // Đăng ký với PlayerUnitManager
            if (PlayerUnitManager.Instance != null)
            {
                PlayerUnitManager.Instance.RegisterUnit(this);
            }

            RefreshColor();
            Debug.Log($"[PlaceableUnit] '{unitName}' đặt lên ô ({targetCell.row}, {targetCell.column}).");
        }

        /// <summary>
        /// Trả quân về khu vực chờ (ví dụ khi người dùng hủy).
        /// </summary>
        public void ReturnToWaiting()
        {
            if (OccupiedCell != null)
            {
                OccupiedCell.SetOccupied(false, null);
                OccupiedCell = null;
            }

            // Hủy đăng ký khỏi PlayerUnitManager
            if (PlayerUnitManager.Instance != null)
            {
                PlayerUnitManager.Instance.UnregisterUnit(this);
            }

            transform.position = _waitingPosition;
            RefreshColor();
        }

        /// <summary>
        /// Buộc quân cờ quay về ô cờ chỉ định và tái lập occupied (Phase 10).
        /// </summary>
        public void ForcePlaceBackToCell(BoardCell cell)
        {
            if (cell == null) return;

            // Đặt transform về vị trí đặt của cell
            if (cell.Manager != null)
            {
                transform.position = cell.Manager.GetUnitPlacementPosition(cell, 1.0f);
            }
            else
            {
                Vector3 cellCenter = cell.transform.position;
                cellCenter.y += cell.transform.localScale.y * 0.5f + 1.0f;
                transform.position = cellCenter;
            }

            // Đồng bộ trạng thái ô cờ
            OccupiedCell = cell;
            cell.SetOccupied(true, this);

            // Khôi phục góc xoay mặc định hướng về phía trước
            transform.rotation = Quaternion.identity;

            RefreshColor();
            Debug.Log($"[PlaceableUnit] Reset cờ '{unitName}' về ô ({cell.row}, {cell.column}).");
        }

        /// <summary>
        /// Giải phóng ô cờ hiện tại và huỷ đăng ký khỏi PlayerUnitManager (Phase 12).
        /// </summary>
        public void ReleaseCurrentCell()
        {
            if (OccupiedCell != null)
            {
                OccupiedCell.SetOccupied(false, null);
                OccupiedCell = null;
            }

            if (PlayerUnitManager.Instance != null)
            {
                PlayerUnitManager.Instance.UnregisterUnit(this);
            }
            RefreshColor();
        }

        #endregion

        // ---------------------------------------------------------------
        #region Mouse Interaction

        private void OnMouseDown()
        {
            UnitPlacementManager.Instance?.SelectUnit(this);
        }

        private void OnMouseEnter()
        {
            // Nếu không phải đang được chọn thì highlight nhẹ
            if (UnitPlacementManager.Instance?.SelectedUnit != this)
            {
                ApplyColor(Color.Lerp(normalColor, selectedColor, 0.35f));
            }
        }

        private void OnMouseExit()
        {
            if (UnitPlacementManager.Instance?.SelectedUnit != this)
            {
                RefreshColor();
            }
        }

        #endregion

        // ---------------------------------------------------------------
        #region Color Helpers

        private void RefreshColor(bool forceSelected = false)
        {
            if (!_initialized && Application.isPlaying) return;

            bool selected = forceSelected || (UnitPlacementManager.Instance?.SelectedUnit == this);
            Color target;

            if (selected)
                target = selectedColor;
            else if (OccupiedCell != null)
                target = placedColor;
            else
                target = normalColor;

            ApplyColor(target);
        }

        private void ApplyColor(Color color)
        {
            if (_renderer == null) return;
            _renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(BaseColorID, color);
            _propBlock.SetColor(ColorID, color);
            _renderer.SetPropertyBlock(_propBlock);
        }

        #endregion
    }
}
