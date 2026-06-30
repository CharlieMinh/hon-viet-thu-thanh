using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HonVietThuThanh.Dev5
{
    [RequireComponent(typeof(UnitStarData))]
    public class UnitStarVisual : MonoBehaviour
    {
        [Header("Art")]
        public Sprite starSprite;

        private TMP_Text starText;
        private Canvas starCanvas;
        private readonly List<Image> starImages = new List<Image>();
        private UnitStarData starData;
        private Vector3 baseScale;
        private bool baseScaleCaptured = false;

        private void Awake()
        {
            starData = GetComponent<UnitStarData>();
            CaptureBaseScale();
            SetupTextComponent();
        }

        private void Start()
        {
            RefreshVisual();
        }

        private void CaptureBaseScale()
        {
            if (!baseScaleCaptured)
            {
                baseScale = transform.localScale;
                baseScaleCaptured = true;
            }
        }

        private void SetupTextComponent()
        {
            starText = GetComponentInChildren<TMP_Text>();
            if (starText == null)
            {
                Transform uiParent = transform.Find("UI");
                Transform parentToUse = uiParent != null ? uiParent : transform;
                Transform existingStarText = parentToUse.Find("StarText");
                if (existingStarText != null)
                {
                    starText = existingStarText.GetComponent<TMP_Text>();
                    if (starText == null)
                    {
                        starText = existingStarText.gameObject.AddComponent<TextMeshPro>();
                    }
                }
                else
                {
                    GameObject textGo = new GameObject("StarText");
                    textGo.transform.SetParent(parentToUse, false);
                    textGo.transform.localPosition = new Vector3(0f, 2.5f, 0f);
                    textGo.transform.localRotation = Quaternion.identity;
                    starText = textGo.AddComponent<TextMeshPro>();
                }

                starText.alignment = TextAlignmentOptions.Center;
                starText.fontSize = 4;
                starText.color = Color.yellow;
            }
        }

        public void RefreshVisual()
        {
            if (starData == null)
            {
                starData = GetComponent<UnitStarData>();
            }

            if (starData == null)
            {
                return;
            }

            CaptureBaseScale();

            int star = starData.starLevel;
            if (starSprite != null)
            {
                RefreshStarIcons(star);
                if (starText != null)
                {
                    starText.gameObject.SetActive(false);
                }
            }
            else if (starText != null)
            {
                starText.gameObject.SetActive(true);
                if (star <= 3)
                {
                    starText.text = new string('*', Mathf.Max(1, star));
                }
                else
                {
                    starText.text = $"* x{star}";
                }
            }

            float scaleMultiplier = 1f + 0.1f * (star - 1);
            transform.localScale = baseScale * scaleMultiplier;
        }

        private void RefreshStarIcons(int star)
        {
            EnsureStarCanvas();
            if (starCanvas == null)
            {
                return;
            }

            int iconCount = Mathf.Clamp(star, 1, 3);
            for (int i = starImages.Count; i < iconCount; i++)
            {
                GameObject iconGo = new GameObject($"StarIcon_{i + 1}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                iconGo.transform.SetParent(starCanvas.transform, false);
                Image image = iconGo.GetComponent<Image>();
                image.raycastTarget = false;
                image.preserveAspect = true;
                starImages.Add(image);
            }

            float iconSize = 0.28f;
            float spacing = 0.23f;
            float startX = -spacing * (iconCount - 1) * 0.5f;

            for (int i = 0; i < starImages.Count; i++)
            {
                Image image = starImages[i];
                bool active = i < iconCount;
                image.gameObject.SetActive(active);
                if (!active)
                {
                    continue;
                }

                image.sprite = starSprite;
                image.color = Color.white;

                RectTransform rect = image.rectTransform;
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(iconSize, iconSize);
                rect.anchoredPosition = new Vector2(startX + i * spacing, 0f);
                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;
            }
        }

        private void EnsureStarCanvas()
        {
            if (starCanvas != null)
            {
                return;
            }

            Transform uiParent = transform.Find("UI");
            Transform parentToUse = uiParent != null ? uiParent : transform;
            Transform existingCanvas = parentToUse.Find("StarCanvas");

            GameObject canvasGo;
            if (existingCanvas != null)
            {
                canvasGo = existingCanvas.gameObject;
                canvasGo.transform.localPosition = new Vector3(0f, 2.18f, 0f);
                canvasGo.transform.localRotation = Quaternion.identity;
                canvasGo.transform.localScale = Vector3.one;
            }
            else
            {
                canvasGo = new GameObject("StarCanvas", typeof(RectTransform), typeof(Canvas));
                canvasGo.transform.SetParent(parentToUse, false);
                canvasGo.transform.localPosition = new Vector3(0f, 2.18f, 0f);
                canvasGo.transform.localRotation = Quaternion.identity;
                canvasGo.transform.localScale = Vector3.one;
            }

            starCanvas = canvasGo.GetComponent<Canvas>();
            if (starCanvas == null)
            {
                starCanvas = canvasGo.AddComponent<Canvas>();
            }

            starCanvas.renderMode = RenderMode.WorldSpace;
            starCanvas.sortingOrder = 25;

            RectTransform rect = canvasGo.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(1f, 0.35f);
                rect.localScale = Vector3.one;
            }

            starImages.Clear();
            Image[] existingImages = canvasGo.GetComponentsInChildren<Image>(true);
            foreach (Image image in existingImages)
            {
                starImages.Add(image);
            }
        }

        private void LateUpdate()
        {
            if (starText != null && Camera.main != null)
            {
                starText.transform.rotation = Quaternion.LookRotation(starText.transform.position - Camera.main.transform.position);
            }

            if (starCanvas != null && Camera.main != null)
            {
                starCanvas.transform.rotation = Quaternion.LookRotation(starCanvas.transform.position - Camera.main.transform.position);
            }
        }
    }
}
