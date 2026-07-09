using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HonVietThuThanh.Dev4
{
    /// <summary>
    /// Handles the transition-only loading scene between menu and gameplay.
    /// Uses unscaled time so pause state cannot freeze the loading flow.
    /// </summary>
    [ExecuteAlways]
    public class LoadingScreenController : MonoBehaviour
    {
        private const string FallbackTargetSceneName = SceneLoadRequest.DefaultTargetSceneName;
        private const string CanvasName = "Canvas_Loading";

        [Header("Progress")]
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TMP_Text progressText;

        [Header("Content")]
        [SerializeField] private Sprite backgroundSprite;
        [SerializeField] private Sprite logoSprite;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text tipText;

        [Header("Timing")]
        [SerializeField] private float minimumDisplayTime = 1.25f;
        [SerializeField] private float activationDelay = 0.15f;

        private void OnEnable()
        {
            EnsureRuntimeUi();
            PopulateStaticText();
            UpdateProgress(0f);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!gameObject.scene.IsValid())
            {
                return;
            }

            EnsureRuntimeUi();
            PopulateStaticText();
            UpdateProgress(0f);
        }
#endif

        private void Start()
        {
            Time.timeScale = 1f;
            EnsureRuntimeUi();
            PopulateStaticText();
            UpdateProgress(0f);

            if (Application.isPlaying)
            {
                StartCoroutine(LoadTargetSceneAsync());
            }
        }

        private IEnumerator LoadTargetSceneAsync()
        {
            string targetSceneName = SceneLoadRequest.TargetSceneName;
            if (string.IsNullOrWhiteSpace(targetSceneName))
            {
                targetSceneName = FallbackTargetSceneName;
                Debug.LogWarning($"[LoadingScreenController] Target scene was empty. Falling back to {FallbackTargetSceneName}.");
            }

            AsyncOperation operation = SceneManager.LoadSceneAsync(targetSceneName);
            if (operation == null)
            {
                Debug.LogError($"[LoadingScreenController] Could not start loading scene '{targetSceneName}'.");
                yield break;
            }

            operation.allowSceneActivation = false;
            float elapsed = 0f;

            while (!operation.isDone)
            {
                elapsed += Time.unscaledDeltaTime;
                float loadProgress = Mathf.Clamp01(operation.progress / 0.9f);
                float timeProgress = minimumDisplayTime > 0f ? Mathf.Clamp01(elapsed / minimumDisplayTime) : 1f;
                float displayedProgress = Mathf.Min(loadProgress, timeProgress);

                UpdateProgress(displayedProgress);

                if (loadProgress >= 1f && elapsed >= minimumDisplayTime)
                {
                    UpdateProgress(1f);

                    if (activationDelay > 0f)
                    {
                        yield return new WaitForSecondsRealtime(activationDelay);
                    }

                    operation.allowSceneActivation = true;
                    yield break;
                }

                yield return null;
            }
        }

        private void UpdateProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);

            if (progressSlider != null)
            {
                progressSlider.value = progress;
            }

            if (progressText != null)
            {
                int percent = Mathf.RoundToInt(progress * 100f);
                progressText.text = $"\u0110ANG T\u1ea2I... {percent}%";
            }
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }

        private void PopulateStaticText()
        {
            SetText(descriptionText, SceneLoadRequest.LoadingDescription);
            SetText(tipText, SceneLoadRequest.LoadingTip);
        }

        private void EnsureRuntimeUi()
        {
            if (progressSlider != null && progressText != null && descriptionText != null && tipText != null)
            {
                return;
            }

            Canvas canvas = FindChildComponent<Canvas>(CanvasName);
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("Canvas_Loading", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvasObject.transform.SetParent(transform, false);
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
            }

            ClearGeneratedUi(canvas.transform);
            CreateBackground(canvas.transform);
            CreateTopLeftTitle(canvas.transform);
            CreateCenterPanel(canvas.transform);
            CreateTipBox(canvas.transform);
            EnsureEventSystem();
        }

        private void CreateBackground(Transform parent)
        {
            Image background = CreateImage(parent, "Background_DarkFantasy", new Color(0.09f, 0.035f, 0.025f, 1f));
            Stretch(background.rectTransform);
            background.sprite = backgroundSprite;
            background.type = backgroundSprite != null ? Image.Type.Simple : Image.Type.Sliced;
            background.preserveAspect = backgroundSprite != null;

            Image warmVignette = CreateImage(parent, "Background_WarmOverlay", new Color(0.38f, 0.08f, 0.02f, 0.44f));
            Stretch(warmVignette.rectTransform);

            Image darkVignette = CreateImage(parent, "Background_DarkVignette", new Color(0.03f, 0.012f, 0.008f, 0.52f));
            Stretch(darkVignette.rectTransform);

            Image templeGlow = CreateImage(parent, "Center_TempleGlow", new Color(1f, 0.42f, 0.05f, 0.20f));
            RectTransform glowRect = templeGlow.rectTransform;
            glowRect.anchorMin = new Vector2(0.22f, 0.12f);
            glowRect.anchorMax = new Vector2(0.78f, 0.88f);
            glowRect.offsetMin = Vector2.zero;
            glowRect.offsetMax = Vector2.zero;
        }

        private void CreateTopLeftTitle(Transform parent)
        {
            TMP_Text label = CreateText(parent, "Title_GameName", "H\u1ed2N VI\u1ec6T TH\u1ee6 TH\u00c0NH", 30f, FontStyles.Bold, new Color(1f, 0.77f, 0.35f, 1f));
            RectTransform rect = label.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(76f, -48f);
            rect.sizeDelta = new Vector2(620f, 70f);
            label.alignment = TextAlignmentOptions.Left;
        }

        private void CreateCenterPanel(Transform parent)
        {
            Image panel = CreateImage(parent, "Panel_LoadingContent", new Color(0.12f, 0.045f, 0.03f, 0.90f));
            RectTransform panelRect = panel.rectTransform;
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = new Vector2(0f, 22f);
            panelRect.sizeDelta = new Vector2(1120f, 540f);

            Image accentTop = CreateImage(panel.transform, "Accent_TopGold", new Color(1f, 0.54f, 0.12f, 0.85f));
            RectTransform accentTopRect = accentTop.rectTransform;
            accentTopRect.anchorMin = new Vector2(0.06f, 1f);
            accentTopRect.anchorMax = new Vector2(0.94f, 1f);
            accentTopRect.pivot = new Vector2(0.5f, 1f);
            accentTopRect.anchoredPosition = Vector2.zero;
            accentTopRect.sizeDelta = new Vector2(0f, 5f);

            if (logoSprite != null)
            {
                Image logo = CreateImage(panel.transform, "Logo_Loading", Color.white);
                logo.sprite = logoSprite;
                logo.preserveAspect = true;
                RectTransform logoRect = logo.rectTransform;
                logoRect.anchorMin = new Vector2(0.5f, 0.72f);
                logoRect.anchorMax = new Vector2(0.5f, 0.72f);
                logoRect.pivot = new Vector2(0.5f, 0.5f);
                logoRect.anchoredPosition = new Vector2(0f, 56f);
                logoRect.sizeDelta = new Vector2(360f, 250f);
            }

            descriptionText = CreateText(panel.transform, "Text_Lore", SceneLoadRequest.DefaultLoadingDescription, 30f, FontStyles.Normal, new Color(0.98f, 0.84f, 0.64f, 1f));
            RectTransform descriptionRect = descriptionText.rectTransform;
            descriptionRect.anchorMin = new Vector2(0.12f, 0.34f);
            descriptionRect.anchorMax = new Vector2(0.88f, 0.56f);
            descriptionRect.offsetMin = Vector2.zero;
            descriptionRect.offsetMax = Vector2.zero;
            descriptionText.alignment = TextAlignmentOptions.Center;
            descriptionText.textWrappingMode = TextWrappingModes.Normal;

            CreateProgressBar(panel.transform);
        }

        private void CreateProgressBar(Transform parent)
        {
            GameObject sliderObject = new GameObject("Slider_LoadingProgress", typeof(RectTransform), typeof(Slider));
            sliderObject.transform.SetParent(parent, false);
            progressSlider = sliderObject.GetComponent<Slider>();
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
            progressSlider.interactable = false;

            RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.16f, 0.16f);
            sliderRect.anchorMax = new Vector2(0.84f, 0.16f);
            sliderRect.pivot = new Vector2(0.5f, 0.5f);
            sliderRect.anchoredPosition = Vector2.zero;
            sliderRect.sizeDelta = new Vector2(0f, 38f);

            Image background = CreateImage(sliderObject.transform, "Background", new Color(0.08f, 0.04f, 0.03f, 1f));
            Stretch(background.rectTransform);

            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObject.transform, false);
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(5f, 5f);
            fillAreaRect.offsetMax = new Vector2(-5f, -5f);

            Image fill = CreateImage(fillArea.transform, "Fill", new Color(1f, 0.48f, 0.06f, 1f));
            Stretch(fill.rectTransform);
            progressSlider.targetGraphic = fill;
            progressSlider.fillRect = fill.rectTransform;

            progressText = CreateText(parent, "Text_LoadingPercent", "\u0110ANG T\u1ea2I... 0%", 28f, FontStyles.Bold, new Color(1f, 0.77f, 0.35f, 1f));
            RectTransform percentRect = progressText.rectTransform;
            percentRect.anchorMin = new Vector2(0.16f, 0.03f);
            percentRect.anchorMax = new Vector2(0.84f, 0.12f);
            percentRect.offsetMin = Vector2.zero;
            percentRect.offsetMax = Vector2.zero;
            progressText.alignment = TextAlignmentOptions.Center;
        }

        private void CreateTipBox(Transform parent)
        {
            Image tipBox = CreateImage(parent, "TipBox", new Color(0.12f, 0.055f, 0.035f, 0.88f));
            RectTransform tipBoxRect = tipBox.rectTransform;
            tipBoxRect.anchorMin = new Vector2(0.5f, 0f);
            tipBoxRect.anchorMax = new Vector2(0.5f, 0f);
            tipBoxRect.pivot = new Vector2(0.5f, 0f);
            tipBoxRect.anchoredPosition = new Vector2(0f, 70f);
            tipBoxRect.sizeDelta = new Vector2(1180f, 92f);

            tipText = CreateText(tipBox.transform, "Text_Tip", SceneLoadRequest.DefaultLoadingTip, 28f, FontStyles.Italic, new Color(1f, 0.82f, 0.45f, 1f));
            Stretch(tipText.rectTransform, new Vector2(38f, 12f), new Vector2(-38f, -12f));
            tipText.alignment = TextAlignmentOptions.Center;
        }

        private static Image CreateImage(Transform parent, string name, Color color)
        {
            GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);

            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static TMP_Text CreateText(Transform parent, string name, string value, float fontSize, FontStyles style, Color color)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);

            TMP_Text text = textObject.GetComponent<TMP_Text>();
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.enableAutoSizing = true;
            text.fontSizeMin = Mathf.Max(16f, fontSize * 0.55f);
            text.fontSizeMax = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            return text;
        }

        private static void Stretch(RectTransform rectTransform)
        {
            Stretch(rectTransform, Vector2.zero, Vector2.zero);
        }

        private static void Stretch(RectTransform rectTransform, Vector2 offsetMin, Vector2 offsetMax)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }

        private static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));
            Type inputSystemModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");

            if (inputSystemModuleType != null)
            {
                eventSystemObject.AddComponent(inputSystemModuleType);
                return;
            }

            eventSystemObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        private T FindChildComponent<T>(string childName) where T : Component
        {
            Transform child = transform.Find(childName);
            return child != null ? child.GetComponent<T>() : null;
        }

        private static void ClearGeneratedUi(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
    }
}
