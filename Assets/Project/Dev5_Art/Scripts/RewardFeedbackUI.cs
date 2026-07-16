using UnityEngine;
using TMPro;
using System.Collections;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Component quản lý hiển thị thông báo nhận Vàng (Kill Reward hoặc Interest) lên màn hình (Phase 13).
    /// </summary>
    public class RewardFeedbackUI : MonoBehaviour
    {
        public static RewardFeedbackUI Instance { get; private set; }

        [Header("UI Reference")]
        public TMP_Text rewardText;
        public float displayDuration = 2.5f;

        private Coroutine clearCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[RewardFeedbackUI] Phát hiện instance trùng lặp, tự huỷ.");
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (rewardText != null)
            {
                rewardText.text = "";
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Kích hoạt hiển thị lượng Vàng nhận được kèm theo nguồn.
        /// </summary>
        public void ShowGoldReward(int amount, string reason)
        {
            if (rewardText == null) return;

            string message = "";
            if (reason.Equals("Enemy Kill", System.StringComparison.OrdinalIgnoreCase))
            {
                message = $"+{amount} Enemy Kill";
                rewardText.color = new Color(1.0f, 0.82f, 0f, 1f); // Màu vàng tươi sáng cho kill reward
            }
            else if (reason.Equals("Interest", System.StringComparison.OrdinalIgnoreCase))
            {
                message = $"Interest +{amount}G";
                rewardText.color = new Color(0f, 0.85f, 1.0f, 1f); // Màu xanh Cyan cho lợi tức
            }
            else
            {
                message = $"+{amount}G ({reason})";
                rewardText.color = Color.white;
            }

            rewardText.text = message;

            // Chạy Coroutine tự động xoá thông báo sau khoảng thời gian chỉ định
            if (clearCoroutine != null)
            {
                StopCoroutine(clearCoroutine);
            }
            clearCoroutine = StartCoroutine(ClearTextAfterDelay());
        }

        private IEnumerator ClearTextAfterDelay()
        {
            yield return new WaitForSeconds(displayDuration);
            if (rewardText != null)
            {
                rewardText.text = "";
            }
        }
    }
}
