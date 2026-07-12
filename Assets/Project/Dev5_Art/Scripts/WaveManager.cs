using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    [System.Serializable]
    public class WaveEnemyEntry
    {
        public string enemyName;
        public GameObject enemyPrefab;
        public int count;
        public float spawnInterval;
        public int maxHealth;
        public int damage;
        public float attackRange;
        public float attackCooldown;
        public float moveSpeed;
        public int killGoldReward;
    }

    [System.Serializable]
    public class WaveData
    {
        public string waveName;
        public List<WaveEnemyEntry> enemies = new List<WaveEnemyEntry>();
    }

    /// <summary>
    /// Singleton quản lý các đợt quái (Phase 8).
    /// Sinh quái tự động khi Combat bắt đầu và báo cáo hoàn thành khi toàn bộ quái bị tiêu diệt.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        [Header("Cấu hình Wave")]
        public int currentWaveIndex = 0;
        [Tooltip("Prefab quái mặc định (Legacy)")]
        public GameObject enemyPrefab;
        public Transform[] spawnPoints;
        public Transform enemiesParent;
        public List<WaveData> waves = new List<WaveData>();

        [Header("Trạng thái Runtime")]
        [SerializeField] private bool isSpawning = false;

        public bool IsSpawning => isSpawning;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[WaveManager] Phát hiện instance trùng lặp, tự huỷ.");
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
        /// Bắt đầu đợt quái hiện tại.
        /// </summary>
        public void StartCurrentWave()
        {
            if (isSpawning)
            {
                Debug.LogWarning("[WaveManager] Đang sinh quái, không thể bắt đầu wave mới!");
                return;
            }

            if (currentWaveIndex < 0 || currentWaveIndex >= waves.Count)
            {
                Debug.LogWarning($"[WaveManager] Wave index {currentWaveIndex} vượt quá số lượng cấu hình!");
                return;
            }

            // Reset tracker trước mỗi wave mới để xóa ref rác từ wave trước
            AnimationPendingTracker.Instance?.ResetAll();

            StartCoroutine(SpawnWaveRoutine());
        }

        /// <summary>
        /// Coroutine sinh quái lần lượt hỗ trợ Wave Composition.
        /// </summary>
        private IEnumerator SpawnWaveRoutine()
        {
            isSpawning = true;
            WaveData wave = waves[currentWaveIndex];
            Debug.Log($"[WaveManager] Bắt đầu đợt quái: {wave.waveName}");

            int spawnedCount = 0;
            foreach (var entry in wave.enemies)
            {
                if (entry == null) continue;
                
                GameObject activePrefab = entry.enemyPrefab != null ? entry.enemyPrefab : enemyPrefab;
                if (activePrefab == null)
                {
                    Debug.LogError($"[WaveManager] Không tìm thấy Prefab cho quái '{entry.enemyName}'!");
                    continue;
                }

                for (int i = 0; i < entry.count; i++)
                {
                    // Nếu game không còn trong phase Combat (ví dụ người chơi bị thua), dừng spawn ngay lập tức
                    if (GamePhaseManager.Instance != null && !GamePhaseManager.Instance.IsCombatPhase)
                    {
                        Debug.Log("[WaveManager] Trận đấu kết thúc hoặc trạng thái thay đổi. Dừng spawn quái.");
                        isSpawning = false;
                        yield break;
                    }

                    if (spawnPoints == null || spawnPoints.Length == 0)
                    {
                        Debug.LogError("[WaveManager] Spawn points trống!");
                        yield break;
                    }

                    // Lần lượt chọn điểm sinh quái trong danh sách
                    Transform spawnPoint = spawnPoints[spawnedCount % spawnPoints.Length];
                    Vector3 pos = spawnPoint.position;
                    Quaternion rot = spawnPoint.rotation;

                    // Sinh quái
                    GameObject enemyGO = Instantiate(activePrefab, pos, rot);
                    if (enemiesParent != null)
                    {
                        enemyGO.transform.SetParent(enemiesParent);
                    }

                    enemyGO.name = $"{wave.waveName}_{entry.enemyName}_{i}";

                    // Thiết lập chỉ số máu cho quái theo wave
                    Health hp = enemyGO.GetComponent<Health>();
                    if (hp != null)
                    {
                        hp.InitializeHealth(entry.maxHealth);
                    }

                    // Thiết lập chỉ số chiến đấu
                    EnemyCombatStats stats = enemyGO.GetComponent<EnemyCombatStats>();
                    if (stats != null)
                    {
                        stats.InitializeStats(entry.damage, entry.attackRange, entry.attackCooldown, entry.moveSpeed, 10f);
                    }

                    // Thiết lập lượng vàng nhận được khi quái bị hạ gục
                    EnemyController enemyCtrl = enemyGO.GetComponent<EnemyController>();
                    if (enemyCtrl != null)
                    {
                        enemyCtrl.enemyName = entry.enemyName;
                        enemyCtrl.killGoldReward = entry.killGoldReward;
                    }

                    spawnedCount++;
                    yield return new WaitForSeconds(entry.spawnInterval);
                }
            }

            isSpawning = false;
            Debug.Log($"[WaveManager] Sinh xong toàn bộ quái đợt: {wave.waveName}.");

            // Kiểm tra xem quái có bị chết hết trong quá trình sinh hay không
            CheckWaveCompletion();
        }

        /// <summary>
        /// Kiểm tra xem đợt quái đã hoàn thành chưa (được gọi từ EnemyManager khi quái chết).
        /// </summary>
        public void CheckWaveCompletion()
        {
            if (isSpawning) return; // Chưa sinh hết quái, chưa tính là hoàn thành

            if (EnemyManager.Instance != null && EnemyManager.Instance.GetAliveEnemyCount() == 0)
            {
                OnWaveCompleted();
            }
        }

        /// <summary>
        /// Xử lý khi hoàn thành toàn bộ quái của đợt hiện tại.
        /// </summary>
        private void OnWaveCompleted()
        {
            Debug.Log($"[WaveManager] Đã hoàn thành đợt quái: {waves[currentWaveIndex].waveName}");

            // Nếu còn animation Death đang chạy dở → đợi rồi mới chuyển round
            if (AnimationPendingTracker.Instance != null && AnimationPendingTracker.Instance.IsAnyPending)
            {
                Debug.Log($"[WaveManager] Đang chờ {AnimationPendingTracker.Instance.PendingCount} animation(s) kết thúc trước khi chuyển round...");
                StartCoroutine(WaitForAnimationsThenComplete());
            }
            else
            {
                if (GamePhaseManager.Instance != null)
                {
                    GamePhaseManager.Instance.CompleteWave();
                }
            }
        }

        /// <summary>
        /// Coroutine: đợi cho đến khi tất cả Death animation kết thúc rồi mới gọi CompleteWave().
        /// </summary>
        private IEnumerator WaitForAnimationsThenComplete()
        {
            while (AnimationPendingTracker.Instance != null && AnimationPendingTracker.Instance.IsAnyPending)
            {
                yield return new WaitForSeconds(0.1f);
            }

            Debug.Log("[WaveManager] Tất cả animation đã kết thúc. Chuyển round mới.");
            if (GamePhaseManager.Instance != null)
            {
                GamePhaseManager.Instance.CompleteWave();
            }
        }

        /// <summary>
        /// Kiểm tra xem còn đợt quái nào tiếp theo không.
        /// </summary>
        public bool HasMoreWaves()
        {
            return currentWaveIndex + 1 < waves.Count;
        }

        /// <summary>
        /// Chuyển sang đợt quái tiếp theo.
        /// </summary>
        public void AdvanceToNextWave()
        {
            if (HasMoreWaves())
            {
                currentWaveIndex++;
                Debug.Log($"[WaveManager] Chuyển tiếp sang wave index: {currentWaveIndex}");
                if (GamePhaseManager.Instance != null)
                {
                    GamePhaseManager.Instance.UpdateStateUI();
                }
            }
        }
    }
}
