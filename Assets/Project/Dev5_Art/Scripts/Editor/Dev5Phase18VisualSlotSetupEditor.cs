#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using HonVietThuThanh.Dev5;

namespace HonVietThuThanh.Dev5Editor
{
    /// <summary>
    /// Editor script tự động thiết lập Phase 18: Character Model Visual Slot.
    /// Chạy qua thanh menu: Dev5 / Setup Phase 18 - Character Visual Slots
    /// </summary>
    public static class Dev5Phase18VisualSlotSetupEditor
    {
        private const string SCENE_REQUIRED = "Scene_Dev5_Art";
        
        // Paths to prefabs
        private const string KNIGHT_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Knight_Unit_Prefab.prefab";
        private const string ARCHER_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Archer_Unit_Prefab.prefab";
        private const string TANK_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Tank_Unit_Prefab.prefab";
        
        private const string GOBLIN_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Enemies/Goblin_Enemy_Prefab.prefab";
        private const string ORC_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Enemies/Orc_Enemy_Prefab.prefab";
        private const string ENEMY_ARCHER_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Enemies/EnemyArcher_Prefab.prefab";

        [MenuItem("Dev5/Setup Phase 18 - Character Visual Slots")]
        public static void SetupPhase18VisualSlots()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.name.Equals(SCENE_REQUIRED, System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"[Phase18VisualSlotSetup] Đang chạy trên scene '{activeScene.name}' thay vì '{SCENE_REQUIRED}'.");
            }

            Undo.SetCurrentGroupName("Phase 18 Character Visual Slots Setup");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Tái cấu trúc các Prefab Hero (Unit)
            RestructurePrefabForVisualSlot(KNIGHT_PREFAB_PATH, isUnit: true);
            RestructurePrefabForVisualSlot(ARCHER_PREFAB_PATH, isUnit: true);
            RestructurePrefabForVisualSlot(TANK_PREFAB_PATH, isUnit: true);

            // 2. Tái cấu trúc các Prefab Enemy
            RestructurePrefabForVisualSlot(GOBLIN_PREFAB_PATH, isUnit: false);
            RestructurePrefabForVisualSlot(ORC_PREFAB_PATH, isUnit: false);
            RestructurePrefabForVisualSlot(ENEMY_ARCHER_PREFAB_PATH, isUnit: false);

            Undo.CollapseUndoOperations(undoGroup);

            // Đánh dấu Scene thay đổi và lưu lại
            EditorSceneManager.MarkSceneDirty(activeScene);
            bool isSaved = EditorSceneManager.SaveScene(activeScene);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Phase18VisualSlotSetup] ✅ Setup Phase 18 - Character Visual Slots hoàn tất! Scene lưu: {(isSaved ? "Thành công" : "Thất bại")}");
        }

        private static void RestructurePrefabForVisualSlot(string path, bool isUnit)
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefabAsset == null)
            {
                Debug.LogWarning($"[Phase18VisualSlotSetup] Không tìm thấy prefab tại đường dẫn: {path}");
                return;
            }

            // Tạo instance tạm trong Scene để chỉnh sửa
            GameObject instance = PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;
            if (instance == null)
            {
                Debug.LogError($"[Phase18VisualSlotSetup] Không thể tạo instance của prefab '{prefabAsset.name}'!");
                return;
            }

            // 1. Xử lý Visual child
            Transform visualChild = instance.transform.Find("Visual");
            if (visualChild == null)
            {
                GameObject visualGo = new GameObject("Visual");
                visualGo.transform.SetParent(instance.transform, false);
                visualGo.transform.localPosition = Vector3.zero;
                visualGo.transform.localRotation = Quaternion.identity;
                visualGo.transform.localScale = Vector3.one;
                visualChild = visualGo.transform;
            }

            // 2. Xử lý Placeholder child dưới Visual
            Transform placeholderChild = visualChild.Find("Placeholder");
            if (placeholderChild == null)
            {
                GameObject placeholderGo = new GameObject("Placeholder");
                placeholderGo.transform.SetParent(visualChild, false);
                placeholderGo.transform.localPosition = Vector3.zero;
                placeholderGo.transform.localRotation = Quaternion.identity;
                placeholderGo.transform.localScale = Vector3.one;
                placeholderChild = placeholderGo.transform;
            }

            // 3. Xử lý ModelSlot child dưới Visual
            Transform modelSlotChild = visualChild.Find("ModelSlot");
            if (modelSlotChild == null)
            {
                GameObject modelSlotGo = new GameObject("ModelSlot");
                modelSlotGo.transform.SetParent(visualChild, false);
                modelSlotGo.transform.localPosition = Vector3.zero;
                modelSlotGo.transform.localRotation = Quaternion.identity;
                modelSlotGo.transform.localScale = Vector3.one;
                modelSlotChild = modelSlotGo.transform;
            }
            else
            {
                // Cấu hình lại default transform cho ModelSlot
                modelSlotChild.localPosition = Vector3.zero;
                modelSlotChild.localRotation = Quaternion.identity;
                modelSlotChild.localScale = Vector3.one;
            }

            // 4. Di chuyển MeshFilter/MeshRenderer từ Visual (từ setup Phase 17) sang Placeholder
            MeshFilter mfOnVisual = visualChild.GetComponent<MeshFilter>();
            if (mfOnVisual != null)
            {
                MeshFilter mfOnPlaceholder = placeholderChild.gameObject.GetComponent<MeshFilter>();
                if (mfOnPlaceholder == null)
                {
                    mfOnPlaceholder = placeholderChild.gameObject.AddComponent<MeshFilter>();
                }
                mfOnPlaceholder.sharedMesh = mfOnVisual.sharedMesh;
                Object.DestroyImmediate(mfOnVisual, true);
            }

            MeshRenderer mrOnVisual = visualChild.GetComponent<MeshRenderer>();
            if (mrOnVisual != null)
            {
                MeshRenderer mrOnPlaceholder = placeholderChild.gameObject.GetComponent<MeshRenderer>();
                if (mrOnPlaceholder == null)
                {
                    mrOnPlaceholder = placeholderChild.gameObject.AddComponent<MeshRenderer>();
                }
                mrOnPlaceholder.sharedMaterials = mrOnVisual.sharedMaterials;
                Object.DestroyImmediate(mrOnVisual, true);
            }

            // Di chuyển MeshFilter/MeshRenderer từ Root (nếu có) sang Placeholder
            MeshFilter mfOnRoot = instance.GetComponent<MeshFilter>();
            if (mfOnRoot != null)
            {
                MeshFilter mfOnPlaceholder = placeholderChild.gameObject.GetComponent<MeshFilter>();
                if (mfOnPlaceholder == null)
                {
                    mfOnPlaceholder = placeholderChild.gameObject.AddComponent<MeshFilter>();
                }
                mfOnPlaceholder.sharedMesh = mfOnRoot.sharedMesh;
                Object.DestroyImmediate(mfOnRoot, true);
            }

            MeshRenderer mrOnRoot = instance.GetComponent<MeshRenderer>();
            if (mrOnRoot != null)
            {
                MeshRenderer mrOnPlaceholder = placeholderChild.gameObject.GetComponent<MeshRenderer>();
                if (mrOnPlaceholder == null)
                {
                    mrOnPlaceholder = placeholderChild.gameObject.AddComponent<MeshRenderer>();
                }
                mrOnPlaceholder.sharedMaterials = mrOnRoot.sharedMaterials;
                Object.DestroyImmediate(mrOnRoot, true);
            }

            // 5. Gắn và cấu hình CharacterVisualSlot script ở Root
            CharacterVisualSlot visualSlotComp = instance.GetComponent<CharacterVisualSlot>();
            if (visualSlotComp == null)
            {
                visualSlotComp = instance.AddComponent<CharacterVisualSlot>();
            }
            visualSlotComp.visualRoot = visualChild;
            visualSlotComp.placeholder = placeholderChild.gameObject;
            visualSlotComp.modelSlot = modelSlotChild;

            // 6. Xử lý UI child (giữ nguyên hoặc tái cấu trúc HealthBar/StarText đúng chỗ)
            Transform uiChild = instance.transform.Find("UI");
            if (uiChild == null)
            {
                GameObject uiGo = new GameObject("UI");
                uiGo.transform.SetParent(instance.transform, false);
                uiGo.transform.localPosition = Vector3.zero;
                uiGo.transform.localRotation = Quaternion.identity;
                uiGo.transform.localScale = Vector3.one;
                uiChild = uiGo.transform;
            }

            // Di chuyển HealthBar/HealthBarCanvas nếu đang ở root
            Transform rootHealthBar = instance.transform.Find("HealthBar");
            if (rootHealthBar == null) rootHealthBar = instance.transform.Find("HealthBarCanvas");

            if (rootHealthBar != null)
            {
                rootHealthBar.SetParent(uiChild, false);
                rootHealthBar.name = "HealthBar";
            }

            if (isUnit)
            {
                // Di chuyển StarText nếu đang ở root
                Transform rootStarText = instance.transform.Find("StarText");
                if (rootStarText != null)
                {
                    rootStarText.SetParent(uiChild, false);
                    rootStarText.name = "StarText";
                }
            }

            // 7. Chạy AutoFit để ẩn/hiện placeholder nếu đã có model thật
            visualSlotComp.AutoFitModelIfNeeded();

            // 8. Lưu thay đổi đè lên prefab cũ
            PrefabUtility.SaveAsPrefabAsset(instance, path);

            // Huỷ instance tạm trong scene
            Object.DestroyImmediate(instance);

            Debug.Log($"[Phase18VisualSlotSetup] Đã tái cấu trúc thành công prefab tại: {path}");
        }
    }
}
#endif
