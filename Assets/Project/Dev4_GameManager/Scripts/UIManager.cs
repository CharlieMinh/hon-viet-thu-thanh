using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using HonVietThuThanh.Shared;

namespace HonVietThuThanh.Dev4
{
    /// <summary>
    /// UIManager — cập nhật toàn bộ HUD và panel dựa trên events.
    ///
    /// LẮNG NGHE:
    ///   EconomyManager.OnGoldChanged    → cập nhật text Linh Khí
    ///   BaseHealthManager.OnBaseHPChanged → cập nhật text + slider máu thành
    ///   GameEvents.OnWaveStarted        → cập nhật wave counter
    ///   GameStateManager.OnGameStateChanged → hiện Win/Lose panel, Start Wave button
    ///
    /// SETUP Canvas trong Unity:
    ///   Canvas (Screen Space - Overlay)
    ///   ├── TopBar/
    ///   │   ├── GoldText       (TMP)   → kéo vào goldText
    ///   │   ├── BaseHPText     (TMP)   → kéo vào baseHPText
    ///   │   ├── BaseHPSlider   (Slider)→ kéo vào baseHPSlider
    ///   │   └── WaveText       (TMP)   → kéo vào waveText
    ///   ├── StartWaveButton    (Button)→ kéo vào startWaveButton
    ///   ├── WinPanel           (Panel) → kéo vào winPanel
    ///   └── LosePanel          (Panel) → kéo vào losePanel
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Linh Khí (Gold)")]
        [SerializeField] private TextMeshProUGUI goldText;

        [Header("Sinh Mệnh Đền (Base HP)")]
        [SerializeField] private TextMeshProUGUI baseHPText;
        [SerializeField] private Slider           baseHPSlider;

        [Header("Wave Counter")]
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private int              totalWaves = 3;

        [Header("Buttons & Panels")]
        [SerializeField] private GameObject startWaveButton;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;

        private int _currentWaveDisplay = 0;

        private void Awake()
        {
            Debug.Log($"[UIManager] Awake on {gameObject.name}, parent = {(transform.parent ? transform.parent.name : "null")}", this);

            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[UIManager] Duplicate detected. Existing = {Instance.name}, New = {gameObject.name}. Removing only the duplicate component.", this);
                enabled = false;
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
            Debug.Log("[UIManager] OnEnable subscribe events", this);

            EconomyManager.OnGoldChanged += UpdateGoldUI;
            BaseHealthManager.OnBaseHPChanged += UpdateBaseHPUI;
            GameEvents.OnWaveStarted += UpdateWaveUI;
            GameStateManager.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            EconomyManager.OnGoldChanged        -= UpdateGoldUI;
            BaseHealthManager.OnBaseHPChanged   -= UpdateBaseHPUI;
            GameEvents.OnWaveStarted            -= UpdateWaveUI;
            GameStateManager.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void OnDestroy()
        {
            Debug.Log($"[UIManager] OnDestroy called on {gameObject.name}", this);

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            Debug.Log("[UIManager] Start called", this);

            SetActive(winPanel, false);
            SetActive(losePanel, false);
            SetActive(startWaveButton, true);

            int initGold = EconomyManager.Instance ? EconomyManager.Instance.CurrentGold : 150;
            int initHP = BaseHealthManager.Instance ? BaseHealthManager.Instance.CurrentBaseHP : 100;
            int maxHP = BaseHealthManager.Instance ? BaseHealthManager.Instance.MaxBaseHP : 100;

            UpdateGoldUI(initGold);
            UpdateBaseHPUI(initHP, maxHP);
            RefreshWaveText(0);
        }

        // --- UI Updaters ---

        private void UpdateGoldUI(int gold)
        {
            if (goldText) goldText.text = $"Linh Khi: {gold}";
        }

        private void UpdateBaseHPUI(int current, int max)
        {
            if (baseHPText)   baseHPText.text = $"Base HP: {current} / {max}";
            if (baseHPSlider)
            {
                baseHPSlider.maxValue = max;
                baseHPSlider.value    = current;
            }
        }

        private void UpdateWaveUI(int waveIndex)
        {
            _currentWaveDisplay = waveIndex + 1;
            RefreshWaveText(waveIndex);
            // Ẩn nút Start Wave khi wave đang chạy
            SetActive(startWaveButton, false);
        }

        private void RefreshWaveText(int waveIndex)
        {
            if (waveText) waveText.text = $"Wave {waveIndex + 1} / {totalWaves}";
        }

        private void HandleGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Win:
                    SetActive(winPanel,  true);
                    SetActive(losePanel, false);
                    SetActive(startWaveButton, false);
                    break;

                case GameState.Lose:
                    SetActive(losePanel, true);
                    SetActive(winPanel,  false);
                    SetActive(startWaveButton, false);
                    break;

                case GameState.WaveComplete:
                    // Hiện lại nút Start Wave để người chơi bắt đầu wave tiếp
                    SetActive(startWaveButton, true);
                    break;

                case GameState.Preparation:
                    SetActive(startWaveButton, true);
                    SetActive(winPanel,  false);
                    SetActive(losePanel, false);
                    break;
            }
        }

        // --- Button Callbacks (gắn vào Button.OnClick trong Inspector) ---

        /// <summary>
        /// Gọi từ nút "Bắt đầu Wave" trong Inspector.
        /// Phát OnWaveStartRequested → Dev2's WaveManager lắng nghe.
        /// </summary>
        public void OnStartWaveClicked()
        {
            if (GameStateManager.Instance &&
                GameStateManager.Instance.CurrentState != GameState.Preparation &&
                GameStateManager.Instance.CurrentState != GameState.WaveComplete)
            {
                Debug.LogWarning("[UIManager] Không thể bắt đầu wave khi đang trong wave!");
                return;
            }

            Debug.Log("[UIManager] Start Wave clicked → RaiseWaveStartRequested");
            GameEvents.RaiseWaveStartRequested();
        }

        /// <summary>Gọi từ nút "Chơi lại" trong Win/Lose panel.</summary>
        public void OnRestartClicked()
        {
            Debug.Log("[UIManager] Restart game.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // --- Helper ---
        private static void SetActive(GameObject go, bool active)
        {
            if (!go)
            {
                Debug.LogWarning("[UIManager] SetActive skipped because target is null.");
                return;
            }

            Debug.Log($"[UIManager] SetActive target={go.name}, active={active}, scene={go.scene.name}", go);

            if (go.GetComponent<Canvas>() != null)
            {
                Debug.LogError($"[UIManager] ERROR: Trying to SetActive Canvas directly: {go.name}", go);
                return;
            }

            go.SetActive(active);
        }
    }
}
