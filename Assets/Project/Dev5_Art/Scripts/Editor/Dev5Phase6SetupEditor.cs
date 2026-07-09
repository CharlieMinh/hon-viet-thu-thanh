#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using HonVietThuThanh.Dev5;

namespace HonVietThuThanh.Dev5Editor
{
    /// <summary>
    /// Editor script tự động thiết lập Phase 6: GamePhaseManager, Nút Start Battle UI, và StateText hiển thị.
    /// Chạy qua thanh menu: Dev5 / Setup Phase 6 - Game State & Start Battle Button
    /// </summary>
    public static class Dev5Phase6SetupEditor
    {
        private const string SCENE_REQUIRED = "Scene_Dev5_Art";

        [MenuItem("Dev5/Setup Phase 6 - Game State & Start Battle Button")]
        public static void SetupPhase6()
        {
            // 0. Xác nhận Scene hoạt động
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.name.Equals(SCENE_REQUIRED, System.StringComparison.OrdinalIgnoreCase))
            {
                bool proceed = EditorUtility.DisplayDialog(
                    "Sai Scene hoạt động",
                    $"Chức năng này được tối ưu cho scene '{SCENE_REQUIRED}'.\nScene hiện tại là '{activeScene.name}'.\n\nBạn có muốn tiếp tục?",
                    "Tiếp tục", "Hủy"
                );
                if (!proceed) return;
            }

            // Tìm Canvas chính
            GameObject shopCanvas = GameObject.Find("ShopCanvas");
            if (shopCanvas == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Không tìm thấy GameObject 'ShopCanvas' trong Scene!", "OK");
                return;
            }

            Undo.SetCurrentGroupName("Phase 6 Setup");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Tạo/Cấu hình GameObject GamePhaseManager
            GamePhaseManager phaseManager = EnsureGamePhaseManager();

            // 2. Tạo Text hiển thị State hiện tại
            TextMeshProUGUI stateText = EnsureStateText(shopCanvas.transform);
            if (stateText != null && phaseManager != null)
            {
                var so = new SerializedObject(phaseManager);
                so.FindProperty("stateText").objectReferenceValue = stateText;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // 3. Tạo Nút Start Battle
            EnsureStartBattleButton(shopCanvas.transform);

            Undo.CollapseUndoOperations(undoGroup);

            // Lưu scene lại
            EditorSceneManager.MarkSceneDirty(activeScene);
            bool isSaved = EditorSceneManager.SaveScene(activeScene);

            Debug.Log($"[Phase6Setup] ✅ Setup Phase 6 hoàn tất! Scene lưu: {(isSaved ? "Thành công" : "Thất bại")}");

            EditorUtility.DisplayDialog(
                "Phase 6 Setup Hoàn Tất",
                "Đã tạo/thiết lập thành công:\n" +
                "• GameObject: GamePhaseManager (chứa script điều hướng trạng thái)\n" +
                "• UI Text: StateText (hiển thị trạng thái góc trên màn hình)\n" +
                "• UI Button: StartBattleButton (Nút bắt đầu trận đấu góc dưới bên phải)\n\n" +
                "Nhấn Play để kiểm tra chuyển đổi trạng thái Preparation -> Combat.",
                "OK"
            );
        }

        private static GamePhaseManager EnsureGamePhaseManager()
        {
            var existing = Object.FindAnyObjectByType<GamePhaseManager>();
            if (existing != null)
            {
                Debug.Log("[Phase6Setup] GamePhaseManager đã tồn tại.");
                return existing;
            }

            GameObject go = new GameObject("GamePhaseManager");
            Undo.RegisterCreatedObjectUndo(go, "Create GamePhaseManager");
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            GamePhaseManager phaseManager = go.AddComponent<GamePhaseManager>();
            Debug.Log("[Phase6Setup] Đã tạo GameObject 'GamePhaseManager'.");
            return phaseManager;
        }

        private static TextMeshProUGUI EnsureStateText(Transform parent)
        {
            Transform existing = parent.Find("StateText");
            if (existing != null)
            {
                Debug.Log("[Phase6Setup] StateText đã tồn tại.");
                return existing.GetComponent<TextMeshProUGUI>();
            }

            GameObject go = new GameObject("StateText");
            Undo.RegisterCreatedObjectUndo(go, "Create StateText UI");
            go.transform.SetParent(parent, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f); // Giữa - Trên
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -20f);
            rect.sizeDelta = new Vector2(300f, 50f);

            TextMeshProUGUI tmpText = go.AddComponent<TextMeshProUGUI>();
            tmpText.text = "State: Preparation";
            tmpText.fontSize = 28;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Color.white;

            // Đặt font mặc định cho dễ nhìn
            tmpText.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

            Debug.Log("[Phase6Setup] Đã tạo UI Text 'StateText'.");
            return tmpText;
        }

        private static void EnsureStartBattleButton(Transform parent)
        {
            Transform existing = parent.Find("StartBattleButton");
            if (existing != null)
            {
                Debug.Log("[Phase6Setup] StartBattleButton đã tồn tại.");
                return;
            }

            // Tạo GameObject nút chính
            GameObject buttonGO = new GameObject("StartBattleButton");
            Undo.RegisterCreatedObjectUndo(buttonGO, "Create StartBattleButton UI");
            buttonGO.transform.SetParent(parent, false);

            RectTransform rect = buttonGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f); // Dưới - Phải
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-20f, 20f);
            rect.sizeDelta = new Vector2(180f, 50f);

            // Thêm Image làm nền nút
            Image img = buttonGO.AddComponent<Image>();
            img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            img.type = Image.Type.Sliced;
            img.color = new Color(0.15f, 0.45f, 0.85f, 1f); // Màu xanh dương nổi bật

            // Thêm Button component
            Button btn = buttonGO.AddComponent<Button>();
            btn.targetGraphic = img;

            // Thêm StartBattleButtonHandler để tự đăng ký sự kiện click
            buttonGO.AddComponent<StartBattleButtonHandler>();

            // Tạo GameObject Text con bên trong nút
            GameObject textGO = new GameObject("Text (TMP)");
            textGO.transform.SetParent(buttonGO.transform, false);

            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI btnText = textGO.AddComponent<TextMeshProUGUI>();
            btnText.text = "Start Battle";
            btnText.fontSize = 20;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.color = Color.white;
            btnText.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

            Debug.Log("[Phase6Setup] Đã tạo UI Button 'StartBattleButton'.");
        }
    }
}
#endif
