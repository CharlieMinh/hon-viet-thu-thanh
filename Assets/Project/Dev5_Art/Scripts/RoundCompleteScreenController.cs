using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Runtime overlay shown between combat waves.
    /// </summary>
    public class RoundCompleteScreenController : MonoBehaviour
    {
        private static readonly Color Gold = new Color(1f, 0.82f, 0.38f, 1f);
        private static readonly Color DeepRed = new Color(0.43f, 0.13f, 0.06f, 1f);
        private static readonly Color Wood = new Color(0.27f, 0.13f, 0.07f, 0.96f);
        private static readonly Color Parchment = new Color(0.93f, 0.78f, 0.52f, 0.98f);

        private CanvasGroup rootGroup;
        private GamePhaseManager subscribedPhaseManager;
        private TMP_Text titleText;
        private TMP_Text roundText;
        private TMP_Text defeatedValueText;
        private TMP_Text goldValueText;
        private TMP_Text goldDetailText;
        private TMP_Text unitCountValueText;
        private TMP_Text unitCountDetailText;
        private TMP_Text unitListTitleText;
        private RectTransform unitListRoot;

        public static RoundCompleteScreenController EnsureExists()
        {
            RoundCompleteScreenController existing =
                FindAnyObjectByType<RoundCompleteScreenController>(FindObjectsInactive.Include);

            if (existing != null)
            {
                return existing;
            }

            GameObject host = new GameObject("RoundCompleteScreenController");
            return host.AddComponent<RoundCompleteScreenController>();
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
            if (state == GameState.WaveCompleted)
            {
                RefreshContent();
                SetVisible(true);
                return;
            }

            SetVisible(false);
        }

        private void RefreshContent()
        {
            int completedWaveIndex = RoundResultTracker.CompletedWaveIndex;
            if (completedWaveIndex < 0 && WaveManager.Instance != null)
            {
                completedWaveIndex = WaveManager.Instance.currentWaveIndex;
            }

            int completedRoundNumber = Mathf.Max(1, completedWaveIndex + 1);
            int totalRounds = WaveManager.Instance != null ? WaveManager.Instance.waves.Count : completedRoundNumber;

            if (titleText != null)
            {
                titleText.text = "HOÀN THÀNH VÒNG ĐẤU!";
            }

            if (roundText != null)
            {
                roundText.text = $"Vòng {completedRoundNumber} / {totalRounds} hoàn tất";
            }

            if (defeatedValueText != null)
            {
                defeatedValueText.text = RoundResultTracker.EnemiesDefeated.ToString();
            }

            if (goldValueText != null)
            {
                goldValueText.text = $"+{RoundResultTracker.TotalGoldEarned}";
            }

            if (goldDetailText != null)
            {
                goldDetailText.text = $"Hạ quái +{RoundResultTracker.KillGoldEarned}  |  Lợi tức +{RoundResultTracker.InterestGoldEarned}";
            }

            List<PlaceableUnit> aliveUnits = PlayerUnitManager.Instance != null
                ? PlayerUnitManager.Instance.GetAlivePlacedUnits()
                : new List<PlaceableUnit>();

            if (unitCountValueText != null)
            {
                unitCountValueText.text = aliveUnits.Count.ToString();
            }

            if (unitCountDetailText != null)
            {
                unitCountDetailText.text = aliveUnits.Count == 1 ? "tướng còn trên sân" : "tướng còn trên sân";
            }

            if (unitListTitleText != null)
            {
                unitListTitleText.text = "Máu từng tướng trên sàn đấu";
            }

            RebuildUnitRows(aliveUnits);
        }

        private void RebuildUnitRows(List<PlaceableUnit> aliveUnits)
        {
            if (unitListRoot == null)
            {
                return;
            }

            for (int i = unitListRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(unitListRoot.GetChild(i).gameObject);
            }

            if (aliveUnits.Count == 0)
            {
                TMP_Text emptyText = CreateText("EmptyUnitText", unitListRoot, "Không còn tướng nào trên sân.", 18f, FontStyles.Bold);
                emptyText.color = new Color(0.42f, 0.12f, 0.04f, 1f);
                emptyText.alignment = TextAlignmentOptions.Center;
                Stretch(emptyText.rectTransform);
                return;
            }

            for (int i = 0; i < aliveUnits.Count; i++)
            {
                CreateUnitRow(unitListRoot, aliveUnits[i]);
            }
        }

        private void CreateUnitRow(RectTransform parent, PlaceableUnit unit)
        {
            RectTransform row = CreateRect("UnitHealthRow", parent);
            ApplyLayoutSize(row.gameObject, 0f, 20f);
            LayoutElement rowLayoutElement = row.GetComponent<LayoutElement>();
            rowLayoutElement.minHeight = 20f;
            rowLayoutElement.flexibleHeight = 0f;

            Image rowImage = row.gameObject.AddComponent<Image>();
            rowImage.color = new Color(0.18f, 0.08f, 0.035f, 0.88f);

            Outline rowOutline = row.gameObject.AddComponent<Outline>();
            rowOutline.effectColor = new Color(1f, 0.78f, 0.36f, 0.28f);
            rowOutline.effectDistance = new Vector2(1f, -1f);

            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 2, 2);
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            TMP_Text nameText = CreateText("UnitName", row, GetHeroDisplayName(unit), 12f, FontStyles.Bold);
            nameText.color = Gold;
            nameText.alignment = TextAlignmentOptions.MidlineLeft;
            ApplyLayoutSize(nameText.gameObject, 170f, 18f);
            ApplyReadableTextStyle(nameText, Color.black, 0.12f);

            Health health = unit != null ? unit.GetComponent<Health>() : null;
            int currentHealth = health != null ? health.CurrentHealth : 0;
            int maxHealth = health != null ? health.MaxHealth : 0;
            float healthPercent = maxHealth > 0 ? Mathf.Clamp01((float)currentHealth / maxHealth) : 0f;

            RectTransform bar = CreateRect("HealthBar", row);
            bar.sizeDelta = new Vector2(0f, 14f);
            ApplyLayoutSize(bar.gameObject, 0f, 14f);
            LayoutElement barLayoutElement = bar.GetComponent<LayoutElement>();
            barLayoutElement.minHeight = 14f;
            barLayoutElement.flexibleHeight = 0f;

            Image barBack = bar.gameObject.AddComponent<Image>();
            barBack.color = new Color(0.08f, 0.03f, 0.02f, 1f);

            RectTransform fill = CreateRect("Fill", bar);
            fill.anchorMin = new Vector2(0f, 0f);
            fill.anchorMax = new Vector2(healthPercent, 1f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;

            Image fillImage = fill.gameObject.AddComponent<Image>();
            fillImage.color = Color.Lerp(new Color(0.78f, 0.12f, 0.08f, 1f), new Color(0.22f, 0.78f, 0.22f, 1f), healthPercent);

            TMP_Text hpText = CreateText("HealthText", bar, $"{currentHealth}/{maxHealth}", 12f, FontStyles.Bold);
            hpText.color = Color.white;
            hpText.alignment = TextAlignmentOptions.Center;
            Stretch(hpText.rectTransform);
            ApplyReadableTextStyle(hpText, Color.black, 0.14f);
        }

        private static string GetHeroDisplayName(PlaceableUnit unit)
        {
            if (unit == null || string.IsNullOrWhiteSpace(unit.unitName))
            {
                return "Tướng";
            }

            switch (unit.unitName)
            {
                case "Archer":
                    return "An Dương Vương";
                case "Tank":
                    return "Chử Đồng Tử";
                case "Knight":
                    return "Sơn Tinh";
                default:
                    return unit.unitName;
            }
        }

        private void ContinueToPreparation()
        {
            SetVisible(false);

            if (GamePhaseManager.Instance != null)
            {
                GamePhaseManager.Instance.StartPreparation();
            }
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

        private void BuildScreen()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 4900;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960f, 540f);
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();
            rootGroup = gameObject.AddComponent<CanvasGroup>();

            RectTransform root = gameObject.GetComponent<RectTransform>();
            Stretch(root);

            CreateBackground(root);

            RectTransform banner = CreatePanel("Banner", root, new Color(0.58f, 0.25f, 0.1f, 0.98f), new Vector2(560f, 82f), new Vector2(0f, 188f));
            titleText = CreateText("Title", banner, "HOÀN THÀNH VÒNG ĐẤU!", 32f, FontStyles.Bold);
            titleText.color = Gold;
            titleText.alignment = TextAlignmentOptions.Center;
            ApplyReadableTextStyle(titleText, new Color(0.12f, 0.035f, 0f, 1f), 0.2f);
            Stretch(titleText.rectTransform);

            RectTransform subtitle = CreatePanel("RoundSubtitle", root, DeepRed, new Vector2(230f, 34f), new Vector2(0f, 138f));
            roundText = CreateText("RoundText", subtitle, "Vòng hoàn tất", 18f, FontStyles.Bold);
            roundText.color = new Color(1f, 0.92f, 0.68f, 1f);
            roundText.alignment = TextAlignmentOptions.Center;
            ApplyReadableTextStyle(roundText, Color.black, 0.12f);
            Stretch(roundText.rectTransform);

            RectTransform mainPanel = CreatePanel("RoundSummaryPanel", root, Wood, new Vector2(680f, 320f), new Vector2(0f, -8f));
            VerticalLayoutGroup panelLayout = mainPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            panelLayout.padding = new RectOffset(22, 22, 18, 18);
            panelLayout.spacing = 14f;
            panelLayout.childAlignment = TextAnchor.UpperCenter;
            panelLayout.childControlWidth = true;
            panelLayout.childForceExpandWidth = true;
            panelLayout.childForceExpandHeight = false;

            RectTransform statsRow = CreateRect("StatsRow", mainPanel);
            ApplyLayoutSize(statsRow.gameObject, 0f, 92f);
            HorizontalLayoutGroup statsLayout = statsRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            statsLayout.spacing = 14f;
            statsLayout.childAlignment = TextAnchor.MiddleCenter;
            statsLayout.childControlWidth = true;
            statsLayout.childForceExpandWidth = true;

            TMP_Text ignoredDetailText;
            CreateStatCard(statsRow, "Kẻ địch hạ gục", out defeatedValueText, out ignoredDetailText, "0");
            CreateStatCard(statsRow, "Linh khí thu thập được", out goldValueText, out goldDetailText, "+0");
            CreateStatCard(statsRow, "Tướng còn lại", out unitCountValueText, out unitCountDetailText, "0");

            RectTransform listPanel = CreateRect("UnitListPanel", mainPanel);
            ApplyLayoutSize(listPanel.gameObject, 0f, 170f);
            Image listImage = listPanel.gameObject.AddComponent<Image>();
            listImage.color = Parchment;

            VerticalLayoutGroup listLayout = listPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            listLayout.padding = new RectOffset(16, 16, 10, 12);
            listLayout.spacing = 8f;
            listLayout.childControlWidth = true;
            listLayout.childForceExpandWidth = true;
            listLayout.childForceExpandHeight = false;

            unitListTitleText = CreateText("UnitListTitle", listPanel, "Máu từng tướng trên sàn đấu", 17f, FontStyles.Bold);
            unitListTitleText.color = DeepRed;
            unitListTitleText.alignment = TextAlignmentOptions.Center;
            ApplyLayoutSize(unitListTitleText.gameObject, 0f, 24f);

            unitListRoot = CreateRect("UnitRows", listPanel);
            ApplyLayoutSize(unitListRoot.gameObject, 0f, 112f);
            VerticalLayoutGroup rowsLayout = unitListRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            rowsLayout.spacing = 3f;
            rowsLayout.childControlWidth = true;
            rowsLayout.childForceExpandWidth = true;
            rowsLayout.childForceExpandHeight = false;

            Button nextButton = CreateButton(root, "VÒNG TIẾP THEO", new Vector2(0f, -206f));
            nextButton.onClick.AddListener(ContinueToPreparation);
        }

        private void CreateBackground(RectTransform root)
        {
            Image background = CreateImage("Background", root, new Color(0.75f, 0.51f, 0.28f, 1f));
            Stretch(background.rectTransform);

            Sprite bgSprite = LoadSprite(Dev5RuntimeUIArt.RoundCompleteBackground);
            if (bgSprite != null)
            {
                background.sprite = bgSprite;
                background.color = Color.white;
                background.preserveAspect = false;
            }

            Image wash = CreateImage("ParchmentWash", root, new Color(0.95f, 0.77f, 0.42f, 0.14f));
            Stretch(wash.rectTransform);
            wash.raycastTarget = false;

            Image dim = CreateImage("SoftDim", root, new Color(0.18f, 0.06f, 0.02f, 0.12f));
            Stretch(dim.rectTransform);
            dim.raycastTarget = false;
        }

        private void CreateStatCard(RectTransform parent, string label, out TMP_Text valueText, out TMP_Text detailText, string defaultValue)
        {
            RectTransform card = CreatePanel(label, parent, new Color(0.34f, 0.16f, 0.08f, 0.94f), new Vector2(0f, 88f), Vector2.zero);
            LayoutElement layoutElement = card.gameObject.AddComponent<LayoutElement>();
            layoutElement.flexibleWidth = 1f;
            layoutElement.preferredHeight = 88f;

            VerticalLayoutGroup layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.spacing = 2f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            TMP_Text labelText = CreateText("Label", card, label, 13f, FontStyles.Bold);
            labelText.color = new Color(1f, 0.86f, 0.56f, 1f);
            labelText.alignment = TextAlignmentOptions.Center;
            ApplyLayoutSize(labelText.gameObject, 0f, 18f);

            valueText = CreateText("Value", card, defaultValue, 28f, FontStyles.Bold);
            valueText.color = Color.white;
            valueText.alignment = TextAlignmentOptions.Center;
            ApplyReadableTextStyle(valueText, Color.black, 0.16f);
            ApplyLayoutSize(valueText.gameObject, 0f, 34f);

            detailText = CreateText("Detail", card, "", 11f, FontStyles.Bold);
            detailText.color = Gold;
            detailText.alignment = TextAlignmentOptions.Center;
            ApplyLayoutSize(detailText.gameObject, 0f, 18f);
        }

        private Button CreateButton(RectTransform parent, string label, Vector2 anchoredPosition)
        {
            RectTransform rect = CreateRect("NextRoundButton", parent);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(370f, 64f);
            rect.anchoredPosition = anchoredPosition;

            Image image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0.38f, 0.095f, 0.035f, 0.98f);

            Outline outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = Gold;
            outline.effectDistance = new Vector2(2f, -2f);

            RectTransform inner = CreateRect("InnerPlate", rect);
            inner.anchorMin = Vector2.zero;
            inner.anchorMax = Vector2.one;
            inner.offsetMin = new Vector2(10f, 9f);
            inner.offsetMax = new Vector2(-10f, -9f);

            Image innerImage = inner.gameObject.AddComponent<Image>();
            innerImage.color = new Color(0.62f, 0.22f, 0.09f, 0.94f);
            innerImage.raycastTarget = false;

            Outline innerOutline = inner.gameObject.AddComponent<Outline>();
            innerOutline.effectColor = new Color(1f, 0.82f, 0.38f, 0.55f);
            innerOutline.effectDistance = new Vector2(1f, -1f);

            Button button = rect.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.9f, 0.55f, 1f);
            colors.pressedColor = new Color(0.78f, 0.44f, 0.2f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            TMP_Text text = CreateText("Label", rect, label, 24f, FontStyles.Bold);
            text.color = new Color(1f, 0.86f, 0.38f, 1f);
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.outlineColor = new Color(0.12f, 0.035f, 0f, 1f);
            text.outlineWidth = 0.12f;
            Stretch(text.rectTransform);
            text.rectTransform.offsetMin = new Vector2(22f, 6f);
            text.rectTransform.offsetMax = new Vector2(-22f, -6f);

            return button;
        }

        private static RectTransform CreatePanel(string name, RectTransform parent, Color color, Vector2 size, Vector2 anchoredPosition)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;

            Outline outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.78f, 0.36f, 0.72f);
            outline.effectDistance = new Vector2(2f, -2f);

            return rect;
        }

        private static TMP_Text CreateText(string name, RectTransform parent, string text, float size, FontStyles style)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            TMP_FontAsset font = LoadFont();
            if (font != null)
            {
                tmp.font = font;
            }

            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = Mathf.Max(8f, size * 0.62f);
            tmp.fontSizeMax = size;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;

            return tmp;
        }

        private static void ApplyReadableTextStyle(TMP_Text text, Color outlineColor, float outlineWidth)
        {
            text.outlineColor = outlineColor;
            text.outlineWidth = outlineWidth;

            Shadow shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.78f);
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

        private static void ApplyLayoutSize(GameObject gameObject, float preferredWidth, float preferredHeight)
        {
            LayoutElement layout = gameObject.GetComponent<LayoutElement>();
            if (layout == null)
            {
                layout = gameObject.AddComponent<LayoutElement>();
            }

            if (preferredWidth > 0f)
            {
                layout.preferredWidth = preferredWidth;
            }
            else
            {
                layout.flexibleWidth = 1f;
            }

            layout.preferredHeight = preferredHeight;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static TMP_FontAsset LoadFont()
        {
            return Dev5RuntimeUIArt.LoadMenuFont();
        }

        private static Sprite LoadSprite(string resourcePath)
        {
            return Dev5RuntimeUIArt.LoadSprite(resourcePath);
        }
    }
}
