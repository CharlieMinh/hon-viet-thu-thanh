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
    /// Editor script tự động thiết lập Phase 13: Combat Readability UI (Thanh máu, thông tin cờ, thông báo Vàng).
    /// Chạy qua thanh menu: Dev5 / Setup Phase 13 - Combat Readability UI
    /// </summary>
    public static class Dev5Phase13SetupEditor
    {
        private const string SCENE_REQUIRED = "Scene_Dev5_Art";
        private const string KNIGHT_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Knight_Unit_Prefab.prefab";
        private const string ARCHER_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Archer_Unit_Prefab.prefab";
        private const string TANK_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Tank_Unit_Prefab.prefab";
        private const string ENEMY_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Enemies/Enemy_Test_Prefab.prefab";

        [MenuItem("Dev5/Setup Phase 13 - Combat Readability UI")]
        public static void SetupPhase13()
        {
            // 0. Xác nhận Scene hoạt động
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.name.Equals(SCENE_REQUIRED, System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"[Phase13Setup] Đang chạy trên scene '{activeScene.name}' thay vì '{SCENE_REQUIRED}'.");
            }

            Undo.SetCurrentGroupName("Phase 13 UI Setup");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Thêm HealthBar component vào các Prefabs tướng và quái
            EnsureHealthBarOnPrefab(KNIGHT_PREFAB_PATH, "Knight");
            EnsureHealthBarOnPrefab(ARCHER_PREFAB_PATH, "Archer");
            EnsureHealthBarOnPrefab(TANK_PREFAB_PATH, "Tank");
            EnsureHealthBarOnPrefab(ENEMY_PREFAB_PATH, "Enemy");

            // 2. Thiết lập UI Panels dưới ShopCanvas
            SetupSceneUI();

            Undo.CollapseUndoOperations(undoGroup);

            // Lưu scene
            EditorSceneManager.MarkSceneDirty(activeScene);
            bool isSaved = EditorSceneManager.SaveScene(activeScene);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Phase13Setup] ✅ Setup Phase 13 hoàn tất! Scene lưu: {(isSaved ? "Thành công" : "Thất bại")}");
        }

        private static void EnsureHealthBarOnPrefab(string path, string name)
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefabAsset == null)
            {
                Debug.LogWarning($"[Phase13Setup] Không tìm thấy prefab tại đường dẫn: {path}");
                return;
            }

            // Tạo instance tạm để thêm script
            GameObject instance = PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;
            if (instance == null)
            {
                Debug.LogError($"[Phase13Setup] Không thể tạo instance của prefab '{name}'!");
                return;
            }

            HealthBar hb = instance.GetComponent<HealthBar>();
            if (hb == null)
            {
                hb = instance.AddComponent<HealthBar>();
            }
            hb.health = instance.GetComponent<Health>();

            // Lưu prefab
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);

            Debug.Log($"[Phase13Setup] Đã đính kèm component HealthBar vào prefab '{name}'.");
        }

        private static void SetupSceneUI()
        {
            // Tìm Canvas chính trong Scene
            Canvas shopCanvas = Object.FindAnyObjectByType<Canvas>();
            if (shopCanvas == null || !shopCanvas.name.Contains("Canvas"))
            {
                GameObject canvasGo = GameObject.Find("ShopCanvas");
                if (canvasGo != null)
                {
                    shopCanvas = canvasGo.GetComponent<Canvas>();
                }
            }

            if (shopCanvas == null)
            {
                Debug.LogError("[Phase13Setup] Không tìm thấy Canvas/ShopCanvas trong Scene để dựng UI!");
                return;
            }

            // A. Dựng UnitInfoPanel
            Transform existingPanel = shopCanvas.transform.Find("UnitInfoPanel");
            if (existingPanel == null)
            {
                GameObject panelGo = new GameObject("UnitInfoPanel");
                Undo.RegisterCreatedObjectUndo(panelGo, "Create UnitInfoPanel UI");
                panelGo.transform.SetParent(shopCanvas.transform, false);

                // Thêm Image làm nền panel
                Image bg = panelGo.AddComponent<Image>();
                bg.color = new Color(0.1f, 0.1f, 0.12f, 0.85f); // Xám đen bán trong suốt

                RectTransform rect = panelGo.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(1f, 0f);
                rect.anchorMax = new Vector2(1f, 0f);
                rect.pivot = new Vector2(1f, 0f);
                rect.sizeDelta = new Vector2(250f, 150f);
                rect.anchoredPosition = new Vector2(-20f, 20f); // Lệch góc dưới bên phải

                // Tiêu đề Tên cờ (Name + Star Text)
                GameObject titleGo = new GameObject("UnitNameText");
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
                titleRect.offsetMin = new Vector2(12f, -30f);
                titleRect.offsetMax = new Vector2(-12f, -5f);

                // Chi tiết chỉ số (Stats Text)
                GameObject statsGo = new GameObject("UnitStatsText");
                statsGo.transform.SetParent(panelGo.transform, false);
                TextMeshProUGUI statsText = statsGo.AddComponent<TextMeshProUGUI>();
                statsText.fontSize = 14f;
                statsText.color = Color.white;
                statsText.alignment = TextAlignmentOptions.Left;

                RectTransform statsRect = statsGo.GetComponent<RectTransform>();
                statsRect.anchorMin = new Vector2(0f, 0f);
                statsRect.anchorMax = new Vector2(1f, 1f);
                statsRect.pivot = new Vector2(0.5f, 0.5f);
                statsRect.offsetMin = new Vector2(12f, 5f);
                statsRect.offsetMax = new Vector2(-12f, -35f);

                // Đính kèm component quản lý UI thông tin
                UnitInfoPanel panelScript = panelGo.AddComponent<UnitInfoPanel>();
                panelScript.panelParent = panelGo;
                panelScript.nameText = titleText;
                panelScript.statsText = statsText;

                Debug.Log("[Phase13Setup] Đã tạo thành công UI Panel 'UnitInfoPanel'.");
            }
            else
            {
                Debug.Log("[Phase13Setup] UnitInfoPanel đã tồn tại trong Canvas.");
            }

            // B. Dựng RewardFeedbackText
            Transform existingFeedback = shopCanvas.transform.Find("RewardFeedbackText");
            if (existingFeedback == null)
            {
                GameObject feedbackGo = new GameObject("RewardFeedbackText");
                Undo.RegisterCreatedObjectUndo(feedbackGo, "Create RewardFeedbackText UI");
                feedbackGo.transform.SetParent(shopCanvas.transform, false);

                RectTransform rect = feedbackGo.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.sizeDelta = new Vector2(400f, 60f);
                rect.anchoredPosition = new Vector2(0f, -80f); // Lệch từ đỉnh màn hình xuống

                TextMeshProUGUI textComp = feedbackGo.AddComponent<TextMeshProUGUI>();
                textComp.fontSize = 26f;
                textComp.fontStyle = FontStyles.Bold;
                textComp.alignment = TextAlignmentOptions.Center;

                // Đính kèm component quản lý thông báo Vàng
                RewardFeedbackUI feedbackScript = feedbackGo.AddComponent<RewardFeedbackUI>();
                feedbackScript.rewardText = textComp;

                Debug.Log("[Phase13Setup] Đã tạo thành công UI Text 'RewardFeedbackText'.");
            }
            else
            {
                Debug.Log("[Phase13Setup] RewardFeedbackText đã tồn tại trong Canvas.");
            }
        }
    }
}
#endif
