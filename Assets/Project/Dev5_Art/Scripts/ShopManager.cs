using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace HonVietThuThanh.Dev5
{
    [System.Serializable]
    public struct ShopUnitData
    {
        public string unitName;
        public int price;
        public GameObject unitPrefab;
    }

    /// <summary>
    /// Quản lý việc mua quân từ shop và spawn quân vào Bench ở Phase 3.
    /// </summary>
    public class ShopManager : MonoBehaviour
    {
        [Header("Danh sách hàng hóa")]
        public List<ShopUnitData> shopUnits = new List<ShopUnitData>();

        [Header("Bench Setup")]
        [Tooltip("Các vị trí chờ (Bench) của người chơi")]
        public Transform[] benchPoints;

        [Header("Tham chiếu")]
        [Tooltip("Transform parent chứa toàn bộ unit đã mua (PlayerUnits)")]
        public Transform playerUnitsParent;

        [Header("Tham chiếu UI Buttons")]
        public UnityEngine.UI.Button buyKnightButton;
        public UnityEngine.UI.Button buyArcherButton;
        public UnityEngine.UI.Button buyTankButton;

        [Header("Tham chiếu UI")]
        [Tooltip("Text hiển thị lỗi (ví dụ: Not enough Gold, Bench is full)")]
        public TMP_Text messageText;
        [Tooltip("Thời gian hiển thị thông báo lỗi")]
        public float messageDuration = 3f;

        private Coroutine messageCoroutine;

        private void Start()
        {
            if (buyKnightButton != null) buyKnightButton.onClick.AddListener(BuyKnight);
            if (buyArcherButton != null) buyArcherButton.onClick.AddListener(BuyArcher);
            if (buyTankButton != null) buyTankButton.onClick.AddListener(BuyTank);

            if (playerUnitsParent == null)
            {
                GameObject parentGo = GameObject.Find("PlayerUnits");
                if (parentGo != null)
                {
                    playerUnitsParent = parentGo.transform;
                }
            }

            if (messageText != null)
            {
                messageText.text = "";
            }
        }

        /// <summary>
        /// Mua Knight (3 Gold)
        /// </summary>
        public void BuyKnight()
        {
            BuyUnitByName("Knight");
        }

        /// <summary>
        /// Mua Archer (4 Gold)
        /// </summary>
        public void BuyArcher()
        {
            BuyUnitByName("Archer");
        }

        /// <summary>
        /// Mua Tank (5 Gold)
        /// </summary>
        public void BuyTank()
        {
            BuyUnitByName("Tank");
        }

        /// <summary>
        /// Hàm mua unit theo tên
        /// </summary>
        private void BuyUnitByName(string name)
        {
            // Kiểm tra trạng thái game trước khi mua (Phase 6)
            if (GamePhaseManager.Instance != null && !GamePhaseManager.Instance.IsPreparationPhase)
            {
                ShowErrorMessage("Cannot buy units during Combat");
                return;
            }

            // Tìm thông tin mặt hàng
            ShopUnitData unitData = shopUnits.Find(x => x.unitName.Equals(name, System.StringComparison.OrdinalIgnoreCase));
            if (unitData.unitPrefab == null)
            {
                Debug.LogError($"[ShopManager] Prefab cho unit '{name}' chưa được cấu hình hoặc bị thiếu!");
                return;
            }

            // Kiểm tra EconomyManager
            if (EconomyManager.Instance == null)
            {
                Debug.LogError("[ShopManager] EconomyManager.Instance is null!");
                return;
            }

            // Kiểm tra tiền
            if (!EconomyManager.Instance.CanSpendGold(unitData.price))
            {
                ShowErrorMessage("Not enough Gold");
                return;
            }

            // Kiểm tra ô chờ trống
            Transform spawnPoint = GetFirstEmptyBenchPoint();
            if (spawnPoint == null)
            {
                ShowErrorMessage("Bench is full");
                return;
            }

            // Thực hiện trừ tiền và sinh quân cờ
            if (EconomyManager.Instance.SpendGold(unitData.price))
            {
                GameObject spawnedUnit = Instantiate(unitData.unitPrefab, spawnPoint.position, Quaternion.identity);
                
                if (playerUnitsParent != null)
                {
                    spawnedUnit.transform.SetParent(playerUnitsParent);
                }

                // Thiết lập tên trong PlaceableUnit nếu có
                PlaceableUnit pu = spawnedUnit.GetComponent<PlaceableUnit>();
                if (pu != null)
                {
                    pu.unitName = unitData.unitName;
                }

                // Thiết lập dữ liệu sao ban đầu (Phase 12)
                UnitStarData starData = spawnedUnit.GetComponent<UnitStarData>();
                if (starData == null)
                {
                    starData = spawnedUnit.AddComponent<UnitStarData>();
                }
                starData.unitId = unitData.unitName;
                starData.starLevel = 1;

                // Đồng bộ chỉ số máu/damage ban đầu của quân cờ
                UnitCombatStats stats = spawnedUnit.GetComponent<UnitCombatStats>();
                if (stats != null)
                {
                    stats.ApplyStarMultiplier(1);
                }

                // Thêm UnitStarVisual nếu chưa có để hiển thị (Phase 12)
                UnitStarVisual visual = spawnedUnit.GetComponent<UnitStarVisual>();
                if (visual == null)
                {
                    visual = spawnedUnit.AddComponent<UnitStarVisual>();
                }
                visual.RefreshVisual();

                Debug.Log($"[ShopManager] Đã mua thành công {unitData.unitName} với giá {unitData.price}G. Gold còn lại: {EconomyManager.Instance.CurrentGold}");

                // Tự động kích hoạt gộp sao nếu đang ở Phase Preparation hoặc WaveCompleted (Phase 12)
                if (UnitStarUpgradeManager.Instance != null)
                {
                    UnitStarUpgradeManager.Instance.TryUpgradeAllUnits();
                }
            }
        }

        /// <summary>
        /// Tìm điểm Bench trống đầu tiên bằng cách kiểm tra khoảng cách đến các quân cờ hiện tại.
        /// </summary>
        private Transform GetFirstEmptyBenchPoint()
        {
            foreach (var point in benchPoints)
            {
                if (point == null) continue;
                if (!IsUnitAtPosition(point.position))
                {
                    return point;
                }
            }
            return null;
        }

        /// <summary>
        /// Kiểm tra xem tại vị trí này có quân cờ nào đang đứng hay không.
        /// </summary>
        private bool IsUnitAtPosition(Vector3 position)
        {
            // Tìm tất cả các PlaceableUnit trong scene
            PlaceableUnit[] units = Object.FindObjectsByType<PlaceableUnit>(FindObjectsSortMode.None);
            foreach (var unit in units)
            {
                // Nếu unit đang nằm trên board (OccupiedCell != null) thì bỏ qua
                if (unit.OccupiedCell != null) continue;

                // Nếu unit đang ở rất gần vị trí Bench point (trong phạm vi 0.5f) thì coi như ô đó đã có quân
                if (Vector3.Distance(unit.transform.position, position) < 0.5f)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Hiển thị thông báo lỗi lên màn hình và console.
        /// </summary>
        private void ShowErrorMessage(string message)
        {
            Debug.LogWarning($"[ShopManager] {message}");
            if (messageText != null)
            {
                messageText.text = message;
                if (messageCoroutine != null)
                {
                    StopCoroutine(messageCoroutine);
                }
                messageCoroutine = StartCoroutine(ClearMessageAfterDelay());
            }
        }

        private System.Collections.IEnumerator ClearMessageAfterDelay()
        {
            yield return new WaitForSeconds(messageDuration);
            if (messageText != null)
            {
                messageText.text = "";
            }
        }
        /// <summary>
        /// Bật/Tắt các nút mua của Shop dựa trên trạng thái (Phase 6).
        /// </summary>
        public void SetShopButtonsInteractable(bool interactable)
        {
            if (buyKnightButton != null) buyKnightButton.interactable = interactable;
            if (buyArcherButton != null) buyArcherButton.interactable = interactable;
            if (buyTankButton != null) buyTankButton.interactable = interactable;
        }
    }
}
