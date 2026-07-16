using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace HonVietThuThanh.EditorTools
{
    public static class StoryUiShellBuilder
    {
        private const string MenuPath = "Tools/Hon Viet/Create Story UI Shell";
        private const string StoryRootName = "StoryRoot";

        [MenuItem(MenuPath)]
        public static void CreateStoryUiShell()
        {
            Canvas canvas = GetSelectedCanvas();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog(
                    "Create Story UI Shell",
                    "Hãy chọn đúng một Canvas trong Hierarchy trước khi chạy tool.",
                    "OK");
                return;
            }

            if (canvas.transform.Find(StoryRootName) != null)
            {
                EditorUtility.DisplayDialog(
                    "Create Story UI Shell",
                    $"Canvas '{canvas.name}' đã có '{StoryRootName}'. Tool sẽ không tạo trùng.",
                    "OK");
                return;
            }

            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Create Story UI Shell");

            RectTransform root = CreateRect(StoryRootName, canvas.transform);
            Stretch(root);

            RectTransform blackScreenPanel = CreatePanel("BlackScreenPanel", root, Color.black);
            Stretch(blackScreenPanel);
            CreateBlackScreenContent(blackScreenPanel);
            blackScreenPanel.gameObject.SetActive(false);

            RectTransform dialogueOverlay = CreateRect("DialogueOverlay", root);
            Stretch(dialogueOverlay);
            CreateDialogueOverlayContent(dialogueOverlay);
            dialogueOverlay.gameObject.SetActive(false);

            RectTransform unlockPanel = CreatePanel("UnlockPanel", root, new Color(0.08f, 0.06f, 0.045f, 0.96f));
            unlockPanel.anchorMin = new Vector2(0.5f, 0.5f);
            unlockPanel.anchorMax = new Vector2(0.5f, 0.5f);
            unlockPanel.pivot = new Vector2(0.5f, 0.5f);
            unlockPanel.sizeDelta = new Vector2(620f, 320f);
            unlockPanel.anchoredPosition = Vector2.zero;
            CreateUnlockPanelContent(unlockPanel);
            unlockPanel.gameObject.SetActive(false);

            Selection.activeGameObject = root.gameObject;
            Undo.CollapseUndoOperations(undoGroup);

            EditorUtility.DisplayDialog(
                "Create Story UI Shell",
                $"Đã tạo '{StoryRootName}' dưới Canvas '{canvas.name}'.",
                "OK");
        }

        [MenuItem(MenuPath, true)]
        public static bool ValidateCreateStoryUiShell()
        {
            return true;
        }

        private static Canvas GetSelectedCanvas()
        {
            if (Selection.gameObjects == null || Selection.gameObjects.Length != 1)
            {
                return null;
            }

            return Selection.gameObjects[0].GetComponent<Canvas>();
        }

        private static void CreateBlackScreenContent(RectTransform parent)
        {
            TMP_Text storyText = CreateText("StoryText", parent, "Story text preview...", 34f, TextAlignmentOptions.Center);
            storyText.color = Color.white;
            storyText.enableWordWrapping = true;
            SetRect(storyText.rectTransform, new Vector2(0.18f, 0.36f), new Vector2(0.82f, 0.68f), Vector2.zero, Vector2.zero);

            Button continueButton = CreateButton("ContinueButton", parent, "Tiếp tục", new Vector2(150f, 52f));
            SetRect(continueButton.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(150f, 52f), new Vector2(-100f, 60f));

            Button skipButton = CreateButton("SkipButton", parent, "Bỏ qua", new Vector2(130f, 52f));
            SetRect(skipButton.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(130f, 52f), new Vector2(-260f, 60f));
        }

        private static void CreateDialogueOverlayContent(RectTransform parent)
        {
            RectTransform darkBackground = CreatePanel("DarkBackground", parent, new Color(0f, 0f, 0f, 0.58f));
            Stretch(darkBackground);

            RectTransform portraitLeft = CreatePanel("PortraitLeft", parent, new Color(1f, 1f, 1f, 0.18f));
            SetRect(portraitLeft, new Vector2(0f, 0.28f), new Vector2(0f, 0.78f), new Vector2(250f, 360f), new Vector2(170f, 0f));

            RectTransform portraitRight = CreatePanel("PortraitRight", parent, new Color(1f, 1f, 1f, 0.18f));
            SetRect(portraitRight, new Vector2(1f, 0.28f), new Vector2(1f, 0.78f), new Vector2(250f, 360f), new Vector2(-170f, 0f));

            RectTransform dialogueBox = CreatePanel("DialogueBox", parent, new Color(0.06f, 0.045f, 0.04f, 0.94f));
            SetRect(dialogueBox, new Vector2(0.08f, 0f), new Vector2(0.92f, 0f), new Vector2(0f, 210f), new Vector2(0f, 125f));

            TMP_Text speakerNameText = CreateText("SpeakerNameText", dialogueBox, "Tên người nói", 26f, TextAlignmentOptions.Left);
            speakerNameText.color = new Color(1f, 0.82f, 0.38f, 1f);
            SetRect(speakerNameText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(-56f, 44f), new Vector2(28f, -30f));

            TMP_Text dialogueText = CreateText("DialogueText", dialogueBox, "Nội dung lời thoại...", 24f, TextAlignmentOptions.TopLeft);
            dialogueText.color = Color.white;
            dialogueText.enableWordWrapping = true;
            SetRect(dialogueText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(-56f, -80f), new Vector2(28f, -64f));

            Button continueButton = CreateButton("ContinueButton", parent, "Tiếp tục", new Vector2(150f, 52f));
            SetRect(continueButton.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(150f, 52f), new Vector2(-100f, 34f));

            Button skipButton = CreateButton("SkipButton", parent, "Bỏ qua", new Vector2(130f, 52f));
            SetRect(skipButton.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(130f, 52f), new Vector2(-260f, 34f));
        }

        private static void CreateUnlockPanelContent(RectTransform parent)
        {
            RectTransform heroPortrait = CreatePanel("HeroPortrait", parent, new Color(1f, 1f, 1f, 0.18f));
            SetRect(heroPortrait, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(190f, 230f), new Vector2(130f, 0f));

            TMP_Text titleText = CreateText("TitleText", parent, "ĐÃ MỞ KHÓA", 28f, TextAlignmentOptions.Left);
            titleText.color = new Color(1f, 0.82f, 0.38f, 1f);
            SetRect(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(-280f, 42f), new Vector2(250f, -40f));

            TMP_Text heroNameText = CreateText("HeroNameText", parent, "Tên Linh Tướng", 32f, TextAlignmentOptions.Left);
            heroNameText.color = Color.white;
            SetRect(heroNameText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(-280f, 48f), new Vector2(250f, -88f));

            TMP_Text roleText = CreateText("RoleText", parent, "Vai trò", 22f, TextAlignmentOptions.Left);
            roleText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            SetRect(roleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(-280f, 34f), new Vector2(250f, -134f));

            TMP_Text descriptionText = CreateText("DescriptionText", parent, "Mô tả ngắn về tướng và gợi ý sử dụng.", 20f, TextAlignmentOptions.TopLeft);
            descriptionText.color = Color.white;
            descriptionText.enableWordWrapping = true;
            SetRect(descriptionText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(-280f, -190f), new Vector2(250f, -170f));

            Button closeButton = CreateButton("CloseButton", parent, "Đóng", new Vector2(130f, 46f));
            SetRect(closeButton.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(130f, 46f), new Vector2(-88f, 42f));
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            Undo.SetTransformParent(go.transform, parent, $"Parent {name}");
            return go.GetComponent<RectTransform>();
        }

        private static RectTransform CreatePanel(string name, Transform parent, Color color)
        {
            RectTransform rect = CreateRect(name, parent);
            Image image = Undo.AddComponent<Image>(rect.gameObject);
            image.color = color;
            image.raycastTarget = true;
            return rect;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 size)
        {
            RectTransform rect = CreatePanel(name, parent, new Color(0.12f, 0.075f, 0.04f, 0.95f));
            rect.sizeDelta = size;

            Button button = Undo.AddComponent<Button>(rect.gameObject);
            Image image = rect.GetComponent<Image>();
            button.targetGraphic = image;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.88f, 0.48f, 1f);
            colors.pressedColor = new Color(0.78f, 0.58f, 0.25f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.55f);
            button.colors = colors;

            TMP_Text labelText = CreateText("Label", rect, label, 20f, TextAlignmentOptions.Center);
            labelText.color = Color.white;
            Stretch(labelText.rectTransform);

            return button;
        }

        private static TMP_Text CreateText(string name, Transform parent, string text, float fontSize, TextAlignmentOptions alignment)
        {
            RectTransform rect = CreateRect(name, parent);
            TextMeshProUGUI tmp = Undo.AddComponent<TextMeshProUGUI>(rect.gameObject);
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            tmp.enableWordWrapping = false;
            return tmp;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Vector2 anchoredPosition)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = anchoredPosition;
        }
    }
}
