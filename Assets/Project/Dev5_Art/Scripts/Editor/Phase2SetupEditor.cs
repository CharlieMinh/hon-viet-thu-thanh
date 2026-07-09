using UnityEngine;
using UnityEditor;
using HonVietThuThanh.Dev5;

namespace HonVietThuThanh.Dev5Editor
{
    /// <summary>
    /// Editor utility tự động thiết lập Phase 2: Unit Placeholder + UnitPlacementManager.
    /// Chạy qua menu: Dev5 > Setup Phase 2 – Unit Placement
    /// </summary>
    public static class Phase2SetupEditor
    {
        private const string SCENE_REQUIRED = "Scene_Dev5_Art";

        // ── Màu sắc unit ──────────────────────────────────────────────────
        private static readonly Color KnightColor  = new Color(0.20f, 0.50f, 1.00f, 1f); // Xanh dương
        private static readonly Color ArcherColor  = new Color(1.00f, 0.80f, 0.15f, 1f); // Vàng
        private static readonly Color TankColor    = new Color(0.55f, 0.55f, 0.55f, 1f); // Xám

        // ── Vị trí unit trong khu vực chờ ────────────────────────────────
        private static readonly Vector3 KnightWaitPos = new Vector3(-3.5f, 1.05f, -1f);
        private static readonly Vector3 ArcherWaitPos = new Vector3(-3.5f, 1.05f,  0f);
        private static readonly Vector3 TankWaitPos   = new Vector3(-3.5f, 1.05f,  1f);

        // ── Material paths ────────────────────────────────────────────────
        private const string MatDir   = "Assets/Project/Dev5_Art/Materials";
        private const string MatKnight = MatDir + "/M_Unit_Knight.mat";
        private const string MatArcher = MatDir + "/M_Unit_Archer.mat";
        private const string MatTank   = MatDir + "/M_Unit_Tank.mat";

        // ─────────────────────────────────────────────────────────────────
        [MenuItem("Dev5/Setup Phase 2 - Unit Placement")]
        public static void SetupPhase2()
        {
            // ── 0. Kiểm tra scene ────────────────────────────────────────
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!activeScene.name.Equals(SCENE_REQUIRED))
            {
                bool proceed = EditorUtility.DisplayDialog(
                    "Sai Scene",
                    $"Hãy mở scene '{SCENE_REQUIRED}' trước.\n\nScene hiện tại: '{activeScene.name}'.\n\nVẫn tiếp tục?",
                    "Tiếp tục", "Hủy");
                if (!proceed) return;
            }

            Undo.SetCurrentGroupName("Phase 2 Setup");
            int undoGroup = Undo.GetCurrentGroup();

            // ── 1. Tạo / tìm UnitPlacementManager ───────────────────────
            EnsureUnitPlacementManager();

            // ── 2. Tạo materials ─────────────────────────────────────────
            Material matKnight = EnsureMaterial(MatKnight, KnightColor);
            Material matArcher = EnsureMaterial(MatArcher, ArcherColor);
            Material matTank   = EnsureMaterial(MatTank,   TankColor);

            // ── 3. Tạo / tìm PlayerUnits container ──────────────────────
            GameObject playerUnitsGO = EnsureGameObject("PlayerUnits", Vector3.zero, null);

            // ── 4. Tạo 3 unit ────────────────────────────────────────────
            EnsureUnit("Knight_TestUnit", "Knight", KnightWaitPos, KnightColor, matKnight, playerUnitsGO.transform);
            EnsureUnit("Archer_TestUnit", "Archer", ArcherWaitPos, ArcherColor, matArcher, playerUnitsGO.transform);
            EnsureUnit("Tank_TestUnit",   "Tank",   TankWaitPos,   TankColor,   matTank,   playerUnitsGO.transform);

            Undo.CollapseUndoOperations(undoGroup);

            EditorUtility.DisplayDialog(
                "Phase 2 Setup Hoàn Tất",
                "Đã tạo thành công:\n" +
                "• UnitPlacementManager\n" +
                "• PlayerUnits (container)\n" +
                "• Knight_TestUnit\n" +
                "• Archer_TestUnit\n" +
                "• Tank_TestUnit\n\n" +
                "Lưu scene lại (Ctrl+S) và nhấn Play để test.",
                "OK");

            Debug.Log("[Phase2Setup] ✅ Phase 2 setup hoàn tất. Mở Play Mode để kiểm tra.");
        }

        // ─────────────────────────────────────────────────────────────────
        #region Helpers

        private static void EnsureUnitPlacementManager()
        {
            // Nếu đã tồn tại thì bỏ qua
            var existing = Object.FindAnyObjectByType<UnitPlacementManager>();
            if (existing != null)
            {
                Debug.Log("[Phase2Setup] UnitPlacementManager đã tồn tại, bỏ qua.");
                return;
            }

            GameObject go = new GameObject("UnitPlacementManager");
            Undo.RegisterCreatedObjectUndo(go, "Create UnitPlacementManager");
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            go.AddComponent<UnitPlacementManager>();
            Debug.Log("[Phase2Setup] Đã tạo UnitPlacementManager.");
        }

        private static Material EnsureMaterial(string assetPath, Color baseColor)
        {
            // Nếu material đã tồn tại, trả về luôn
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (existing != null) return existing;

            // Tìm shader URP/Lit (có thể dùng Shader.Find)
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                         ?? Shader.Find("Standard");

            if (shader == null)
            {
                Debug.LogError($"[Phase2Setup] Không tìm thấy shader! Material '{assetPath}' sẽ dùng default.");
                return null;
            }

            Material mat = new Material(shader);
            mat.SetColor("_BaseColor", baseColor);
            mat.SetColor("_Color", baseColor);

            AssetDatabase.CreateAsset(mat, assetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"[Phase2Setup] Đã tạo material: {assetPath}");
            return mat;
        }

        private static GameObject EnsureGameObject(string name, Vector3 worldPos, Transform parent)
        {
            // Tìm trong scene trước
            var go = GameObject.Find(name);
            if (go != null) return go;

            go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            go.transform.SetPositionAndRotation(worldPos, Quaternion.identity);
            if (parent != null) go.transform.SetParent(parent);
            return go;
        }

        private static void EnsureUnit(string goName, string unitName, Vector3 worldPos,
                                        Color unitColor, Material mat, Transform parent)
        {
            // Kiểm tra nếu đã tồn tại
            if (GameObject.Find(goName) != null)
            {
                Debug.Log($"[Phase2Setup] '{goName}' đã tồn tại, bỏ qua.");
                return;
            }

            // ── Tạo Capsule ──────────────────────────────────────────────
            GameObject unitGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            unitGO.name = goName;
            Undo.RegisterCreatedObjectUndo(unitGO, $"Create {goName}");

            unitGO.transform.position = worldPos;
            unitGO.transform.SetParent(parent);

            // ── Gắn material ─────────────────────────────────────────────
            MeshRenderer renderer = unitGO.GetComponent<MeshRenderer>();
            if (renderer != null && mat != null)
            {
                renderer.sharedMaterial = mat;
            }
            else if (renderer != null)
            {
                // Fallback: đặt màu thủ công qua MaterialPropertyBlock không khả dụng trong Editor
                // nên ta tạo instance material tạm
                Material fallback = new Material(Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit"));
                fallback.color = unitColor;
                renderer.sharedMaterial = fallback;
            }

            // ── Đảm bảo có Collider ──────────────────────────────────────
            // CreatePrimitive đã tự thêm CapsuleCollider, kiểm tra cho chắc
            if (unitGO.GetComponent<Collider>() == null)
            {
                unitGO.AddComponent<CapsuleCollider>();
            }

            // ── Gắn PlaceableUnit script ─────────────────────────────────
            PlaceableUnit pu = unitGO.AddComponent<PlaceableUnit>();
            pu.unitName = unitName;

            // Cài màu sắc cho component (serialize field sẽ giữ nguyên sau khi lưu)
            // SerializedObject để set qua Undo-safe path
            var so = new SerializedObject(pu);
            so.FindProperty("normalColor").colorValue   = unitColor;
            so.FindProperty("selectedColor").colorValue = new Color(1f, 0.85f, 0.1f, 1f); // Vàng chọn
            so.FindProperty("placedColor").colorValue   = Color.Lerp(unitColor, Color.white, 0.35f);
            so.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log($"[Phase2Setup] Đã tạo unit '{goName}' tại {worldPos}.");
        }

        #endregion
    }
}
