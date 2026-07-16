using UnityEngine;
using TMPro;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Quản lý hệ thống tiền tệ (Gold) đơn giản của người chơi ở Phase 3.
    /// </summary>
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        [Header("Cấu hình")]
        [Tooltip("Số Gold ban đầu của người chơi")]
        public int startingGold = 10;

        [Header("Tham chiếu UI")]
        [Tooltip("Text hiển thị số Gold hiện tại")]
        public TMP_Text goldText;

        public int CurrentGold => currentGold;

        private int currentGold;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[EconomyManager] Phát hiện instance trùng lặp, hủy bỏ bản mới.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            InitializeStartingGold();
        }

        private void Start()
        {
            InitializeStartingGold();
            UpdateGoldUI();
        }

        private void InitializeStartingGold()
        {
            if (GameConfig.Instance != null)
            {
                currentGold = GameConfig.Instance.debugMode ? GameConfig.Instance.startingGoldDebug : GameConfig.Instance.startingGoldNormal;
            }
            else
            {
                currentGold = startingGold;
            }
        }

        /// <summary>
        /// Kiểm tra người chơi có đủ Gold để mua hay không.
        /// </summary>
        public bool CanSpendGold(int amount)
        {
            return currentGold >= amount;
        }

        /// <summary>
        /// Trừ Gold của người chơi nếu đủ. Trả về true nếu thành công.
        /// </summary>
        public bool SpendGold(int amount)
        {
            if (CanSpendGold(amount))
            {
                currentGold -= amount;
                UpdateGoldUI();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Cộng thêm Gold cho người chơi.
        /// </summary>
        public void AddGold(int amount)
        {
            if (amount < 0) return;
            currentGold += amount;
            UpdateGoldUI();
        }

        /// <summary>
        /// Tính toán lượng vàng lợi tức dựa trên số vàng hiện tại (Phase 11).
        /// </summary>
        public int CalculateInterestGold()
        {
            return Mathf.Min(currentGold / 10, 2);
        }

        /// <summary>
        /// Cộng thêm vàng lợi tức và trả về lượng vàng đã cộng (Phase 11).
        /// </summary>
        public int GrantInterestGold()
        {
            int interest = CalculateInterestGold();
            if (interest > 0)
            {
                AddGold(interest);
                Debug.Log($"[EconomyManager] Interest reward: +{interest} Gold. Current Gold: {currentGold}");

                // Gọi UI hiển thị nhận Vàng lợi tức (Phase 13)
                if (RewardFeedbackUI.Instance != null)
                {
                    RewardFeedbackUI.Instance.ShowGoldReward(interest, "Interest");
                }
            }
            else
            {
                Debug.Log($"[EconomyManager] No interest granted. Current Gold: {currentGold}");
            }
            return interest;
        }

        /// <summary>
        /// Cập nhật hiển thị số Gold lên UI.
        /// </summary>
        public void UpdateGoldUI()
        {
            if (goldText != null)
            {
                goldText.text = $"Linh Khí: {currentGold}";
            }
        }
    }
}
