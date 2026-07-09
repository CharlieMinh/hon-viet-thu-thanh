#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using HonVietThuThanh.Dev5;

namespace HonVietThuThanh.Dev5Editor
{
    /// <summary>
    /// Editor script tự động thiết lập Phase 18: Map Art Integration / Environment Setup.
    /// Chạy qua thanh menu: Dev5 / Setup Phase 18 - Environment Setup
    /// </summary>
    public static class Dev5Phase18SetupEditor
    {
        private const string SCENE_REQUIRED = "Scene_Dev5_Art";
        private const string SCENE_PATH = "Assets/Project/Dev5_Art/Scenes/Scene_Dev5_Art.unity";
        private const string GROUND_MAT_PATH = "Assets/Project/Dev5_Art/Materials/M_Ground_Visual.mat";
        private const string BORDER_MAT_PATH = "Assets/Project/Dev5_Art/Materials/M_Border_Visual.mat";
        private const string PROP_ROCK_MAT_PATH = "Assets/Project/Dev5_Art/Materials/M_Prop_Rock.mat";
        private const string PROP_TRUNK_MAT_PATH = "Assets/Project/Dev5_Art/Materials/M_Prop_Trunk.mat";
        private const string PROP_LEAF_MAT_PATH = "Assets/Project/Dev5_Art/Materials/M_Prop_Leaf.mat";

        [MenuItem("Dev5/Setup Phase 18 - Environment Setup")]
        public static void SetupPhase18()
        {
            // 0. Mở Scene hoạt động
            Scene activeScene;
            if (Application.isBatchMode)
            {
                activeScene = EditorSceneManager.OpenScene(SCENE_PATH);
            }
            else
            {
                activeScene = EditorSceneManager.GetActiveScene();
                if (!activeScene.name.Equals(SCENE_REQUIRED, System.StringComparison.OrdinalIgnoreCase))
                {
                    bool proceed = EditorUtility.DisplayDialog(
                        "Sai Scene",
                        $"Hãy mở scene '{SCENE_REQUIRED}' trước.\n\nScene hiện tại: '{activeScene.name}'.\n\nMở scene này để chạy setup?",
                        "Mở và Chạy", "Hủy");
                    if (proceed)
                    {
                        activeScene = EditorSceneManager.OpenScene(SCENE_PATH);
                    }
                    else
                    {
                        return;
                    }
                }
            }

            Undo.SetCurrentGroupName("Phase 18 Environment Setup");
            int undoGroup = Undo.GetCurrentGroup();

            // 1. Tạo các Material cần thiết nếu chưa có
            Material groundMat = GetOrCreateMaterial(GROUND_MAT_PATH, new Color(0.18f, 0.38f, 0.22f)); // Grass Green
            Material borderMat = GetOrCreateMaterial(BORDER_MAT_PATH, new Color(0.25f, 0.25f, 0.28f)); // Dark stone grey
            Material rockMat = GetOrCreateMaterial(PROP_ROCK_MAT_PATH, new Color(0.35f, 0.35f, 0.35f)); // Stone Grey
            Material trunkMat = GetOrCreateMaterial(PROP_TRUNK_MAT_PATH, new Color(0.4f, 0.25f, 0.15f)); // Trunk Brown
            Material leafMat = GetOrCreateMaterial(PROP_LEAF_MAT_PATH, new Color(0.2f, 0.6f, 0.25f)); // Foliage Green

            // 2. Tìm và vô hiệu hoá Map cũ
            DisableOldMap();

            // 3. Tạo Environment hierarchy
            GameObject envGo = EnsureRootGameObject("Environment");
            Transform mapVisualTrans = EnsureChildGameObject(envGo, "Map_Visual").transform;
            Transform propsTrans = EnsureChildGameObject(envGo, "Props").transform;
            Transform vfxTrans = EnsureChildGameObject(envGo, "VFX").transform;

            // 4. Tạo Gameplay hierarchy
            GameObject gameplayGo = EnsureRootGameObject("Gameplay");

            // 5. Tìm BoardManager để lấy toạ độ căn giữa cho Map mới
            BoardManager boardManager = Object.FindFirstObjectByType<BoardManager>();
            Vector3 boardCenter = Vector3.zero;
            if (boardManager != null)
            {
                boardCenter = boardManager.transform.position;
                // Giữ nguyên Y của BoardManager ở 0.52 nếu mặt map ở 0.5 (như hiện tại)
                if (boardManager.transform.position.y != 0.52f)
                {
                    Undo.RecordObject(boardManager.transform, "Adjust BoardManager Y");
                    Vector3 pos = boardManager.transform.position;
                    pos.y = 0.52f;
                    boardManager.transform.position = pos;
                }
            }
            else
            {
                Debug.LogWarning("[Phase18Setup] Không tìm thấy BoardManager trong Scene để làm tâm!");
            }

            // 6. Tạo Ground Plane mới (Y = 0.5) dưới Map_Visual
            GameObject groundPlane = CreateGroundPlane(mapVisualTrans, boardCenter, groundMat);

            // 7. Tạo borders quanh Board (Y = 0.5)
            CreateBorders(mapVisualTrans, boardCenter, borderMat);

            // 8. Tạo một vài props placeholder (rock/tree/pillar)
            CreatePropsPlaceholder(propsTrans, boardCenter, rockMat, trunkMat, leafMat);

            // 9. Di chuyển các object gameplay vào "Gameplay"
            MoveGameplayObjects(gameplayGo);

            // 10. Tạo/Chuẩn hoá "Managers" hierarchy
            GameObject managersGo = EnsureRootGameObject("Managers");
            MoveManagersObjects(managersGo);

            // 11. Di chuyển ShopCanvas lên root
            MoveShopCanvasToRoot();

            // Cấu hình layer Ignore Raycast (Layer 2) cho toàn bộ Map_Visual và Props
            SetLayerRecursive(mapVisualTrans.gameObject, 2);
            SetLayerRecursive(propsTrans.gameObject, 2);
            SetLayerRecursive(vfxTrans.gameObject, 2);

            Undo.CollapseUndoOperations(undoGroup);

            // Đánh dấu Scene thay đổi và lưu lại
            EditorSceneManager.MarkSceneDirty(activeScene);
            bool isSaved = EditorSceneManager.SaveScene(activeScene);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[Phase18Setup] ✅ Setup Phase 18 hoàn tất! Scene lưu: {(isSaved ? "Thành công" : "Thất bại")}");
        }

        private static Material GetOrCreateMaterial(string path, Color color)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
                if (urpLit != null)
                {
                    mat = new Material(urpLit);
                }
                else
                {
                    mat = new Material(Shader.Find("Standard"));
                }
                mat.color = color;
                mat.SetColor("_BaseColor", color);
                AssetDatabase.CreateAsset(mat, path);
                Debug.Log($"[Phase18Setup] Đã tạo Material tại: {path}");
            }
            return mat;
        }

        private static void DisableOldMap()
        {
            string[] possibleNames = { "Map", "Ground", "Cube", "Map_Ground_Debug" };
            foreach (var name in possibleNames)
            {
                GameObject go = GameObject.Find(name);
                // Đảm bảo là root hoặc con của Map
                if (go != null && go.transform.parent == null)
                {
                    Undo.RecordObject(go, "Rename and Disable Old Map");
                    go.name = "Map_Ground_Debug";
                    go.SetActive(false);
                    Debug.Log($"[Phase18Setup] Đã đổi tên và disable old map object: '{name}' -> 'Map_Ground_Debug'.");
                    return;
                }
            }
        }

        private static GameObject EnsureRootGameObject(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go != null && go.transform.parent == null)
            {
                return go;
            }
            go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, $"Create Root {name}");
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            Debug.Log($"[Phase18Setup] Đã tạo root GameObject: '{name}'");
            return go;
        }

        private static GameObject EnsureChildGameObject(GameObject parent, string name)
        {
            Transform child = parent.transform.Find(name);
            if (child != null)
            {
                return child.gameObject;
            }
            GameObject go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, $"Create Child {name}");
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            Debug.Log($"[Phase18Setup] Đã tạo child GameObject: '{parent.name}/{name}'");
            return go;
        }

        private static GameObject CreateGroundPlane(Transform parent, Vector3 boardCenter, Material mat)
        {
            // Kiểm tra xem đã có Ground_Plane chưa để tránh trùng lặp
            Transform existing = parent.Find("Ground_Plane");
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing.gameObject);
            }

            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "Ground_Plane";
            Undo.RegisterCreatedObjectUndo(plane, "Create Ground Plane");
            plane.transform.SetParent(parent);
            
            // Đặt Y = 0.5, căn giữa theo BoardManager
            plane.transform.position = new Vector3(boardCenter.x, 0.5f, boardCenter.z);
            plane.transform.localScale = new Vector3(4f, 1f, 4f); // 40x40 units

            MeshRenderer mr = plane.GetComponent<MeshRenderer>();
            if (mr != null && mat != null)
            {
                mr.sharedMaterial = mat;
            }

            return plane;
        }

        private static void CreateBorders(Transform parent, Vector3 boardCenter, Material mat)
        {
            string[] borderNames = { "Border_Left", "Border_Right", "Border_Top", "Border_Bottom" };
            foreach (var bName in borderNames)
            {
                Transform existing = parent.Find(bName);
                if (existing != null)
                {
                    Undo.DestroyObjectImmediate(existing.gameObject);
                }
            }

            // Tạo 4 bức tường xung quanh khu vực (kích thước khoảng 15x15 units xung quanh boardCenter)
            float offset = 10f;
            float thickness = 0.6f;
            float height = 0.8f;
            float length = 20.6f;

            // Border Left
            GameObject borderL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            borderL.name = "Border_Left";
            Undo.RegisterCreatedObjectUndo(borderL, "Create Border_Left");
            borderL.transform.SetParent(parent);
            borderL.transform.position = new Vector3(boardCenter.x - offset, 0.5f + height/2f, boardCenter.z);
            borderL.transform.localScale = new Vector3(thickness, height, length);
            borderL.GetComponent<MeshRenderer>().sharedMaterial = mat;

            // Border Right
            GameObject borderR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            borderR.name = "Border_Right";
            Undo.RegisterCreatedObjectUndo(borderR, "Create Border_Right");
            borderR.transform.SetParent(parent);
            borderR.transform.position = new Vector3(boardCenter.x + offset, 0.5f + height/2f, boardCenter.z);
            borderR.transform.localScale = new Vector3(thickness, height, length);
            borderR.GetComponent<MeshRenderer>().sharedMaterial = mat;

            // Border Top
            GameObject borderT = GameObject.CreatePrimitive(PrimitiveType.Cube);
            borderT.name = "Border_Top";
            Undo.RegisterCreatedObjectUndo(borderT, "Create Border_Top");
            borderT.transform.SetParent(parent);
            borderT.transform.position = new Vector3(boardCenter.x, 0.5f + height/2f, boardCenter.z + offset);
            borderT.transform.localScale = new Vector3(length, height, thickness);
            borderT.GetComponent<MeshRenderer>().sharedMaterial = mat;

            // Border Bottom
            GameObject borderB = GameObject.CreatePrimitive(PrimitiveType.Cube);
            borderB.name = "Border_Bottom";
            Undo.RegisterCreatedObjectUndo(borderB, "Create Border_Bottom");
            borderB.transform.SetParent(parent);
            borderB.transform.position = new Vector3(boardCenter.x, 0.5f + height/2f, boardCenter.z - offset);
            borderB.transform.localScale = new Vector3(length, height, thickness);
            borderB.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        private static void CreatePropsPlaceholder(Transform parent, Vector3 boardCenter, Material rockMat, Material trunkMat, Material leafMat)
        {
            // Xoá các props cũ nếu có để tránh spawn đè
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);
            }

            // Tạo các hòn đá (squashed cubes)
            CreateRock(parent, "Rock_1", new Vector3(boardCenter.x - 8f, 0.7f, boardCenter.z + 8f), new Vector3(1.5f, 0.8f, 1.2f), rockMat);
            CreateRock(parent, "Rock_2", new Vector3(boardCenter.x + 8f, 0.6f, boardCenter.z - 8f), new Vector3(1.1f, 1.1f, 1.1f), rockMat);
            CreateRock(parent, "Rock_3", new Vector3(boardCenter.x - 7.5f, 0.5f, boardCenter.z - 7.5f), new Vector3(0.9f, 0.5f, 0.9f), rockMat);

            // Tạo cây (cylinder + sphere)
            CreateTree(parent, "Tree_1", new Vector3(boardCenter.x - 8f, 0.5f, boardCenter.z - 2f), trunkMat, leafMat);
            CreateTree(parent, "Tree_2", new Vector3(boardCenter.x + 8f, 0.5f, boardCenter.z + 4f), trunkMat, leafMat);
            CreateTree(parent, "Tree_3", new Vector3(boardCenter.x - 2f, 0.5f, boardCenter.z + 8.5f), trunkMat, leafMat);
        }

        private static void CreateRock(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rock.name = name;
            Undo.RegisterCreatedObjectUndo(rock, "Create Rock Prop");
            rock.transform.SetParent(parent);
            rock.transform.position = pos;
            rock.transform.localScale = scale;
            rock.transform.rotation = Quaternion.Euler(Random.Range(0, 20f), Random.Range(0, 360f), Random.Range(0, 20f));
            rock.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        private static void CreateTree(Transform parent, string name, Vector3 pos, Material trunkMat, Material leafMat)
        {
            GameObject treeGroup = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(treeGroup, "Create Tree Group");
            treeGroup.transform.SetParent(parent);
            treeGroup.transform.position = pos;

            // Thân cây
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            Undo.RegisterCreatedObjectUndo(trunk, "Create Tree Trunk");
            trunk.transform.SetParent(treeGroup.transform);
            trunk.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            trunk.transform.localScale = new Vector3(0.3f, 0.75f, 0.3f);
            trunk.GetComponent<MeshRenderer>().sharedMaterial = trunkMat;

            // Lá cây
            GameObject leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leaves.name = "Leaves";
            Undo.RegisterCreatedObjectUndo(leaves, "Create Tree Leaves");
            leaves.transform.SetParent(treeGroup.transform);
            leaves.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            leaves.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
            leaves.GetComponent<MeshRenderer>().sharedMaterial = leafMat;
        }

        private static void MoveGameplayObjects(GameObject gameplayRoot)
        {
            string[] gameplayNames = { "BoardManager", "BenchSpawnPoints", "EnemySpawnPoints", "PlayerUnits", "Enemies" };
            foreach (var name in gameplayNames)
            {
                GameObject go = GameObject.Find(name);
                if (go != null && go.transform.parent != gameplayRoot.transform)
                {
                    Undo.SetTransformParent(go.transform, gameplayRoot.transform, $"Reparent {name} to Gameplay");
                    Debug.Log($"[Phase18Setup] Di chuyển '{name}' vào 'Gameplay'.");
                }
            }

            // Đảm bảo Enemies container tồn tại dưới Gameplay
            Transform enemiesTrans = gameplayRoot.transform.Find("Enemies");
            if (enemiesTrans == null)
            {
                GameObject enemiesGo = new GameObject("Enemies");
                Undo.RegisterCreatedObjectUndo(enemiesGo, "Create Enemies Container");
                enemiesGo.transform.SetParent(gameplayRoot.transform);
                enemiesGo.transform.localPosition = Vector3.zero;
                enemiesGo.transform.localRotation = Quaternion.identity;
                enemiesGo.transform.localScale = Vector3.one;
                Debug.Log("[Phase18Setup] Đã tạo container 'Enemies' dưới 'Gameplay'.");
            }

            // Chuẩn hoá Y của BenchSpawnPoints và EnemySpawnPoints để unit đứng đẹp trên BoardCell (Y khoảng 0.535)
            // BenchSpawnPoints và EnemySpawnPoints nên ở Y = 1.02 đến 1.05
            AdjustSpawnPointHeights(gameplayRoot.transform.Find("BenchSpawnPoints"));
            AdjustSpawnPointHeights(gameplayRoot.transform.Find("EnemySpawnPoints"));
        }

        private static void AdjustSpawnPointHeights(Transform container)
        {
            if (container == null) return;
            for (int i = 0; i < container.childCount; i++)
            {
                Transform child = container.GetChild(i);
                if (child.position.y < 0.9f)
                {
                    Undo.RecordObject(child, "Adjust Spawn Point Height");
                    Vector3 pos = child.position;
                    pos.y = 1.02f; // Cao độ hợp lý để unit đứng trên map visual 0.5
                    child.position = pos;
                    Debug.Log($"[Phase18Setup] Cập nhật Y của '{container.name}/{child.name}' lên {pos.y}.");
                }
            }
        }

        private static void MoveManagersObjects(GameObject managersRoot)
        {
            string[] managerNames = {
                "GameConfig", "GamePhaseManager", "EnemyManager", "PlayerUnitManager",
                "WaveManager", "ShopManager", "EconomyManager", "UnitStarUpgradeManager",
                "BattleResetManager", "RightClickInspectController", "EventSystem"
            };

            foreach (var name in managerNames)
            {
                GameObject go = GameObject.Find(name);
                if (go != null && go.transform.parent != managersRoot.transform)
                {
                    Undo.SetTransformParent(go.transform, managersRoot.transform, $"Reparent {name} to Managers");
                    Debug.Log($"[Phase18Setup] Di chuyển '{name}' vào 'Managers'.");
                }
            }
        }

        private static void MoveShopCanvasToRoot()
        {
            GameObject canvasGo = GameObject.Find("ShopCanvas");
            if (canvasGo != null && canvasGo.transform.parent != null)
            {
                Undo.SetTransformParent(canvasGo.transform, null, "Move ShopCanvas to Root");
                Debug.Log("[Phase18Setup] Đã đưa 'ShopCanvas' về root level.");
            }
        }

        private static void SetLayerRecursive(GameObject go, int layer)
        {
            if (go == null) return;
            Undo.RecordObject(go, "Set Layer");
            go.layer = layer;
            foreach (Transform child in go.transform)
            {
                SetLayerRecursive(child.gameObject, layer);
            }
        }
    }
}
#endif
