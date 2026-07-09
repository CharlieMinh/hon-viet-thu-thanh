#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HonVietThuThanh.Dev5;

namespace HonVietThuThanh.Dev5Editor
{
    /// <summary>
    /// Editor script tự động thiết lập Phase 16: Tạo UI Inspect Panel và gắn RightClickInspectController vào Scene.
    /// Tự động chạy khi mở Unity / Domain Reload hoặc chạy thủ công qua thanh menu: Dev5 / Setup Phase 16 - Right Click Inspect Panel
    /// </summary>
    // [InitializeOnLoad]
    public static class Dev5Phase16SetupEditor
    {
        private const string SCENE_REQUIRED = "Scene_Dev5_Art";

        static Dev5Phase16SetupEditor()
        {
            // EditorApplication.delayCall += RunSetupOnLoad;
        }

        private static void RunSetupOnLoad()
        {
            // Kiểm tra xem đã tạo UI Inspect Panel chưa, nếu chưa tự động thiết lập
            GameObject canvasGo = GameObject.Find("ShopCanvas");
            if (canvasGo != null && canvasGo.transform.Find("InspectPanel") == null)
            {
                Debug.Log("[Phase16Setup] Phát hiện chưa thiết lập UI Inspect Panel cho Phase 16. Bắt đầu cấu hình...");
                SetupPhase16();
            }
        }

        [MenuItem("Dev5/Setup Phase 16 - Right Click Inspect Panel")]
        public static void SetupPhase16()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.name.Equals(SCENE_REQUIRED, System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"[Phase16Setup] Đang chạy trên scene '{activeScene.name}' thay vì '{SCENE_REQUIRED}'.");
            }

            // 1. Tìm ShopCanvas chính trong Scene
            GameObject shopCanvas = GameObject.Find("ShopCanvas");
            if (shopCanvas == null)
            {
                Canvas canvasObj = Object.FindFirstObjectByType<Canvas>();
                if (canvasObj != null)
                {
                    shopCanvas = canvasObj.gameObject;
                }
            }

            if (shopCanvas == null)
            {
                Debug.LogError("[Phase16Setup] Không tìm thấy Canvas hoặc 'ShopCanvas' trong Scene để thiết lập UI!");
                return;
            }

            Undo.SetCurrentGroupName("Phase 16 UI Setup");
            int undoGroup = Undo.GetCurrentGroup();

            // 2. Dựng InspectPanel
            Transform existingInspectPanel = shopCanvas.transform.Find("InspectPanel");
            InspectPanel panelScript = null;

            if (existingInspectPanel == null)
            {
                GameObject panelGo = new GameObject("InspectPanel");
                Undo.RegisterCreatedObjectUndo(panelGo, "Create InspectPanel UI");
                panelGo.transform.SetParent(shopCanvas.transform, false);

                // Nền đen xám trong suốt
                Image bg = panelGo.AddComponent<Image>();
                bg.color = new Color(0.08f, 0.08f, 0.1f, 0.9f);

                RectTransform rect = panelGo.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(1f, 0.5f);
                rect.anchorMax = new Vector2(1f, 0.5f);
                rect.pivot = new Vector2(1f, 0.5f);
                rect.sizeDelta = new Vector2(300f, 250f);
                rect.anchoredPosition = new Vector2(-20f, 100f); // Ở lề phải màn hình, nằm trên UnitInfoPanel một chút

                // A. Tiêu đề Inspect (Tên + Cấp Sao)
                GameObject titleGo = new GameObject("InspectTitleText");
                titleGo.transform.SetParent(panelGo.transform, false);
                TextMeshProUGUI titleText = titleGo.AddComponent<TextMeshProUGUI>();
                titleText.fontSize = 18f;
                titleText.fontStyle = FontStyles.Bold;
                titleText.color = Color.yellow;
                titleText.alignment = TextAlignmentOptions.Left;

                RectTransform titleRect = titleGo.GetComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0f, 1f);
                titleRect.anchorMax = new Vector2(1f, 1f);
                titleRect.pivot = new Vector2(0.5f, 1f);
                titleRect.offsetMin = new Vector2(12f, -35f);
                titleRect.offsetMax = new Vector2(-12f, -10f);

                // B. Chỉ số chi tiết (Stats Text)
                GameObject statsGo = new GameObject("InspectStatsText");
                statsGo.transform.SetParent(panelGo.transform, false);
                TextMeshProUGUI statsText = statsGo.AddComponent<TextMeshProUGUI>();
                statsText.fontSize = 14f;
                statsText.color = Color.white;
                statsText.alignment = TextAlignmentOptions.Left;

                RectTransform statsRect = statsGo.GetComponent<RectTransform>();
                statsRect.anchorMin = Vector2.zero;
                statsRect.anchorMax = Vector2.one;
                statsRect.pivot = new Vector2(0.5f, 0.5f);
                statsRect.offsetMin = new Vector2(12f, 10f);
                statsRect.offsetMax = new Vector2(-12f, -40f);

                // C. Nút đóng (Close Button)
                GameObject closeBtnGo = new GameObject("CloseButton");
                closeBtnGo.transform.SetParent(panelGo.transform, false);
                Image btnImg = closeBtnGo.AddComponent<Image>();
                btnImg.color = new Color(0.35f, 0.1f, 0.1f, 0.85f);
                Button closeBtn = closeBtnGo.AddComponent<Button>();

                RectTransform btnRect = closeBtnGo.GetComponent<RectTransform>();
                btnRect.anchorMin = new Vector2(1f, 1f);
                btnRect.anchorMax = new Vector2(1f, 1f);
                btnRect.pivot = new Vector2(1f, 1f);
                btnRect.sizeDelta = new Vector2(25f, 25f);
                btnRect.anchoredPosition = new Vector2(-5f, -5f);

                GameObject btnTextGo = new GameObject("Text");
                btnTextGo.transform.SetParent(closeBtnGo.transform, false);
                TextMeshProUGUI btnText = btnTextGo.AddComponent<TextMeshProUGUI>();
                btnText.text = "X";
                btnText.fontSize = 12f;
                btnText.fontStyle = FontStyles.Bold;
                btnText.color = Color.white;
                btnText.alignment = TextAlignmentOptions.Center;

                RectTransform btnTextRect = btnTextGo.GetComponent<RectTransform>();
                btnTextRect.anchorMin = Vector2.zero;
                btnTextRect.anchorMax = Vector2.one;
                btnTextRect.offsetMin = Vector2.zero;
                btnTextRect.offsetMax = Vector2.zero;

                // Gắn script quản lý InspectPanel
                panelScript = panelGo.AddComponent<InspectPanel>();
                panelScript.panelParent = panelGo;
                panelScript.titleText = titleText;
                panelScript.statsText = statsText;
                panelScript.closeButton = closeBtn;

                Debug.Log("[Phase16Setup] Đã dựng thành công UI panel 'InspectPanel' dưới ShopCanvas.");
            }
            else
            {
                panelScript = existingInspectPanel.GetComponent<InspectPanel>();
                Debug.Log("[Phase16Setup] UnitInspectPanel đã tồn tại trong Canvas.");
            }

            // 3. Tạo GameObject RightClickInspectController
            GameObject controllerGo = GameObject.Find("RightClickInspectController");
            if (controllerGo == null)
            {
                controllerGo = new GameObject("RightClickInspectController");
                Undo.RegisterCreatedObjectUndo(controllerGo, "Create RightClickInspectController");
            }

            RightClickInspectController inspectScript = controllerGo.GetComponent<RightClickInspectController>();
            if (inspectScript == null)
            {
                controllerGo.AddComponent<RightClickInspectController>();
            }

            Undo.CollapseUndoOperations(undoGroup);

            // Lưu Scene và Assets
            EditorSceneManager.MarkSceneDirty(activeScene);
            bool isSaved = EditorSceneManager.SaveScene(activeScene);
            AssetDatabase.SaveAssets();

            Debug.Log($"[Phase16Setup] ✅ Thiết lập thành công Right Click Inspect Controller. Scene lưu: {(isSaved ? "Thành công" : "Thất bại")}");
        }
    }
}
#endif
