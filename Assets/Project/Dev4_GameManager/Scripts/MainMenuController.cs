using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

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
        [SerializeField] private string loadingSceneName = "Scene_Loading";

        [Header("Panels")]
        [SerializeField] private GameObject howToPlayPanel;
        [SerializeField] private TMP_Text howToPlayBodyText;
        [SerializeField] private GameObject settingsPanel;

        private TMP_Text howToPlayRightText;

        private const string LeftGuideText =
            "<color=#EABF62><b>01  MUA TƯỚNG</b></color>\n" +
            "Dùng Vàng mua tướng. Tướng mới sẽ chờ ngoài bàn cờ.\n\n" +
            "<color=#EABF62><b>02  XẾP ĐỘI HÌNH</b></color>\n" +
            "Chọn tướng ở hàng chờ, rồi bấm một ô trống hợp lệ.\n\n" +
            "<color=#EABF62><b>03  PHÂN VAI</b></color>\n" +
            "Tank tuyến trước, Kỵ Sĩ áp sát, Xạ Thủ đứng phía sau.\n\n" +
            "<color=#EABF62><b>04  NÂNG SAO</b></color>\n" +
            "Ba tướng cùng loại, cùng sao sẽ tự hợp nhất và mạnh hơn.";

        private const string RightGuideText =
            "<color=#EABF62><b>05  BẮT ĐẦU TRẬN</b></color>\n" +
            "Nhấn Bắt Đầu Trận Chiến. Tướng sẽ tự động chiến đấu.\n\n" +
            "<color=#EABF62><b>06  QUA VÒNG</b></color>\n" +
            "Hạ sạch quái để nhận Vàng, lợi tức và sang vòng tiếp theo.\n\n" +
            "<color=#EABF62><b>07  MỤC TIÊU</b></color>\n" +
            "Bảo vệ Thành. Thành hết Sinh lực là thua.\n\n" +
            "<color=#D5B98A><b>MẸO NHANH</b></color>\n" +
            "Nhấn ESC để tạm dừng, mở Cài Đặt hoặc trở về Menu.";

        private void Awake()
        {
            ApplyHowToPlayGuide();
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

            Time.timeScale = 1f;
            SceneLoadRequest.Configure(
                gameplaySceneName,
                string.Empty,
                SceneLoadRequest.DefaultLoadingDescription,
                SceneLoadRequest.DefaultLoadingTip);

            if (string.IsNullOrWhiteSpace(loadingSceneName))
            {
                Debug.LogWarning("[MainMenuController] Loading scene name is empty. Loading gameplay scene directly.");
                SceneManager.LoadScene(gameplaySceneName);
                return;
            }

            SceneManager.LoadScene(loadingSceneName);
        }

        public void OpenHowToPlay()
        {
            ApplyHowToPlayGuide();
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

        private void ApplyHowToPlayGuide()
        {
            if (howToPlayBodyText == null && howToPlayPanel != null)
            {
                TMP_Text[] panelTexts = howToPlayPanel.GetComponentsInChildren<TMP_Text>(true);
                foreach (TMP_Text panelText in panelTexts)
                {
                    if (panelText.gameObject.name == "Body_Text")
                    {
                        howToPlayBodyText = panelText;
                        break;
                    }
                }
            }

            if (howToPlayBodyText != null)
            {
                RectTransform panelBox = howToPlayBodyText.transform.parent as RectTransform;
                if (panelBox == null)
                {
                    howToPlayBodyText.text = LeftGuideText + "\n\n" + RightGuideText;
                    return;
                }

                panelBox.sizeDelta = new Vector2(1040f, 600f);
                ConfigureGuideColumn(howToPlayBodyText, new Vector2(-245f, -5f), LeftGuideText);

                Transform rightTransform = panelBox.Find("Body_Right_Text");
                if (howToPlayRightText == null && rightTransform != null)
                {
                    howToPlayRightText = rightTransform.GetComponent<TMP_Text>();
                }

                if (howToPlayRightText == null)
                {
                    howToPlayRightText = Instantiate(howToPlayBodyText, panelBox);
                    howToPlayRightText.gameObject.name = "Body_Right_Text";
                }

                ConfigureGuideColumn(howToPlayRightText, new Vector2(245f, -5f), RightGuideText);
                ConfigureGuideTitle(panelBox);
                ConfigureGuideCloseButton(panelBox);
            }
        }

        private static void ConfigureGuideColumn(TMP_Text text, Vector2 position, string content)
        {
            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(440f, 340f);

            text.text = content;
            text.fontSize = 16f;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Truncate;
        }

        private static void ConfigureGuideTitle(RectTransform panelBox)
        {
            Transform titleTransform = panelBox.Find("Title_Text");
            if (titleTransform == null)
            {
                return;
            }

            TMP_Text title = titleTransform.GetComponent<TMP_Text>();
            if (title != null)
            {
                title.text = "HƯỚNG DẪN CHIẾN ĐẤU";
                title.fontSize = 38f;
                title.alignment = TextAlignmentOptions.Center;
            }

            RectTransform titleRect = titleTransform as RectTransform;
            if (titleRect != null)
            {
                titleRect.anchorMin = new Vector2(0.5f, 1f);
                titleRect.anchorMax = new Vector2(0.5f, 1f);
                titleRect.pivot = new Vector2(0.5f, 0.5f);
                titleRect.anchoredPosition = new Vector2(0f, -50f);
                titleRect.sizeDelta = new Vector2(800f, 60f);
            }
        }

        private static void ConfigureGuideCloseButton(RectTransform panelBox)
        {
            Transform closeTransform = panelBox.Find("Button_CloseHowToPlay");
            RectTransform closeRect = closeTransform as RectTransform;
            if (closeRect == null)
            {
                return;
            }

            closeRect.anchorMin = new Vector2(0.5f, 0f);
            closeRect.anchorMax = new Vector2(0.5f, 0f);
            closeRect.pivot = new Vector2(0.5f, 0.5f);
            closeRect.anchoredPosition = new Vector2(0f, 48f);
            closeRect.sizeDelta = new Vector2(220f, 54f);
        }
    }
}
