using UnityEngine;
using TMPro;

namespace HonVietThuThanh.Dev5
{
    public enum GameState
    {
        Preparation,
        Combat,
        WaveCompleted,
        Win,
        Lose
    }

    /// <summary>
    /// Singleton quản lý các trạng thái hoạt động của trò chơi (Phase 6).
    /// </summary>
    public class GamePhaseManager : MonoBehaviour
    {
        public static GamePhaseManager Instance { get; private set; }

        [Header("Trạng thái hiện tại")]
        [SerializeField] private GameState currentState = GameState.Preparation;

        private bool currentCombatRewardGranted = false;

        [Header("Tham chiếu UI")]
        [Tooltip("Text hiển thị trạng thái hiện tại (TextMeshPro)")]
        public TMP_Text stateText;

        [Tooltip("Text hiển thị wave hiện tại (TextMeshPro)")]
        public TMP_Text waveText;

        public GameState CurrentState => currentState;

        public bool IsPreparationPhase => currentState == GameState.Preparation || currentState == GameState.WaveCompleted;
        public bool IsCombatPhase => currentState == GameState.Combat;

        /// <summary>
        /// Kích hoạt khi GameState thay đổi.
        /// </summary>
        public event System.Action<GameState> OnGameStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[GamePhaseManager] Phát hiện instance trùng lặp, tự huỷ.");
                Destroy(gameObject);
                return;
            }
            Instance = this;

            VictoryScreenController.EnsureExists();
        }

        private void Start()
        {
            SetState(GameState.Preparation);
        }

        /// <summary>
        /// Chuyển đổi trạng thái game và thực hiện cập nhật các kịch bản liên quan.
        /// </summary>
        public void SetState(GameState newState)
        {
            currentState = newState;
            Debug.Log($"[GamePhaseManager] Trạng thái game chuyển sang: {newState}");

            UpdateStateUI();

            // Khóa/Mở các nút mua của Shop dựa trên trạng thái
            ShopManager shop = Object.FindAnyObjectByType<ShopManager>();
            if (shop != null)
            {
                shop.SetShopButtonsInteractable(IsPreparationPhase);
            }

            OnGameStateChanged?.Invoke(newState);
        }

        /// <summary>
        /// Bắt đầu giai đoạn chuẩn bị cho wave kế tiếp.
        /// </summary>
        public void StartPreparation()
        {
            SetState(GameState.Preparation);
        }

        /// <summary>
        /// Bắt đầu chiến đấu (khi người chơi nhấn nút Start Battle).
        /// </summary>
        public void StartCombat()
        {
            if (currentState != GameState.Preparation && currentState != GameState.WaveCompleted)
            {
                Debug.LogWarning("[GamePhaseManager] Chỉ có thể bắt đầu trận đấu từ trạng thái Preparation hoặc WaveCompleted!");
                return;
            }

            // Yêu cầu ít nhất 1 cờ trên board
            if (PlayerUnitManager.Instance != null && !PlayerUnitManager.Instance.HasAlivePlacedUnits())
            {
                Debug.LogWarning("[GamePhaseManager] Place at least one unit before battle!");
                return;
            }

            // Lưu trữ snapshot đội hình cờ trên bàn trước khi vào Combat (Phase 10)
            if (BattleResetManager.Instance != null)
            {
                BattleResetManager.Instance.CaptureCurrentBoardFormation();
            }

            currentCombatRewardGranted = false;

            SetState(GameState.Combat);

            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.StartCurrentWave();
            }
        }

        /// <summary>
        /// Thất bại trận đấu (gọi khi toàn bộ cờ của người chơi trên board bị tiêu diệt).
        /// </summary>
        public void LoseGame()
        {
            if (currentState != GameState.Combat)
            {
                return;
            }

            SetState(GameState.Lose);
            Debug.Log("[GamePhaseManager] Game Over - Defeat!");
        }

        /// <summary>
        /// Hoàn thành đợt quái (gọi khi EnemyManager báo hết quái).
        /// </summary>
        public void CompleteWave()
        {
            if (currentState != GameState.Combat)
            {
                return; // Tránh gọi trùng lặp nếu trạng thái không phải Combat
            }

            // Cộng lợi tức Vàng sau combat nếu chưa được cộng (Phase 11)
            if (!currentCombatRewardGranted)
            {
                currentCombatRewardGranted = true;
                if (EconomyManager.Instance != null)
                {
                    EconomyManager.Instance.GrantInterestGold();
                }
            }

            if (WaveManager.Instance != null)
            {
                if (WaveManager.Instance.HasMoreWaves())
                {
                    SetState(GameState.WaveCompleted);
                    Debug.Log("[GamePhaseManager] Wave Completed!");

                    // Trả cờ còn sống về vị trí gốc sau wave (Phase 10)
                    if (BattleResetManager.Instance != null)
                    {
                        BattleResetManager.Instance.ReturnSurvivingUnitsToFormation();
                    }

                    WaveManager.Instance.AdvanceToNextWave();
                }
                else
                {
                    SetState(GameState.Win);
                    Debug.Log("[GamePhaseManager] All waves completed - Victory");

                    // Trả cờ còn sống về vị trí gốc khi thắng game (Phase 10)
                    if (BattleResetManager.Instance != null)
                    {
                        BattleResetManager.Instance.ReturnSurvivingUnitsToFormation();
                    }
                }
            }
            else
            {
                SetState(GameState.WaveCompleted);
                Debug.Log("[GamePhaseManager] Wave Completed!");

                // Trả cờ còn sống về vị trí gốc sau wave (Phase 10)
                if (BattleResetManager.Instance != null)
                {
                    BattleResetManager.Instance.ReturnSurvivingUnitsToFormation();
                }
            }
        }

        public void UpdateStateUI()
        {
            string waveStatus = "";
            if (currentState == GameState.Win)
            {
                waveStatus = "Victory";
            }
            else if (currentState == GameState.Lose)
            {
                waveStatus = "Defeat";
            }
            else if (currentState == GameState.WaveCompleted)
            {
                waveStatus = "Wave Completed";
            }
            else
            {
                if (WaveManager.Instance != null)
                {
                    waveStatus = $"Wave: {WaveManager.Instance.currentWaveIndex + 1} / {WaveManager.Instance.waves.Count}";
                }
                else
                {
                    waveStatus = $"State: {currentState}";
                }
            }

            if (waveText != null)
            {
                waveText.text = waveStatus;
            }

            if (stateText != null)
            {
                if (waveText == null)
                {
                    stateText.text = waveStatus;
                }
                else
                {
                    stateText.text = $"State: {currentState}";
                }
            }
        }
    }
}
