using UnityEngine;
using UnityEngine.UI;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Component quản lý thanh máu hiển thị trên đầu quân cờ hoặc quái (Phase 13).
    /// </summary>
    public class HealthBar : MonoBehaviour
    {
        [Header("Tham chiếu")]
        public Health health;
        public Image fillImage;

        private Canvas canvas;
        private GameObject canvasGo;
        private bool isInitialized = false;

        private void Awake()
        {
            if (health == null)
            {
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

            // Tự động dựng Canvas World Space và thanh máu nếu chưa gán fillImage
            if (fillImage == null)
            {
                CreateHealthBarDynamically();
            }
            else
            {
                // Nếu fillImage đã được gán, tìm canvas cha của nó để bật/tắt
                canvas = fillImage.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    canvasGo = canvas.gameObject;
                }
            }

            isInitialized = true;
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.OnHealthChanged += HandleHealthChanged;
                Refresh();
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnHealthChanged -= HandleHealthChanged;
            }
        }

        private void Start()
        {
            if (health == null)
            {
                Debug.LogWarning($"[{gameObject.name}] HealthBar hoạt động nhưng không tìm thấy component Health!");
            }
            Refresh();
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.OnHealthChanged -= HandleHealthChanged;
            }
        }

        private void HandleHealthChanged(int current, int max)
        {
            UpdateFill(current, max);
        }

        private void UpdateFill(int current, int max)
        {
            if (fillImage == null) return;

            float ratio = max <= 0 ? 0f : (float)current / max;
            float clampedRatio = Mathf.Clamp01(ratio);

            fillImage.fillAmount = clampedRatio;

            // Nếu thanh máu được dựng động (HealthBarCanvas), chỉnh anchor của fillImage RectTransform để co giãn đúng
            if (canvasGo != null && canvasGo.name == "HealthBarCanvas")
            {
                RectTransform rect = fillImage.rectTransform;
                if (rect != null)
                {
                    rect.anchorMax = new Vector2(clampedRatio, 1f);
                }
            }

            Debug.Log($"[HealthBar] {gameObject.name} fill updated: {clampedRatio:F2} ({current}/{max})");
        }

        /// <summary>
        /// Làm mới tỉ lệ hiển thị của thanh máu.
        /// </summary>
        public void Refresh()
        {
            if (health == null) return;
            UpdateFill(health.CurrentHealth, health.MaxHealth);
        }

        /// <summary>
        /// Hiển thị thanh máu.
        /// </summary>
        public void Show()
        {
            if (canvasGo != null)
            {
                canvasGo.SetActive(true);
            }
        }

        /// <summary>
        /// Ẩn thanh máu.
        /// </summary>
        public void Hide()
        {
            if (canvasGo != null)
            {
                canvasGo.SetActive(false);
            }
        }

        /// <summary>
        /// Dựng động Canvas World Space, background, và thanh máu fill.
        /// </summary>
        private void CreateHealthBarDynamically()
        {
            // Tìm Transform cha thích hợp (tìm child "UI")
            Transform uiParent = transform.Find("UI");
            Transform parentToUse = uiParent != null ? uiParent : transform;

            // Tìm xem đã có GameObject "HealthBar" hay "HealthBarCanvas" dưới parentToUse chưa
            Transform existingCanvas = parentToUse.Find("HealthBar");
            if (existingCanvas == null)
            {
                existingCanvas = parentToUse.Find("HealthBarCanvas");
            }

            if (existingCanvas != null)
            {
                canvasGo = existingCanvas.gameObject;
                canvas = canvasGo.GetComponent<Canvas>();
                if (canvas == null)
                {
                    canvas = canvasGo.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.WorldSpace;
                }
                
                // Đồng bộ RectTransform
                RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();
                if (canvasRect == null) canvasRect = canvasGo.AddComponent<RectTransform>();
                canvasRect.sizeDelta = new Vector2(1.2f, 0.15f);
                canvasRect.localScale = Vector3.one;

                // Tìm hoặc tạo Background
                Transform bgTransform = canvasGo.transform.Find("Background");
                GameObject bgGo = bgTransform != null ? bgTransform.gameObject : new GameObject("Background");
                bgGo.transform.SetParent(canvasGo.transform, false);
                Image bgImage = bgGo.GetComponent<Image>();
                if (bgImage == null) bgImage = bgGo.AddComponent<Image>();
                bgImage.color = new Color(0.15f, 0.15f, 0.15f, 0.85f);
                RectTransform bgRect = bgGo.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.sizeDelta = Vector2.zero;

                // Tìm hoặc tạo Fill
                Transform fillTransform = canvasGo.transform.Find("Fill");
                GameObject fillGo = fillTransform != null ? fillTransform.gameObject : new GameObject("Fill");
                fillGo.transform.SetParent(canvasGo.transform, false);
                fillImage = fillGo.GetComponent<Image>();
                if (fillImage == null) fillImage = fillGo.AddComponent<Image>();
                if (GetComponent<EnemyController>() != null || gameObject.name.Contains("Enemy"))
                {
                    fillImage.color = new Color(0.9f, 0.1f, 0.1f, 1f);
                }
                else
                {
                    fillImage.color = new Color(0.1f, 0.85f, 0.1f, 1f);
                }
                fillImage.type = Image.Type.Filled;
                fillImage.fillMethod = Image.FillMethod.Horizontal;
                fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

                RectTransform fillRect = fillGo.GetComponent<RectTransform>();
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = Vector2.one;
                fillRect.sizeDelta = Vector2.zero;
            }
            else
            {
                // Dựng động hoàn toàn
                canvasGo = new GameObject("HealthBar");
                canvasGo.transform.SetParent(parentToUse, false);
                canvasGo.transform.localPosition = new Vector3(0f, 2.2f, 0f);
                canvasGo.transform.localRotation = Quaternion.identity;

                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;

                RectTransform canvasRect = canvasGo.GetComponent<RectTransform>();
                canvasRect.sizeDelta = new Vector2(1.2f, 0.15f);
                canvasRect.localScale = Vector3.one;

                // 2. Background
                GameObject bgGo = new GameObject("Background");
                bgGo.transform.SetParent(canvasGo.transform, false);
                Image bgImage = bgGo.AddComponent<Image>();
                bgImage.color = new Color(0.15f, 0.15f, 0.15f, 0.85f);
                RectTransform bgRect = bgGo.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.sizeDelta = Vector2.zero;

                // 3. Fill
                GameObject fillGo = new GameObject("Fill");
                fillGo.transform.SetParent(canvasGo.transform, false);
                fillImage = fillGo.AddComponent<Image>();
                if (GetComponent<EnemyController>() != null || gameObject.name.Contains("Enemy"))
                {
                    fillImage.color = new Color(0.9f, 0.1f, 0.1f, 1f);
                }
                else
                {
                    fillImage.color = new Color(0.1f, 0.85f, 0.1f, 1f);
                }
                fillImage.type = Image.Type.Filled;
                fillImage.fillMethod = Image.FillMethod.Horizontal;
                fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

                RectTransform fillRect = fillGo.GetComponent<RectTransform>();
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = Vector2.one;
                fillRect.sizeDelta = Vector2.zero;
            }
        }

        private void LateUpdate()
        {
            // Billboard effect: Xoay mặt UI về phía camera chính
            if (canvasGo != null && Camera.main != null)
            {
                canvasGo.transform.rotation = Quaternion.LookRotation(canvasGo.transform.position - Camera.main.transform.position);
            }
        }
    }
}
