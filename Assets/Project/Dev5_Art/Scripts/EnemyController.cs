using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Component quản lý định danh và trạng thái của Enemy.
    /// Tự động đăng ký và hủy đăng ký với EnemyManager.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Thông tin Enemy")]
        public string enemyName = "Enemy_Test";

        [Header("Kinh tế - Phase 11")]
        [Tooltip("Số Vàng người chơi nhận được khi tiêu diệt quái này")]
        public int killGoldReward = 2;

        private bool rewardGranted = false;

        private Health health;
        private EnemyAnimationController animationController;
        public Health Health => health;

        private void Awake()
        {
            health = GetComponent<Health>();
            animationController = GetComponent<EnemyAnimationController>();
        }

        private void Start()
        {
            // Tự đăng ký với EnemyManager khi bắt đầu
            if (EnemyManager.Instance != null)
            {
                EnemyManager.Instance.RegisterEnemy(this);
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] Không tìm thấy EnemyManager.Instance để đăng ký!");
            }

            // Lắng nghe sự kiện chết
            health.OnDeath += HandleDeath;
        }

        private void HandleDeath()
        {
               if (animationController != null)
            {
                animationController.PlayDeath();
            }

            if (!rewardGranted)
            {
                rewardGranted = true;
                RoundResultTracker.RecordEnemyKill(killGoldReward);

                if (EconomyManager.Instance != null)
                {
                    EconomyManager.Instance.AddGold(killGoldReward);
                    Debug.Log($"[EnemyController] {enemyName} killed. Reward +{killGoldReward} Gold.");

                    // Gọi UI hiển thị nhận Vàng hạ gục (Phase 13)
                    if (RewardFeedbackUI.Instance != null)
                    {
                        RewardFeedbackUI.Instance.ShowGoldReward(killGoldReward, "Enemy Kill");
                    }
                }
                else
                {
                    Debug.LogWarning("[EnemyController] Không tìm thấy EconomyManager.Instance để cộng Gold!");
                }
            }

            // Tự huỷ đăng ký khi chết
            if (EnemyManager.Instance != null)
            {
                EnemyManager.Instance.UnregisterEnemy(this);
            }
        }

        private void OnDestroy()
        {
            // Bảo đảm huỷ liên kết sự kiện và huỷ đăng ký nếu bị xoá bằng cách khác
            if (health != null)
            {
                health.OnDeath -= HandleDeath;
            }
            
            if (EnemyManager.Instance != null)
            {
                EnemyManager.Instance.UnregisterEnemy(this);
            }
        }
    }
}
