using UnityEngine;
using UnityEngine.SceneManagement;

namespace HonVietThuThanh.Dev4
{
    /// <summary>
    /// Controls the standalone main menu scene only.
    /// This script does not touch gameplay state, combat, economy, placement, or audio systems.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Scene")]
        [SerializeField] private string gameplaySceneName = "Scene_Dev5_Art";

        [Header("Panels")]
        [SerializeField] private GameObject howToPlayPanel;
        [SerializeField] private GameObject settingsPanel;

        private void Awake()
        {
            CloseHowToPlay();
            CloseSettings();
        }

        public void StartGame()
        {
            if (string.IsNullOrWhiteSpace(gameplaySceneName))
            {
                Debug.LogError("[MainMenuController] Gameplay scene name is empty.");
                return;
            }

            SceneManager.LoadScene(gameplaySceneName);
        }

        public void OpenHowToPlay()
        {
            SetPanelActive(howToPlayPanel, true);
            SetPanelActive(settingsPanel, false);
        }

        public void CloseHowToPlay()
        {
            SetPanelActive(howToPlayPanel, false);
        }

        public void OpenSettings()
        {
            SetPanelActive(settingsPanel, true);
            SetPanelActive(howToPlayPanel, false);
        }

        public void CloseSettings()
        {
            SetPanelActive(settingsPanel, false);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            Debug.Log("[MainMenuController] Quit requested. Application.Quit runs only in build.");
#else
            Application.Quit();
#endif
        }

        private static void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
        }
    }
}
