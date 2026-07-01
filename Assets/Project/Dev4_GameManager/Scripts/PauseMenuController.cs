using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HonVietThuThanh.Dev4
{
    /// <summary>
    /// Controls the gameplay pause menu UI only.
    /// It does not alter combat, wave, economy, health, placement, or unit logic.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject pauseRoot;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button pauseButton;

        [Header("Settings")]
        [SerializeField] private SettingsMenuController settingsController;

        [Header("Scenes")]
        [SerializeField] private string mainMenuSceneName = "Scene_MainMenu";

        public bool IsPaused { get; private set; }

        private bool isLoadingScene;

        private void Awake()
        {
            Time.timeScale = 1f;
            SetPauseUiVisible(false);
            SetSettingsVisible(false);

            // The scene wires the pause button through UnityEvent so this component
            // does not add a duplicate runtime listener and accidentally toggle twice.
            if (pauseButton != null)
            {
                pauseButton.interactable = true;
            }
        }

        private void Update()
        {
            if (!WasPausePressed())
            {
                return;
            }

            if (IsSettingsOpen())
            {
                CloseSettings();
                return;
            }

            TogglePause();
        }

        private void OnDisable()
        {
            ResetTimeScaleIfNeeded();
        }

        private void OnDestroy()
        {
            ResetTimeScaleIfNeeded();
        }

        public void TogglePause()
        {
            if (IsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        public void PauseGame()
        {
            IsPaused = true;
            Time.timeScale = 0f;
            SetSettingsVisible(false);
            SetPauseUiVisible(true);
        }

        public void ResumeGame()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            SetSettingsVisible(false);
            SetPauseUiVisible(false);
        }

        public void RestartGame()
        {
            isLoadingScene = true;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void OpenSettings()
        {
            IsPaused = true;
            Time.timeScale = 0f;
            SetPausePanelVisible(false);

            if (settingsController != null)
            {
                settingsController.OpenSettings();
            }
            else
            {
                SetSettingsVisible(true);
            }
        }

        public void CloseSettings()
        {
            IsPaused = true;
            Time.timeScale = 0f;

            if (settingsController != null)
            {
                settingsController.CloseWithoutSaving();
            }
            else
            {
                SetSettingsVisible(false);
            }

            SetPauseUiVisible(true);
        }

        public void SaveSettingsAndReturnToPause()
        {
            IsPaused = true;
            Time.timeScale = 0f;

            if (settingsController != null)
            {
                settingsController.SaveSettings();
            }

            SetSettingsVisible(false);
            SetPauseUiVisible(true);
        }

        public void BackToMainMenu()
        {
            isLoadingScene = true;
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private bool IsSettingsOpen()
        {
            return settingsPanel != null && settingsPanel.activeSelf;
        }

        private static bool WasPausePressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                return true;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                return true;
            }
#endif

            return false;
        }

        private void SetPauseUiVisible(bool visible)
        {
            if (pauseRoot != null)
            {
                pauseRoot.SetActive(visible);
            }

            SetPausePanelVisible(visible);
        }

        private void SetPausePanelVisible(bool visible)
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(visible);
            }
        }

        private void SetSettingsVisible(bool visible)
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(visible);
            }
        }

        private void ResetTimeScaleIfNeeded()
        {
            if (!isLoadingScene && IsPaused)
            {
                Time.timeScale = 1f;
            }
        }
    }
}
