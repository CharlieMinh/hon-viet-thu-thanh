using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Singleton theo dõi số lượng Death animation đang chạy dở.
    /// Các animation controller gọi Register() khi bắt đầu Death,
    /// Unregister() khi animation kết thúc và object sắp bị Destroy.
    /// WaveManager đợi PendingCount về 0 trước khi gọi CompleteWave().
    /// </summary>
    public class AnimationPendingTracker : MonoBehaviour
    {
        public static AnimationPendingTracker Instance { get; private set; }

        private int pendingCount = 0;

        /// <summary>Số Death animation đang chờ kết thúc.</summary>
        public int PendingCount => pendingCount;

        /// <summary>True nếu còn ít nhất 1 animation đang chạy.</summary>
        public bool IsAnyPending => pendingCount > 0;

        /// <summary>
        /// Đảm bảo Instance tồn tại. Tự tạo nếu chưa có trong scene.
        /// Gọi từ GamePhaseManager.Awake() để bootstrap sớm.
        /// </summary>
        public static AnimationPendingTracker EnsureExists()
        {
            if (Instance == null)
            {
                GameObject go = new GameObject("[AnimationPendingTracker]");
                go.AddComponent<AnimationPendingTracker>();
                Debug.Log("[AnimationPendingTracker] Auto-created instance.");
            }
            return Instance;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
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
        /// Gọi khi một Death animation bắt đầu chạy.
        /// </summary>
        public void Register()
        {
            pendingCount++;
            Debug.Log($"[AnimationPendingTracker] Register → pending = {pendingCount}");
        }

        /// <summary>
        /// Gọi khi một Death animation đã kết thúc và object sắp Destroy.
        /// </summary>
        public void Unregister()
        {
            pendingCount = Mathf.Max(0, pendingCount - 1);
            Debug.Log($"[AnimationPendingTracker] Unregister → pending = {pendingCount}");
        }

        /// <summary>
        /// Reset toàn bộ counter — gọi khi bắt đầu Combat phase mới
        /// để xóa bất kỳ ref rác nào còn sót.
        /// </summary>
        public void ResetAll()
        {
            if (pendingCount != 0)
            {
                Debug.LogWarning($"[AnimationPendingTracker] ResetAll: xóa {pendingCount} pending animation(s) còn sót.");
            }
            pendingCount = 0;
        }
    }
}
