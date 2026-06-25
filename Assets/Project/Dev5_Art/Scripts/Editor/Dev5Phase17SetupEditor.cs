#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using HonVietThuThanh.Dev5;

namespace HonVietThuThanh.Dev5Editor
{
    /// <summary>
    /// Editor script tự động thiết lập Phase 17: Game Config & Clean Prototype Setup.
    /// Chạy qua thanh menu: Dev5 / Setup Phase 17 - Game Config and Clean Prefabs
    /// </summary>
    public static class Dev5Phase17SetupEditor
    {
        private const string SCENE_REQUIRED = "Scene_Dev5_Art";
        
        // Paths to prefabs
        private const string KNIGHT_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Knight_Unit_Prefab.prefab";
        private const string ARCHER_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Archer_Unit_Prefab.prefab";
        private const string TANK_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Tank_Unit_Prefab.prefab";
        
        private const string GOBLIN_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Enemies/Goblin_Enemy_Prefab.prefab";
        private const string ORC_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Enemies/Orc_Enemy_Prefab.prefab";
        private const string ENEMY_ARCHER_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Enemies/EnemyArcher_Prefab.prefab";

        [MenuItem("Dev5/Setup Phase 17 - Game Config and Clean Prefabs")]
        public static void SetupPhase17()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.name.Equals(SCENE_REQUIRED, System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"[Phase17Setup] Đang chạy trên scene '{activeScene.name}' thay vì '{SCENE_REQUIRED}'.");
            }

            Undo.SetCurrentGroupName("Phase 17 Game Config and Prefabs");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Tạo và cấu hình GameConfig GameObject trong Scene
            EnsureGameConfig();

            // 2. Tái cấu trúc các Prefab Hero (Unit)
            RestructurePrefab(KNIGHT_PREFAB_PATH, isUnit: true);
            RestructurePrefab(ARCHER_PREFAB_PATH, isUnit: true);
            RestructurePrefab(TANK_PREFAB_PATH, isUnit: true);

            // 3. Tái cấu trúc các Prefab Enemy
            RestructurePrefab(GOBLIN_PREFAB_PATH, isUnit: false);
            RestructurePrefab(ORC_PREFAB_PATH, isUnit: false);
            RestructurePrefab(ENEMY_ARCHER_PREFAB_PATH, isUnit: false);

            Undo.CollapseUndoOperations(undoGroup);

            // Đánh dấu Scene thay đổi và lưu lại
            EditorSceneManager.MarkSceneDirty(activeScene);
            bool isSaved = EditorSceneManager.SaveScene(activeScene);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Phase17Setup] ✅ Setup Phase 17 hoàn tất! Scene lưu: {(isSaved ? "Thành công" : "Thất bại")}");
        }

        private static void EnsureGameConfig()
        {
            var existing = Object.FindAnyObjectByType<GameConfig>();
            if (existing != null)
            {
                Debug.Log("[Phase17Setup] GameConfig đã tồn tại trong Scene.");
                return;
            }

            GameObject go = new GameObject("GameConfig");
            Undo.RegisterCreatedObjectUndo(go, "Create GameConfig");
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            GameConfig config = go.AddComponent<GameConfig>();
            config.debugMode = false; // Mặc định là normal mode
            config.startingGoldNormal = 10;
            config.startingGoldDebug = 1000;
            config.enableDebugHotkeys = true;
            config.enableAutoSetupTools = false;

            Debug.Log("[Phase17Setup] Đã tạo thành công GameObject 'GameConfig' trong Scene.");
        }

        private static void RestructurePrefab(string path, bool isUnit)
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefabAsset == null)
            {
                Debug.LogWarning($"[Phase17Setup] Không tìm thấy prefab tại đường dẫn: {path}");
                return;
            }

            // Tạo instance tạm trong Scene để chỉnh sửa
            GameObject instance = PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;
            if (instance == null)
            {
                Debug.LogError($"[Phase17Setup] Không thể tạo instance của prefab '{prefabAsset.name}'!");
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

            // Di chuyển MeshFilter nếu đang ở root
            MeshFilter rootMf = instance.GetComponent<MeshFilter>();
            if (rootMf != null)
            {
                MeshFilter childMf = visualChild.gameObject.GetComponent<MeshFilter>();
                if (childMf == null) childMf = visualChild.gameObject.AddComponent<MeshFilter>();
                childMf.sharedMesh = rootMf.sharedMesh;
                Object.DestroyImmediate(rootMf, true);
            }

            // Di chuyển MeshRenderer nếu đang ở root
            MeshRenderer rootMr = instance.GetComponent<MeshRenderer>();
            if (rootMr != null)
            {
                MeshRenderer childMr = visualChild.gameObject.GetComponent<MeshRenderer>();
                if (childMr == null) childMr = visualChild.gameObject.AddComponent<MeshRenderer>();
                childMr.sharedMaterials = rootMr.sharedMaterials;
                Object.DestroyImmediate(rootMr, true);
            }

            // 2. Xử lý UI child
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

            // Di chuyển HealthBar nếu đang ở root
            Transform rootHealthBar = instance.transform.Find("HealthBar");
            if (rootHealthBar == null) rootHealthBar = instance.transform.Find("HealthBarCanvas");

            if (rootHealthBar != null)
            {
                rootHealthBar.SetParent(uiChild, false);
                rootHealthBar.name = "HealthBar";
            }
            else
            {
                Transform uiHealthBar = uiChild.Find("HealthBar");
                if (uiHealthBar == null)
                {
                    GameObject hbGo = new GameObject("HealthBar");
                    hbGo.transform.SetParent(uiChild, false);
                    hbGo.transform.localPosition = new Vector3(0f, 2.2f, 0f);
                    hbGo.transform.localRotation = Quaternion.identity;
                }
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
                else
                {
                    Transform uiStarText = uiChild.Find("StarText");
                    if (uiStarText == null)
                    {
                        GameObject stGo = new GameObject("StarText");
                        stGo.transform.SetParent(uiChild, false);
                        stGo.transform.localPosition = new Vector3(0f, 2.5f, 0f);
                        stGo.transform.localRotation = Quaternion.identity;
                    }
                }
            }

            // Lưu thay đổi đè lên prefab cũ
            PrefabUtility.SaveAsPrefabAsset(instance, path);

            // Huỷ instance tạm trong scene
            Object.DestroyImmediate(instance);

            Debug.Log($"[Phase17Setup] Đã tái cấu trúc thành công prefab tại: {path}");
        }
    }
}
#endif
