#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using HonVietThuThanh.Dev5;

namespace HonVietThuThanh.Dev5Editor
{
    public static class InspectKnightPrefab
    {
        [MenuItem("Tools/Hon Viet Thu Thanh/Inspect Knight Prefab")]
        public static void Inspect()
        {
            string path = "Assets/Project/Dev5_Art/Prefabs/Heroes/Knight_Unit_Prefab.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogError($"[Inspector] Không tìm thấy Knight prefab tại: {path}");
                return;
            }

            Debug.Log("============ INSPECTING KNIGHT PREFAB ============");
            Debug.Log($"Prefab Name: {prefab.name}");
            Debug.Log($"Active Self: {prefab.activeSelf}");
            
            // Log hierarchy
            LogTransform(prefab.transform, "");

            // Check CharacterVisualSlot reference
            var visualSlot = prefab.GetComponent<CharacterVisualSlot>();
            if (visualSlot != null)
            {
                Debug.Log($"[VisualSlot] visualRoot: {visualSlot.visualRoot?.name}, modelSlot: {visualSlot.modelSlot?.name}, placeholder: {visualSlot.placeholder?.name}");
            }
            else
            {
                Debug.LogWarning("[VisualSlot] Thiếu Component CharacterVisualSlot!");
            }

            // Check CharacterAnimationController
            var animController = prefab.GetComponent<CharacterAnimationController>();
            if (animController != null)
            {
                Debug.Log("[AnimController] Đã gắn CharacterAnimationController");
            }
            else
            {
                Debug.LogWarning("[AnimController] Thiếu Component CharacterAnimationController!");
            }
        }

        private static void LogTransform(Transform t, string indent)
        {
            var mr = t.GetComponent<MeshRenderer>();
            var smr = t.GetComponent<SkinnedMeshRenderer>();
            string rendererInfo = "";
            if (mr != null) rendererInfo += $" | MeshRenderer (Materials: {mr.sharedMaterials.Length}, Enabled: {mr.enabled})";
            if (smr != null) rendererInfo += $" | SkinnedMeshRenderer (Materials: {smr.sharedMaterials.Length}, Enabled: {smr.enabled})";
            
            Debug.Log($"{indent}- {t.name} (Active: {t.gameObject.activeSelf}, LocalPosition: {t.localPosition}, LocalRotation: {t.localEulerAngles}, LocalScale: {t.localScale}){rendererInfo}");
            
            for (int i = 0; i < t.childCount; i++)
            {
                LogTransform(t.GetChild(i), indent + "  ");
            }
        }
    }
}
#endif
