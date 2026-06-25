using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Đại diện cho một ô cờ đơn lẻ trên bàn cờ đặt quân (Phase 1).
    /// Quản lý trạng thái hiển thị màu sắc khi hover, click và khi ô bị chiếm.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(MeshRenderer))]
    public class BoardCell : MonoBehaviour
    {
        [Header("Tọa độ ô")]
        [ReadOnlyInspector] public int row;
        [ReadOnlyInspector] public int column;

        [Header("Trạng thái")]
        [Tooltip("Đánh dấu ô này đã có quân cờ đứng hay chưa")]
        public bool isOccupied = false;

        /// <summary>Quân cờ đang chiếm ô này (null = trống).</summary>
        public PlaceableUnit OccupyingUnit { get; private set; }

        [Header("Cấu hình màu sắc")]
        public Color normalColor = new Color(0.8f, 0.8f, 0.8f, 1f); // Xám nhạt
        public Color hoverColor = new Color(0.3f, 0.8f, 0.3f, 1f);  // Xanh lá nhẹ
        public Color selectedColor = new Color(0.1f, 0.5f, 0.9f, 1f); // Xanh dương
        public Color occupiedColor = new Color(0.9f, 0.2f, 0.2f, 1f); // Đỏ

        private BoardManager manager;
        public BoardManager Manager => manager;
        private MeshRenderer rendererComponent;
        private bool isHovered = false;

        /// <summary>
        /// Khởi tạo ô cờ với tọa độ hàng, cột và tham chiếu đến Manager.
        /// </summary>
        public void Init(int row, int column, BoardManager manager)
        {
            this.row = row;
            this.column = column;
            this.manager = manager;
            RefreshColor();
        }

        private void Awake()
        {
            rendererComponent = GetComponent<MeshRenderer>();
        }

        private void Start()
        {
            RefreshColor();
        }

        private static readonly int BaseColorPropID = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorPropID = Shader.PropertyToID("_Color");
        private MaterialPropertyBlock propBlock;

        /// <summary>
        /// Làm mới màu sắc hiển thị của ô cờ dựa trên trạng thái hiện tại.
        /// </summary>
        public void RefreshColor(bool hoveredState = false)
        {
            if (rendererComponent == null)
            {
                rendererComponent = GetComponent<MeshRenderer>();
            }

            if (rendererComponent == null) return;

            if (propBlock == null)
            {
                propBlock = new MaterialPropertyBlock();
            }

            // Thứ tự ưu tiên màu sắc: Bị chiếm > Được chọn > Hover > Bình thường
            Color targetColor = normalColor;

            if (isOccupied)
            {
                targetColor = occupiedColor;
            }
            else if (manager != null && manager.CurrentSelectedCell == this)
            {
                targetColor = selectedColor;
            }
            else if (hoveredState)
            {
                targetColor = hoverColor;
            }

            rendererComponent.GetPropertyBlock(propBlock);
            propBlock.SetColor(BaseColorPropID, targetColor);
            propBlock.SetColor(ColorPropID, targetColor);
            rendererComponent.SetPropertyBlock(propBlock);
        }

        private void OnMouseEnter()
        {
            if (isOccupied) return;
            isHovered = true;
            RefreshColor(true);
        }

        private void OnMouseExit()
        {
            if (isOccupied) return;
            isHovered = false;
            RefreshColor(false);
        }

        private void OnMouseDown()
        {
            // Nếu đang có quân được chọn → thử đặt quân vào ô này
            if (UnitPlacementManager.Instance != null && UnitPlacementManager.Instance.SelectedUnit != null)
            {
                UnitPlacementManager.Instance.TryPlaceSelectedUnit(this);
                return;
            }

            // Nếu ô đã bị chiếm → không làm gì thêm
            if (isOccupied)
            {
                Debug.LogWarning($"[BoardCell] Ô ({row}, {column}) đang bị chiếm, không thể chọn!");
                return;
            }

            // Chọn ô thông thường (Phase 1)
            if (manager != null)
            {
                manager.SelectCell(this);
            }
        }

        /// <summary>
        /// Thiết lập trạng thái bị chiếm của ô cờ.
        /// Được gọi bởi PlaceableUnit khi đặt/rời ô.
        /// </summary>
        public void SetOccupied(bool occupied, PlaceableUnit unit)
        {
            isOccupied = occupied;
            OccupyingUnit = occupied ? unit : null;

            // Nếu ô đang được chọn mà bị chiếm → bỏ chọn ô đó
            if (occupied && manager != null && manager.CurrentSelectedCell == this)
            {
                manager.SelectCell(null);
            }

            RefreshColor(isHovered);
        }

        private void OnValidate()
        {
            // Cho phép cập nhật màu trực tiếp trong Inspector khi chỉnh sửa isOccupied
            RefreshColor(isHovered);
        }
    }

    /// <summary>
    /// Attribute đơn giản hiển thị ReadOnly trên Inspector của Unity.
    /// </summary>
    public class ReadOnlyInspectorAttribute : PropertyAttribute { }
}
