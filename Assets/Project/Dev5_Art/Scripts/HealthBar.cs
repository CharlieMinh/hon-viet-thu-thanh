using UnityEngine;
using UnityEngine.UI;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// World-space health bar shown above units and enemies.
    /// </summary>
    public class HealthBar : MonoBehaviour
    {
        private static readonly Vector2 FillAnchorMin = new Vector2(0.24f, 0.38f);
        private static readonly Vector2 FillAnchorMax = new Vector2(0.76f, 0.62f);

        [Header("References")]
        public Health health;
        public Image fillImage;

        [Header("Art")]
        public Sprite frameSprite;
        public Sprite fillSprite;

        private static Sprite runtimeSprite;

        private Canvas canvas;
        private GameObject canvasGo;
        private Image frameImage;
        private int lastCurrentHealth = int.MinValue;
        private int lastMaxHealth = int.MinValue;

        private void Awake()
        {
            ResolveHealth();
            ResolveOrCreateFillImage();
        }

        private void OnEnable()
        {
            if (health == null)
            {
                ResolveHealth();
            }

            if (fillImage == null)
            {
                ResolveOrCreateFillImage();
            }

            if (health != null)
            {
                health.OnHealthChanged -= HandleHealthChanged;
                health.OnHealthChanged += HandleHealthChanged;
                Refresh();
            }
        }

        private void Start()
        {
            if (health == null)
            {
                Debug.LogWarning($"[{gameObject.name}] HealthBar could not find a Health component.");
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnHealthChanged -= HandleHealthChanged;
            }
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.OnHealthChanged -= HandleHealthChanged;
            }
        }

        public void Refresh()
        {
            if (health == null)
            {
                return;
            }

            UpdateFill(health.CurrentHealth, health.MaxHealth);
        }

        public void Show()
        {
            if (canvasGo != null)
            {
                canvasGo.SetActive(true);
            }
        }

        public void Hide()
        {
            if (canvasGo != null)
            {
                canvasGo.SetActive(false);
            }
        }

        private void ResolveHealth()
        {
            if (health != null)
            {
                return;
            }

            health = GetComponent<Health>();
            if (health == null)
            {
                health = GetComponentInParent<Health>();
            }
            if (health == null)
            {
                health = GetComponentInChildren<Health>();
            }
        }

        private void ResolveOrCreateFillImage()
        {
            if (fillImage != null)
            {
                canvas = fillImage.GetComponentInParent<Canvas>();
                canvasGo = canvas != null ? canvas.gameObject : null;
                ConfigureFillImage(fillImage);
                FitFillToFrame(fillImage.rectTransform);
                return;
            }

            Transform uiParent = transform.Find("UI");
            Transform parentToUse = uiParent != null ? uiParent : transform;
            Transform existingBar = parentToUse.Find("HealthBar");
            if (existingBar == null)
            {
                existingBar = parentToUse.Find("HealthBarCanvas");
            }

            Transform canvasTransform = FindUsableCanvasTransform(existingBar);
            if (canvasTransform == null)
            {
                canvasTransform = CreateCanvasTransform(parentToUse, existingBar);
            }

            canvasGo = canvasTransform.gameObject;
            canvas = canvasGo.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = canvasGo.AddComponent<Canvas>();
            }

            ConfigureCanvas(canvasTransform);
            EnsureFrame(canvasTransform);
            fillImage = EnsureFill(canvasTransform);
            UpdateFill(health != null ? health.CurrentHealth : 1, health != null ? health.MaxHealth : 1);
        }

        private Transform FindUsableCanvasTransform(Transform existingBar)
        {
            if (existingBar == null)
            {
                return null;
            }

            Canvas existingCanvas = existingBar.GetComponent<Canvas>();
            RectTransform existingRect = existingBar.GetComponent<RectTransform>();
            if (existingCanvas != null && existingRect != null)
            {
                return existingBar;
            }

            Transform nestedCanvas = existingBar.Find("HealthBarCanvas");
            if (nestedCanvas != null && nestedCanvas.GetComponent<RectTransform>() != null)
            {
                return nestedCanvas;
            }

            return null;
        }

        private Transform CreateCanvasTransform(Transform parentToUse, Transform existingBar)
        {
            Transform parent = existingBar != null ? existingBar : parentToUse;
            string canvasName = existingBar != null ? "HealthBarCanvas" : "HealthBar";
            GameObject go = new GameObject(canvasName, typeof(RectTransform), typeof(Canvas));
            go.transform.SetParent(parent, false);

            if (existingBar == null)
            {
                go.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            }
            else
            {
                go.transform.localPosition = Vector3.zero;
            }

            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            return go.transform;
        }

        private void ConfigureCanvas(Transform canvasTransform)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 20;

            RectTransform rect = canvasTransform as RectTransform;
            if (rect == null)
            {
                rect = canvasTransform.GetComponent<RectTransform>();
            }

            if (rect != null)
            {
                rect.sizeDelta = new Vector2(1.35f, 0.32f);
                rect.localScale = new Vector3(1.55f, 2.2f, 1f);
            }
        }

        private void EnsureFrame(Transform canvasTransform)
        {
            Transform frameTransform = canvasTransform.Find("Frame");
            GameObject frameGo = frameTransform != null
                ? frameTransform.gameObject
                : new GameObject("Frame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

            frameGo.transform.SetParent(canvasTransform, false);

            frameImage = frameGo.GetComponent<Image>();
            if (frameImage == null)
            {
                frameImage = frameGo.AddComponent<Image>();
            }

            frameImage.sprite = frameSprite != null ? frameSprite : GetRuntimeSprite();
            frameImage.type = Image.Type.Simple;
            frameImage.color = frameSprite != null ? Color.white : new Color(0.15f, 0.15f, 0.15f, 0.85f);
            frameImage.preserveAspect = true;
            frameImage.raycastTarget = false;
            StretchToParent(frameGo.GetComponent<RectTransform>());
            frameGo.transform.SetAsLastSibling();
        }

        private Image EnsureFill(Transform canvasTransform)
        {
            Transform fillTransform = canvasTransform.Find("Fill");
            GameObject fillGo = fillTransform != null
                ? fillTransform.gameObject
                : new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

            fillGo.transform.SetParent(canvasTransform, false);

            Image image = fillGo.GetComponent<Image>();
            if (image == null)
            {
                image = fillGo.AddComponent<Image>();
            }

            ConfigureFillImage(image);
            FitFillToFrame(image.rectTransform);
            fillGo.transform.SetAsFirstSibling();
            return image;
        }

        private void ConfigureFillImage(Image image)
        {
            image.sprite = fillSprite != null ? fillSprite : GetRuntimeSprite();
            image.type = Image.Type.Simple;
            image.color = fillSprite != null
                ? Color.white
                : (IsEnemyHealthBar() ? new Color(0.9f, 0.1f, 0.1f, 1f) : new Color(0.1f, 0.85f, 0.1f, 1f));
            image.preserveAspect = false;
            image.raycastTarget = false;
        }

        private static Sprite GetRuntimeSprite()
        {
            if (runtimeSprite != null)
            {
                return runtimeSprite;
            }

            runtimeSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);
            runtimeSprite.name = "HealthBar_RuntimeWhiteSprite";
            runtimeSprite.hideFlags = HideFlags.HideAndDontSave;
            return runtimeSprite;
        }

        private bool IsEnemyHealthBar()
        {
            return GetComponent<EnemyController>() != null || gameObject.name.Contains("Enemy");
        }

        private static void StretchToParent(RectTransform rect)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private static void FitFillToFrame(RectTransform rect)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = FillAnchorMin;
            rect.anchorMax = FillAnchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        private void HandleHealthChanged(int current, int max)
        {
            UpdateFill(current, max);
        }

        private void UpdateFill(int current, int max)
        {
            if (fillImage == null)
            {
                ResolveOrCreateFillImage();
            }

            if (fillImage == null)
            {
                return;
            }

            float ratio = max <= 0 ? 0f : Mathf.Clamp01((float)current / max);
            RectTransform fillRect = fillImage.rectTransform;
            if (fillRect != null)
            {
                Vector2 anchorMax = FillAnchorMax;
                anchorMax.x = Mathf.Lerp(FillAnchorMin.x, FillAnchorMax.x, ratio);
                fillRect.anchorMin = FillAnchorMin;
                fillRect.anchorMax = anchorMax;
                fillRect.offsetMin = Vector2.zero;
                fillRect.offsetMax = Vector2.zero;
            }

            fillImage.fillAmount = 1f;
            lastCurrentHealth = current;
            lastMaxHealth = max;
        }

        private void LateUpdate()
        {
            if (health != null && (health.CurrentHealth != lastCurrentHealth || health.MaxHealth != lastMaxHealth))
            {
                UpdateFill(health.CurrentHealth, health.MaxHealth);
            }

            if (canvasGo != null && Camera.main != null)
            {
                canvasGo.transform.rotation = Quaternion.LookRotation(canvasGo.transform.position - Camera.main.transform.position);
            }
        }
    }
}
