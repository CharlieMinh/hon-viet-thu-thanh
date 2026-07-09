using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Cấu hình gameplay cơ bản và thiết lập môi trường chơi thử (Phase 17).
    /// </summary>
    public class GameConfig : MonoBehaviour
    {
        public static GameConfig Instance { get; private set; }

        [Header("Chế độ Debug")]
        [Tooltip("Bật chế độ Debug. Khi bật sẽ sử dụng vàng debug và kích hoạt các tính năng debug.")]
        public bool debugMode = false;

        [Header("Kinh tế (Gold)")]
        [Tooltip("Số vàng bắt đầu ở chế độ thường (Normal)")]
        public int startingGoldNormal = 10;

        [Tooltip("Số vàng bắt đầu ở chế độ Debug")]
        public int startingGoldDebug = 1000;

        [Header("Tùy chọn Debug")]
        [Tooltip("Bật phím nóng Debug (ví dụ: T để gây sát thương enemy)")]
        public bool enableDebugHotkeys = true;

        [Tooltip("Bật các công cụ tự động thiết lập trong Editor")]
        public bool enableAutoSetupTools = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[GameConfig] Phát hiện instance trùng lặp, tự huỷ.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
    }
}
