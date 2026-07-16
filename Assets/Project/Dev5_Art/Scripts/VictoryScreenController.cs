using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Runtime victory overlay for the Dev5 combat flow.
    /// It builds itself in code so the screen appears without scene wiring.
    /// </summary>
    public class VictoryScreenController : MonoBehaviour
    {
        private const string DefaultMainMenuSceneName = "Scene_MainMenu";

        private static readonly Color MenuGold = new Color(1f, 0.82f, 0.38f, 1f);
        private static readonly Color MenuDark = new Color(0.035f, 0.02f, 0.015f, 0.95f);

        [Header("Scenes")]
        [SerializeField] private string mainMenuSceneName = DefaultMainMenuSceneName;

        [Header("Story Before End Screen")]
        [SerializeField] private bool playStoryBeforeEndScreen = true;
        [SerializeField] private StoryPresenter storyPresenter;
        [SerializeField] private StorySequence victoryStorySequence;
        [SerializeField] private StorySequence defeatStorySequence;

        private CanvasGroup rootGroup;
        private GamePhaseManager subscribedPhaseManager;
        private readonly List<Image> sunburstRayImages = new List<Image>();
        private Image dimOverlay;
        private TMP_Text resultText;
        private TMP_Text subtitleText;
        private TMP_Text restartButtonText;
        private bool victoryStoryPlayed;
        private bool defeatStoryPlayed;
        private bool endStoryPlaying;
        private GameState pendingEndScreenState;

        public static VictoryScreenController EnsureExists()
        {
            VictoryScreenController existing =
                FindAnyObjectByType<VictoryScreenController>(FindObjectsInactive.Include);

            if (existing != null)
            {
                return existing;
            }

            GameObject host = new GameObject("VictoryScreenController");
            return host.AddComponent<VictoryScreenController>();
        }

        private void Awake()
        {
            BuildScreen();
            SetVisible(false);
        }

        private void OnEnable()
        {
            SubscribeToPhaseManager();
        }

        private void Start()
        {
            SubscribeToPhaseManager();

            if (GamePhaseManager.Instance != null)
            {
                ApplyGameState(GamePhaseManager.Instance.CurrentState);
            }
        }

        private void OnDisable()
        {
            if (subscribedPhaseManager != null)
            {
                subscribedPhaseManager.OnGameStateChanged -= ApplyGameState;
                subscribedPhaseManager = null;
            }

            if (storyPresenter != null)
            {
                storyPresenter.SequenceCompleted -= HandleEndStoryCompleted;
            }
        }

        private void SubscribeToPhaseManager()
        {
            if (subscribedPhaseManager == GamePhaseManager.Instance)
            {
                return;
            }

            if (subscribedPhaseManager != null)
            {
                subscribedPhaseManager.OnGameStateChanged -= ApplyGameState;
            }

            subscribedPhaseManager = GamePhaseManager.Instance;

            if (subscribedPhaseManager != null)
            {
                subscribedPhaseManager.OnGameStateChanged += ApplyGameState;
            }
        }

        private void ApplyGameState(GameState state)
        {
            bool isEndScreen = state == GameState.Win || state == GameState.Lose;
            if (isEndScreen && TryPlayEndStoryBeforePanel(state))
            {
                SetVisible(false);
                return;
            }

            if (isEndScreen)
            {
                ApplyEndScreenContent(state);
            }

            SetVisible(isEndScreen);
        }

        private bool TryPlayEndStoryBeforePanel(GameState state)
        {
            if (!playStoryBeforeEndScreen || endStoryPlaying)
            {
                return false;
            }

            StorySequence sequence = GetEndStorySequence(state);
            if (sequence == null)
            {
                return false;
            }

            if (state == GameState.Win && victoryStoryPlayed)
            {
                return false;
            }

            if (state == GameState.Lose && defeatStoryPlayed)
            {
                return false;
            }

            ResolveStoryPresenter();
            if (storyPresenter == null)
            {
                Debug.LogWarning("[VictoryScreenController] Cannot play end story because StoryPresenter is missing.", this);
                return false;
            }

            pendingEndScreenState = state;
            endStoryPlaying = true;
            if (state == GameState.Win)
            {
                victoryStoryPlayed = true;
            }
            else
            {
                defeatStoryPlayed = true;
            }

            storyPresenter.SequenceCompleted -= HandleEndStoryCompleted;
            storyPresenter.SequenceCompleted += HandleEndStoryCompleted;
            storyPresenter.Play(sequence);
            return true;
        }

        private void HandleEndStoryCompleted(StorySequence completedSequence)
        {
            if (!endStoryPlaying)
            {
                return;
            }

            storyPresenter.SequenceCompleted -= HandleEndStoryCompleted;
            endStoryPlaying = false;
            ApplyEndScreenContent(pendingEndScreenState);
            SetVisible(true);
        }

        private void ResolveStoryPresenter()
        {
            if (storyPresenter != null)
            {
                return;
            }

            storyPresenter = FindAnyObjectByType<StoryPresenter>(FindObjectsInactive.Include);
        }

        private StorySequence GetEndStorySequence(GameState state)
        {
            StorySequence sequence = state == GameState.Win ? victoryStorySequence : defeatStorySequence;
            if (sequence != null)
            {
                return sequence;
            }

#if UNITY_EDITOR
            string assetName = state == GameState.Win ? "Victory_Story_Test" : "Defeat_Story_Test";
            string[] guids = AssetDatabase.FindAssets(assetName, new[] { "Assets/Project/Dev5_Art" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                StorySequence loadedSequence = AssetDatabase.LoadAssetAtPath<StorySequence>(path);
                if (loadedSequence == null || loadedSequence.name != assetName)
                {
                    continue;
                }

                if (state == GameState.Win)
                {
                    victoryStorySequence = loadedSequence;
                }
                else
                {
                    defeatStorySequence = loadedSequence;
                }

                return loadedSequence;
            }
#endif

            return null;
        }

        private void ApplyEndScreenContent(GameState state)
        {
            if (state == GameState.Lose)
            {
                if (dimOverlay != null)
                {
                    dimOverlay.color = new Color(0.13f, 0.015f, 0.015f, 0.9f);
                }

                ApplyLoseBackgroundColors();

                if (resultText != null)
                {
                    resultText.text = "Thành Đã\nThất Thủ!";
                    resultText.color = new Color(1f, 0.74f, 0.62f, 1f);
                    ConfigureLoseResultText();
                }

                if (subtitleText != null)
                {
                    subtitleText.text = "Tướng trên sàn đã bị đánh bại.";
                }

                if (restartButtonText != null)
                {
                    restartButtonText.text = "Thử Lại";
                }

                return;
            }

            if (dimOverlay != null)
            {
                dimOverlay.color = new Color(0.12f, 0.02f, 0.0f, 0.86f);
            }

            ApplyWinBackgroundColors();

            if (resultText != null)
            {
                resultText.text = "Chiến\nThắng!";
                resultText.color = MenuGold;
                ConfigureWinResultText();
            }

            if (subtitleText != null)
            {
                subtitleText.text = "Bạn đã bảo vệ Dòng Chảy Linh Khí.";
            }

            if (restartButtonText != null)
            {
                restartButtonText.text = "Chơi Lại";
            }
        }

        private void ConfigureLoseResultText()
        {
            if (resultText == null)
            {
                return;
            }

            resultText.enableAutoSizing = true;
            resultText.fontSizeMax = 42f;
            resultText.fontSizeMin = 28f;
            resultText.lineSpacing = -7f;
            resultText.textWrappingMode = TextWrappingModes.NoWrap;
            resultText.outlineColor = new Color(0.18f, 0.015f, 0.015f, 1f);
            resultText.outlineWidth = 0.34f;
        }

        private void ApplyLoseBackgroundColors()
        {
            for (int i = 0; i < sunburstRayImages.Count; i++)
            {
                if (sunburstRayImages[i] == null)
                {
                    continue;
                }

                sunburstRayImages[i].color = i % 2 == 0
                    ? new Color(0.72f, 0.03f, 0.02f, 0.20f)
                    : new Color(0.95f, 0.12f, 0.08f, 0.09f);
            }
        }

        private void ApplyWinBackgroundColors()
        {
            for (int i = 0; i < sunburstRayImages.Count; i++)
            {
                if (sunburstRayImages[i] == null)
                {
                    continue;
                }

                sunburstRayImages[i].color = i % 2 == 0
                    ? new Color(1f, 0.62f, 0.12f, 0.14f)
                    : new Color(1f, 0.82f, 0.28f, 0.06f);
            }
        }

        private void ConfigureWinResultText()
        {
            if (resultText == null)
            {
                return;
            }

            resultText.enableAutoSizing = true;
            resultText.fontSizeMax = 46f;
            resultText.fontSizeMin = 30f;
            resultText.lineSpacing = -14f;
            resultText.textWrappingMode = TextWrappingModes.NoWrap;
            resultText.outlineColor = new Color(0.1f, 0.035f, 0f, 1f);
            resultText.outlineWidth = 0.24f;
        }

        private void SetVisible(bool visible)
        {
            if (rootGroup == null)
            {
                return;
            }

            rootGroup.alpha = visible ? 1f : 0f;
            rootGroup.interactable = visible;
            rootGroup.blocksRaycasts = visible;
        }

        private void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void BackToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void BuildScreen()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5000;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960f, 750f);
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();

            rootGroup = gameObject.AddComponent<CanvasGroup>();

            RectTransform root = gameObject.GetComponent<RectTransform>();
            Stretch(root);

            dimOverlay = CreateImage("DimOverlay", root, new Color(0.12f, 0.02f, 0.0f, 0.86f));
            Stretch(dimOverlay.rectTransform);

            sunburstRayImages.Clear();
            CreateSunburstRays(root, sunburstRayImages);

            Image topShade = CreateImage("TopShade", root, new Color(0f, 0f, 0f, 0.22f));
            Stretch(topShade.rectTransform);

            RectTransform panel = CreateRect("VictoryPanel", root);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(300f, 430f);
            panel.anchoredPosition = new Vector2(0f, 70f);

            Image panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.color = new Color(0.12f, 0.09f, 0.08f, 0.48f);
            Outline panelOutline = panel.gameObject.AddComponent<Outline>();
            panelOutline.effectColor = new Color(1f, 0.82f, 0.32f, 0.18f);
            panelOutline.effectDistance = new Vector2(1f, -1f);

            CreateCornerPin(panel, new Vector2(-1f, 1f));
            CreateCornerPin(panel, new Vector2(1f, 1f));
            CreateCornerPin(panel, new Vector2(-1f, -1f));
            CreateCornerPin(panel, new Vector2(1f, -1f));

            RectTransform badge = CreateRect("VictoryBadge", panel);
            badge.anchorMin = new Vector2(0.5f, 1f);
            badge.anchorMax = new Vector2(0.5f, 1f);
            badge.pivot = new Vector2(0.5f, 1f);
            badge.sizeDelta = new Vector2(126f, 126f);
            badge.anchoredPosition = new Vector2(0f, -34f);

            CreateLogoImage(badge);

            resultText = CreateText("ResultText", panel, "Chiến\nThắng!", 46f, FontStyles.Bold);
            resultText.color = MenuGold;
            resultText.alignment = TextAlignmentOptions.Center;
            resultText.lineSpacing = -14f;
            resultText.textWrappingMode = TextWrappingModes.NoWrap;
            ApplyReadableTextStyle(resultText, new Color(0.1f, 0.035f, 0f, 1f), 0.24f);
            SetRect(resultText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(300f, 118f), new Vector2(0f, -178f));

            subtitleText = CreateText("ResultSubtitle", panel, "Bạn đã bảo vệ Dòng Chảy Linh Khí.", 14f, FontStyles.Normal);
            subtitleText.color = new Color(1f, 0.92f, 0.72f, 1f);
            subtitleText.alignment = TextAlignmentOptions.Center;
            ApplyReadableTextStyle(subtitleText, new Color(0.06f, 0.02f, 0f, 1f), 0.12f);
            SetRect(subtitleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(280f, 32f), new Vector2(0f, -286f));

            Button restartButton = CreateButton(panel, "RestartButton", "Chơi Lại", new Vector2(0f, -332f));
            restartButton.onClick.AddListener(RestartGame);

            Button menuButton = CreateButton(panel, "MainMenuButton", "Trở về Menu", new Vector2(0f, -384f));
            menuButton.onClick.AddListener(BackToMainMenu);
        }

        private Button CreateButton(RectTransform parent, string name, string label, Vector2 anchoredPosition)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(255f, 44f);
            rect.anchoredPosition = anchoredPosition;

            Image image = rect.gameObject.AddComponent<Image>();
            image.color = MenuDark;

            Outline outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.72f, 0.26f, 0.9f);
            outline.effectDistance = new Vector2(1f, -1f);

            Button button = rect.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.88f, 0.45f, 1f);
            colors.pressedColor = new Color(0.8f, 0.58f, 0.18f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            TMP_Text text = CreateText("Label", rect, label, 20f, FontStyles.Bold);
            text.color = MenuGold;
            text.alignment = TextAlignmentOptions.Center;
            ApplyReadableTextStyle(text, Color.black, 0.16f);
            Stretch(text.rectTransform);

            if (name == "RestartButton")
            {
                restartButtonText = text;
            }

            return button;
        }

        private static void CreateLogoImage(RectTransform parent)
        {
            Sprite logoSprite = LoadDev4LogoSprite();
            if (logoSprite == null)
            {
                TMP_Text fallbackText = CreateText("LogoFallback", parent, "HV", 34f, FontStyles.Bold);
                fallbackText.color = MenuGold;
                fallbackText.alignment = TextAlignmentOptions.Center;
                ApplyReadableTextStyle(fallbackText, Color.black, 0.16f);
                Stretch(fallbackText.rectTransform);
                return;
            }

            RectTransform logoRect = CreateRect("Logo_Image", parent);
            logoRect.anchorMin = new Vector2(0.5f, 0.5f);
            logoRect.anchorMax = new Vector2(0.5f, 0.5f);
            logoRect.pivot = new Vector2(0.5f, 0.5f);
            logoRect.sizeDelta = new Vector2(118f, 118f);
            logoRect.anchoredPosition = Vector2.zero;

            Image logoImage = logoRect.gameObject.AddComponent<Image>();
            logoImage.sprite = logoSprite;
            logoImage.color = Color.white;
            logoImage.preserveAspect = true;
            logoImage.raycastTarget = false;
        }

        private static Sprite LoadDev4LogoSprite()
        {
            return Dev5RuntimeUIArt.LoadSprite(Dev5RuntimeUIArt.GameLogo);
        }

        private static TMP_FontAsset LoadDev4MenuFont()
        {
            return Dev5RuntimeUIArt.LoadMenuFont();
        }

        private static TMP_Text CreateText(string name, RectTransform parent, string text, float size, FontStyles style)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            TMP_FontAsset menuFont = LoadDev4MenuFont();
            if (menuFont != null)
            {
                tmp.font = menuFont;
            }

            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = Mathf.Max(8f, size * 0.6f);
            tmp.fontSizeMax = size;
            tmp.raycastTarget = false;

            return tmp;
        }

        private static void ApplyReadableTextStyle(TMP_Text text, Color outlineColor, float outlineWidth)
        {
            text.outlineColor = outlineColor;
            text.outlineWidth = outlineWidth;

            Shadow shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.82f);
            shadow.effectDistance = new Vector2(2f, -2f);
            shadow.useGraphicAlpha = true;
        }

        private static Image CreateImage(string name, RectTransform parent, Color color)
        {
            RectTransform rect = CreateRect(name, parent);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static RectTransform CreateRect(string name, RectTransform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.AddComponent<RectTransform>();
        }

        private static void CreateSunburstRays(RectTransform parent, List<Image> rayImages)
        {
            RectTransform raysRoot = CreateRect("VictoryRays", parent);
            Stretch(raysRoot);

            const int rayCount = 18;
            const float startAngle = -86f;
            const float angleStep = 10f;

            for (int i = 0; i < rayCount; i++)
            {
                RectTransform ray = CreateRect("Ray", raysRoot);
                ray.anchorMin = new Vector2(0.5f, 1f);
                ray.anchorMax = new Vector2(0.5f, 1f);
                ray.pivot = new Vector2(0.5f, 1f);
                ray.sizeDelta = new Vector2(78f, 1200f);
                ray.anchoredPosition = new Vector2(0f, 76f);
                ray.localEulerAngles = new Vector3(0f, 0f, startAngle + angleStep * i);

                Image image = ray.gameObject.AddComponent<Image>();
                image.color = i % 2 == 0
                    ? new Color(1f, 0.62f, 0.12f, 0.14f)
                    : new Color(1f, 0.82f, 0.28f, 0.06f);
                image.raycastTarget = false;
                rayImages.Add(image);
            }
        }

        private static void CreateCornerPin(RectTransform parent, Vector2 corner)
        {
            RectTransform pin = CreateRect("CornerPin", parent);
            pin.anchorMin = new Vector2(corner.x < 0f ? 0f : 1f, corner.y < 0f ? 0f : 1f);
            pin.anchorMax = pin.anchorMin;
            pin.pivot = new Vector2(0.5f, 0.5f);
            pin.sizeDelta = new Vector2(10f, 10f);
            pin.anchoredPosition = new Vector2(corner.x * 8f, corner.y * 8f);

            Image image = pin.gameObject.AddComponent<Image>();
            image.color = new Color(0.05f, 0.035f, 0.03f, 0.78f);

            Outline outline = pin.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.88f, 0.42f, 0.22f);
            outline.effectDistance = new Vector2(1f, -1f);
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 anchoredPosition)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
        }
    }

}
