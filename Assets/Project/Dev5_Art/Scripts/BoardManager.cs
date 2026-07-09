using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Quản lý việc tự động sinh lưới ô cờ và xử lý logic chọn ô cờ (Phase 1).
    /// Phiên bản gốc sử dụng 1 transform cha boardParent duy nhất.
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        [Header("Cấu hình lưới ô cờ")]
        [Min(1)] public int rows = 3;
        [Min(1)] public int columns = 4;
        
        [Tooltip("Kích thước thực tế của từng ô (X = Chiều rộng, Y = Độ dày, Z = Chiều dài)")]
        public Vector3 cellSize = new Vector3(1.8f, 0.1f, 1.8f);
        
        [Tooltip("Khoảng cách giữa các ô cờ")]
        public float spacing = 0.2f;

        [Header("Tham chiếu")]
        [Tooltip("Prefab của ô cờ (phải chứa component BoardCell và Collider)")]
        public BoardCell cellPrefab;

        [Tooltip("Transform cha để chứa các ô cờ được sinh ra (Nếu để trống sẽ lấy chính BoardManager này)")]
        public Transform boardParent;

        // Ô cờ đang được chọn hiện tại
        public BoardCell CurrentSelectedCell { get; private set; }

        // Mảng 2 chiều lưu trữ danh sách các ô cờ
        private BoardCell[,] cells;

        private void Start()
        {
            // Kiểm tra xem đã có ô cờ nào được sinh sẵn từ Editor chưa
            Transform parent = boardParent != null ? boardParent : transform;
            if (parent.childCount == 0)
            {
                GenerateBoard();
            }
            else
            {
                RebuildCellsReference();
            }

            // Tự động sinh Unit và Manager cho Phase 2 nếu chưa có
            SetupPhase2UnitsIfNeeded();
        }

        /// <summary>
        /// Tự động thiết lập các unit cho Phase 2 nếu chưa tồn tại trong Scene.
        /// Giúp người chơi có thể test ngay khi nhấn Play mà không cần setup thủ công.
        /// </summary>
        private void SetupPhase2UnitsIfNeeded()
        {
            // 1. Tạo UnitPlacementManager nếu chưa có
            if (Object.FindAnyObjectByType<UnitPlacementManager>() == null)
            {
                GameObject upmGO = new GameObject("UnitPlacementManager");
                upmGO.AddComponent<UnitPlacementManager>();
                Debug.Log("[BoardManager] Tự động tạo UnitPlacementManager cho Phase 2.");
            }

            // 2. Tạo PlayerUnits container nếu chưa có
            GameObject playerUnitsGO = GameObject.Find("PlayerUnits");
            if (playerUnitsGO == null)
            {
                playerUnitsGO = new GameObject("PlayerUnits");
                Debug.Log("[BoardManager] Tự động tạo PlayerUnits container cho Phase 2.");
            }

            // Cấu hình vị trí và màu sắc
            Vector3 knightPos = new Vector3(-3.5f, 1.05f, -1f);
            Vector3 archerPos = new Vector3(-3.5f, 1.05f, 0f);
            Vector3 tankPos = new Vector3(-3.5f, 1.05f, 1f);

            Color knightColor = new Color(0.20f, 0.50f, 1.00f, 1f); // Xanh dương
            Color archerColor = new Color(1.00f, 0.80f, 0.15f, 1f); // Vàng
            Color tankColor = new Color(0.55f, 0.55f, 0.55f, 1f);   // Xám

            // 3. Tạo các unit (Chỉ khi debugMode của GameConfig được bật)
            if (GameConfig.Instance != null && GameConfig.Instance.debugMode)
            {
                CreatePhase2Unit("Knight_TestUnit", "Knight", knightPos, knightColor, playerUnitsGO.transform);
                CreatePhase2Unit("Archer_TestUnit", "Archer", archerPos, archerColor, playerUnitsGO.transform);
                CreatePhase2Unit("Tank_TestUnit", "Tank", tankPos, tankColor, playerUnitsGO.transform);
            }
            else
            {
                Debug.Log("[BoardManager] Chế độ chơi thường (Normal Mode) hoặc thiếu GameConfig: Không sinh cờ test mặc định.");
            }
        }

        private void CreatePhase2Unit(string goName, string unitName, Vector3 position, Color color, Transform parent)
        {
            if (GameObject.Find(goName) != null) return;

            // Tạo Capsule
            GameObject unitGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            unitGO.name = goName;
            unitGO.transform.position = position;
            unitGO.transform.SetParent(parent);

            // Đặt màu qua MaterialPropertyBlock
            MeshRenderer rend = unitGO.GetComponent<MeshRenderer>();
            if (rend != null)
            {
                var propBlock = new MaterialPropertyBlock();
                rend.GetPropertyBlock(propBlock);
                propBlock.SetColor("_BaseColor", color);
                propBlock.SetColor("_Color", color);
                rend.SetPropertyBlock(propBlock);
            }

            // Gắn PlaceableUnit
            PlaceableUnit pu = unitGO.AddComponent<PlaceableUnit>();
            pu.unitName = unitName;
            pu.normalColor = color;
            pu.placedColor = Color.Lerp(color, Color.white, 0.35f);
            pu.selectedColor = new Color(1f, 0.85f, 0.1f, 1f);

            // Gắn chỉ số chiến đấu và script tự đánh cho Phase 5 & 7
            UnitCombatStats stats = unitGO.AddComponent<UnitCombatStats>();
            stats.rotationSpeed = 10f; // Tốc độ xoay chung

            if (unitName.Equals("Knight", System.StringComparison.OrdinalIgnoreCase))
            {
                stats.damage = 10;
                stats.attackRange = 1.5f;
                stats.attackCooldown = 1.0f;
                stats.goldPerHit = 1;
                stats.moveSpeed = 3f;
            }
            else if (unitName.Equals("Archer", System.StringComparison.OrdinalIgnoreCase))
            {
                stats.damage = 8;
                stats.attackRange = 5.0f;
                stats.attackCooldown = 1.2f;
                stats.goldPerHit = 1;
                stats.moveSpeed = 3f;
            }
            else if (unitName.Equals("Tank", System.StringComparison.OrdinalIgnoreCase))
            {
                stats.damage = 6;
                stats.attackRange = 1.3f;
                stats.attackCooldown = 1.5f;
                stats.goldPerHit = 1;
                stats.moveSpeed = 2f;
            }

            unitGO.AddComponent<UnitAutoAttack>();

            Debug.Log($"[BoardManager] Tự động tạo unit '{goName}' tại vị trí chờ {position}.");
        }

        /// <summary>
        /// Tự động sinh lưới các ô cờ dựa trên cấu hình.
        /// </summary>
        [ContextMenu("Generate Board")]
        public void GenerateBoard()
        {
            if (cellPrefab == null)
            {
                Debug.LogError("[BoardManager] Chưa gán Cell Prefab! Vui lòng kéo thả BoardCell prefab vào Inspector.");
                return;
            }

            ClearBoard();

            cells = new BoardCell[rows, columns];
            Transform parent = boardParent != null ? boardParent : transform;

            // Tính toán tổng kích thước bàn cờ để tự động căn giữa (auto-center)
            float totalWidth = columns * cellSize.x + (columns - 1) * spacing;
            float totalLength = rows * cellSize.z + (rows - 1) * spacing;

            // Vị trí bắt đầu (tương đối với vị trí của BoardManager)
            float startX = -totalWidth / 2f + cellSize.x / 2f;
            float startZ = -totalLength / 2f + cellSize.z / 2f;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    float posX = startX + c * (cellSize.x + spacing);
                    float posZ = startZ + r * (cellSize.z + spacing);
                    Vector3 localPos = new Vector3(posX, 0f, posZ);

                    // Sinh ô cờ
                    BoardCell cell = Instantiate(cellPrefab, parent);
                    cell.transform.localPosition = localPos;
                    cell.transform.localScale = cellSize;
                    cell.transform.localRotation = Quaternion.identity;
                    cell.name = $"BoardCell_R{r}_C{c}";

                    // Khởi tạo thông tin ô cờ
                    cell.Init(r, c, this);
                    cells[r, c] = cell;
                }
            }

            Debug.Log($"[BoardManager] Đã sinh thành công bàn cờ {columns} cột x {rows} hàng.");
        }

        /// <summary>
        /// Xóa sạch các ô cờ hiện có trên bàn cờ.
        /// </summary>
        [ContextMenu("Clear Board")]
        public void ClearBoard()
        {
            Transform parent = boardParent != null ? boardParent : transform;
            
            // Xóa toàn bộ con của boardParent
            int childCount = parent.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                GameObject child = parent.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }

            cells = null;
            CurrentSelectedCell = null;
        }

        /// <summary>
        /// Xử lý logic chọn một ô cờ.
        /// </summary>
        public void SelectCell(BoardCell cell)
        {
            // Cho phép gọi SelectCell(null) để bỏ chọn ô hiện tại
            if (cell != null && cell.isOccupied) return;

            BoardCell oldSelected = CurrentSelectedCell;
            CurrentSelectedCell = cell;

            // Làm mới hiển thị của ô cũ (để quay về màu bình thường)
            if (oldSelected != null)
            {
                oldSelected.RefreshColor();
            }

            // Làm mới hiển thị của ô mới (để chuyển sang màu selected)
            if (CurrentSelectedCell != null)
            {
                CurrentSelectedCell.RefreshColor();
                Debug.Log($"[BoardManager] Đã chọn ô cờ tại vị trí Hàng: {cell.row}, Cột: {cell.column}");
            }
        }

        /// <summary>
        /// Tính toán vị trí thế giới để đặt unit lên tâm ô cờ.
        /// Unit sẽ đứng ngay trên mặt ô, không bị chìm.
        /// </summary>
        /// <param name="cell">Ô cờ cần tính vị trí.</param>
        /// <param name="unitHeightOffset">Khoảng bù thêm theo trục Y (mặc định 0.5 = bán kính Capsule).</param>
        /// <returns>Vị trí thế giới để đặt unit.</returns>
        public Vector3 GetUnitPlacementPosition(BoardCell cell, float unitHeightOffset = 0.5f)
        {
            if (cell == null) return Vector3.zero;

            // Tâm ô theo X và Z, mặt trên ô theo Y
            Vector3 cellTop = cell.transform.position;
            cellTop.y += cell.transform.localScale.y * 0.5f + unitHeightOffset;
            return cellTop;
        }

        /// <summary>
        /// Lấy thông tin ô cờ tại tọa độ hàng/cột.
        /// </summary>
        public BoardCell GetCell(int row, int column)
        {
            if (cells == null)
            {
                RebuildCellsReference();
            }

            if (cells != null && row >= 0 && row < rows && column >= 0 && column < columns)
            {
                return cells[row, column];
            }
            return null;
        }

        /// <summary>
        /// Tìm lại các liên kết ô cờ đang có sẵn trong Scene để điền lại mảng cells.
        /// </summary>
        private void RebuildCellsReference()
        {
            Transform parent = boardParent != null ? boardParent : transform;
            BoardCell[] foundCells = parent.GetComponentsInChildren<BoardCell>();

            cells = new BoardCell[rows, columns];
            int count = 0;
            foreach (var cell in foundCells)
            {
                if (cell.row >= 0 && cell.row < rows && cell.column >= 0 && cell.column < columns)
                {
                    cells[cell.row, cell.column] = cell;
                    cell.Init(cell.row, cell.column, this);
                    count++;
                }
            }
            Debug.Log($"[BoardManager] Đã tìm thấy và liên kết {count} ô cờ có sẵn từ Scene.");
        }
    }
}
