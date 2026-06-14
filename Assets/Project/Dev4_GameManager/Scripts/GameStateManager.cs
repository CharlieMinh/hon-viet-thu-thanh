using System;
using UnityEngine;
using HonVietThuThanh.Shared;

namespace HonVietThuThanh.Dev4
{
    /// <summary>Các trạng thái có thể có của game.</summary>
    public enum GameState
    {
        Preparation,    // Giai đoạn đặt hero, chưa bắt đầu wave
        WaveInProgress, // Đang trong wave, enemy đang spawn và di chuyển
        WaveComplete,   // Wave vừa xong, chuẩn bị wave tiếp
        Win,            // Qua hết 3 wave → THẮNG
        Lose            // Thành hết máu → THUA
    }

    /// <summary>
    /// GameStateManager — máy trạng thái trung tâm của game.
    ///
    /// LẮNG NGHE:
    ///   GameEvents.OnWaveStarted   → chuyển sang WaveInProgress
    ///   GameEvents.OnWaveCompleted → nếu đủ 3 wave → Win, ngược lại → WaveComplete
    ///   BaseHealthManager.OnBaseDestroyed → chuyển sang Lose
    ///
    /// PHÁT RA:
    ///   OnGameStateChanged(GameState) → UIManager hiện/ẩn panel, button
    ///
    /// SETUP trong Inspector:
    ///   - totalWaves = 3
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        [Header("Cấu hình")]
        [Tooltip("Số wave cần qua để thắng")]
        [SerializeField] private int totalWaves = 3;

        public GameState CurrentState { get; private set; } = GameState.Preparation;

        /// <summary>Phát mỗi khi trạng thái game thay đổi.</summary>
        public static event Action<GameState> OnGameStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            GameEvents.OnWaveStarted         += HandleWaveStarted;
            GameEvents.OnWaveCompleted       += HandleWaveCompleted;
            BaseHealthManager.OnBaseDestroyed += HandleBaseDestroyed;
        }

        private void OnDisable()
        {
            GameEvents.OnWaveStarted         -= HandleWaveStarted;
            GameEvents.OnWaveCompleted       -= HandleWaveCompleted;
            BaseHealthManager.OnBaseDestroyed -= HandleBaseDestroyed;
        }

        // --- Handlers ---

        private void HandleWaveStarted(int waveIndex)
        {
            SetState(GameState.WaveInProgress);
        }

        private void HandleWaveCompleted(int waveIndex)
        {
            // waveIndex 0-based: wave 1=0, wave 2=1, wave 3=2
            bool isLastWave = (waveIndex >= totalWaves - 1);
            SetState(isLastWave ? GameState.Win : GameState.WaveComplete);
        }

        private void HandleBaseDestroyed()
        {
            // Lose có thể xảy ra bất kỳ lúc nào, kể cả giữa wave
            SetState(GameState.Lose);
        }

        // --- Core ---

        private void SetState(GameState newState)
        {
            if (CurrentState == newState) return;
            CurrentState = newState;
            Debug.Log($"[GameStateManager] State → {newState}");
            OnGameStateChanged?.Invoke(newState);
        }
    }
}
