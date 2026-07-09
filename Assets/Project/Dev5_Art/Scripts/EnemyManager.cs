using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Quản lý danh sách các enemy còn sống trong scene.
    /// Cung cấp debug bằng phím T để gây sát thương lên enemy đầu tiên.
    /// </summary>
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance { get; private set; }

        [Header("Danh sách theo dõi")]
        [SerializeField] private List<EnemyController> activeEnemies = new List<EnemyController>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[EnemyManager] Phát hiện instance trùng lặp, tự huỷ bỏ.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            // Debug test: Nhấn phím T để gây 10 sát thương lên enemy đầu tiên còn sống (Chỉ khi debugMode và enableDebugHotkeys bật)
            if (GameConfig.Instance != null && GameConfig.Instance.debugMode && GameConfig.Instance.enableDebugHotkeys)
            {
                if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
                {
                    DamageFirstEnemyForDebug();
                }
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
        /// Đăng ký enemy vào danh sách quản lý.
        /// </summary>
        public void RegisterEnemy(EnemyController enemy)
        {
            if (enemy == null) return;

            if (!activeEnemies.Contains(enemy))
            {
                activeEnemies.Add(enemy);
                Debug.Log($"[EnemyManager] Đã đăng ký enemy: {enemy.gameObject.name}. Tổng số enemy: {activeEnemies.Count}");
            }
        }

        /// <summary>
        /// Huỷ đăng ký enemy khỏi danh sách quản lý (gọi khi enemy chết hoặc bị huỷ).
        /// </summary>
        public void UnregisterEnemy(EnemyController enemy)
        {
            if (enemy == null) return;

            if (activeEnemies.Contains(enemy))
            {
                activeEnemies.Remove(enemy);
                Debug.Log($"[EnemyManager] Đã hủy đăng ký enemy: {enemy.gameObject.name}. Số enemy còn sống: {activeEnemies.Count}");

                if (activeEnemies.Count == 0)
                {
                    if (WaveManager.Instance != null)
                    {
                        WaveManager.Instance.CheckWaveCompletion();
                    }
                    else
                    {
                        Debug.Log("[EnemyManager] All enemies defeated");

                        // Báo cáo về GamePhaseManager để kết thúc đợt quái (Phase 6)
                        if (GamePhaseManager.Instance != null && GamePhaseManager.Instance.IsCombatPhase)
                        {
                            GamePhaseManager.Instance.CompleteWave();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Lấy enemy đầu tiên trong danh sách còn sống.
        /// </summary>
        public EnemyController GetFirstAliveEnemy()
        {
            if (activeEnemies.Count > 0)
            {
                return activeEnemies[0];
            }
            return null;
        }

        /// <summary>
        /// Tìm enemy còn sống có vị trí gần với toạ độ truyền vào nhất.
        /// </summary>
        public EnemyController GetNearestEnemy(Vector3 position)
        {
            EnemyController nearest = null;
            float minDistance = float.MaxValue;

            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = activeEnemies[i];
                if (enemy == null || enemy.Health == null || enemy.Health.IsDead)
                {
                    continue;
                }

                float dist = Vector3.Distance(position, enemy.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Lấy số lượng enemy còn sống.
        /// </summary>
        public int GetAliveEnemyCount()
        {
            return activeEnemies.Count;
        }

        /// <summary>
        /// Gây 10 damage lên enemy đầu tiên còn sống để phục vụ debug.
        /// </summary>
        public void DamageFirstEnemyForDebug()
        {
            EnemyController target = GetFirstAliveEnemy();
            if (target != null)
            {
                Debug.Log($"[EnemyManager] Debug Key T: Gây 10 sát thương lên '{target.gameObject.name}'");
                target.Health.TakeDamage(10);
            }
            else
            {
                Debug.Log("[EnemyManager] Không còn enemy nào sống để gây sát thương.");
            }
        }
    }
}
