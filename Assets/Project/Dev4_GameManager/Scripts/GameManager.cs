using UnityEngine;
using HonVietThuThanh.Shared;

namespace HonVietThuThanh.Dev4
{
    /// <summary>
    /// GameManager — bộ điều phối trung tâm của Dev4.
    /// Chỉ chứa logic tổng hợp; mọi concern cụ thể đều delegate sang
    /// EconomyManager, BaseHealthManager, GameStateManager, UIManager.
    ///
    /// Setup trong Scene_Dev4_UI.unity:
    ///   1. Tạo GameObject "GameManager"
    ///   2. Gắn script này
    ///   3. Kéo các Manager reference vào Inspector
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Required Managers (kéo vào Inspector)")]
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private BaseHealthManager baseHealthManager;
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private UIManager uiManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            ValidateManagers();
        }

        private void ValidateManagers()
        {
            if (!economyManager)    Debug.LogError("[GameManager] EconomyManager chưa được gán!");
            if (!baseHealthManager) Debug.LogError("[GameManager] BaseHealthManager chưa được gán!");
            if (!gameStateManager)  Debug.LogError("[GameManager] GameStateManager chưa được gán!");
            if (!uiManager)         Debug.LogError("[GameManager] UIManager chưa được gán!");
        }

        private void OnEnable()
        {
            GameStateManager.OnGameStateChanged += LogStateChange;
        }

        private void OnDisable()
        {
            GameStateManager.OnGameStateChanged -= LogStateChange;
        }

        private void LogStateChange(GameState state)
        {
            Debug.Log($"[GameManager] Game state → {state}");
        }

        // Expose accessor để các script khác lấy nhanh
        public EconomyManager    Economy   => economyManager;
        public BaseHealthManager BaseHealth => baseHealthManager;
        public GameStateManager  GameState  => gameStateManager;
    }
}
