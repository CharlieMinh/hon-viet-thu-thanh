#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace HonVietThuThanh.Dev5Editor
{
    public static class InspectKnightPrefab
    {
        [MenuItem("Tools/Hon Viet Thu Thanh/Inspect Knight Prefab")]
        public static void InspectKnight()
        {
            InspectPrefab("Assets/Project/Dev5_Art/Prefabs/Heroes/Knight_Unit_Prefab.prefab", "KNIGHT PREFAB");
        }

        [MenuItem("Tools/Hon Viet Thu Thanh/Inspect Archer Prefab")]
        public static void InspectArcher()
        {
            InspectPrefab("Assets/Project/Dev5_Art/Prefabs/Heroes/Archer_Unit_Prefab.prefab", "ARCHER PREFAB");
        }

        [MenuItem("Tools/Hon Viet Thu Thanh/Inspect Knight FBX Bones")]
        public static void InspectKnightFBX()
        {
            InspectFBX("Assets/Project/Dev5_Art/Models/Ironbound_Warlord/Meshy_AI_Ironbound_Warlord_biped_Character_output.fbx", "KNIGHT FBX");
        }

        [MenuItem("Tools/Hon Viet Thu Thanh/Inspect Archer FBX Bones")]
        public static void InspectArcherFBX()
        {
            InspectFBX("Assets/Project/Dev5_Art/Animations/Archer/Animation/idle/Meshy_AI_Dragonbow_General_biped/Meshy_AI_Dragonbow_General_biped_Animation_Alert_withSkin.fbx", "ARCHER FBX");
        }

        private static void InspectPrefab(string path, string label)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError($"[Inspector] Không tìm thấy prefab: {path}");
                return;
            }
            Debug.Log($"============ INSPECTING {label} ============");
            LogTransform(prefab.transform, "", 0);
        }

        private static void InspectFBX(string path, string label)
        {
            GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (fbx == null)
            {
                Debug.LogError($"[Inspector] Không tìm thấy FBX: {path}");
                return;
            }
            Debug.Log($"============ INSPECTING {label} HIERARCHY ============");
            LogTransform(fbx.transform, "", 0);
        }

        private static void LogTransform(Transform t, string indent, int depth)
        {
            if (depth > 8) return; // Chỉ log tối đa 8 cấp để tránh ngập lụt console
            
            var mr = t.GetComponent<MeshRenderer>();
            var smr = t.GetComponent<SkinnedMeshRenderer>();
            var animator = t.GetComponent<Animator>();

            string info = "";
            if (mr != null) info += $" | MeshRenderer";
            if (smr != null) info += $" | SkinnedMeshRenderer";
            if (animator != null)
            {
                string controllerName = animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "null";
                string avatarName = animator.avatar != null ? animator.avatar.name : "null";
                info += $" | Animator (Controller: {controllerName}, Avatar: {avatarName}, isHuman: {(animator.avatar != null ? animator.avatar.isHuman.ToString() : "N/A")})";
            }
            
            Debug.Log($"{indent}- {t.name}{info}");
            
            for (int i = 0; i < t.childCount; i++)
            {
                LogTransform(t.GetChild(i), indent + "  ", depth + 1);
            }
        }
    }
}
#endif
