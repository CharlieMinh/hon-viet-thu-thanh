using UnityEngine;
using UnityEditor;
using HonVietThuThanh.Dev5;

namespace HonVietThuThanh.Dev5.EditorTools
{
    /// <summary>
    /// Editor tool giúp tự động hóa quá trình tích hợp Model 3D từ Meshy (www.meshy.ai)
    /// vào đúng cấu trúc Prefab Logic/Visual của dự án Hồn Việt Thủ Thành.
    /// </summary>
    public class MeshyIntegrationHelper : EditorWindow
    {
        [MenuItem("Tools/Hon Viet Thu Thanh/Meshy Integration Helper")]
        public static void ShowWindow()
        {
            GetWindow<MeshyIntegrationHelper>("Meshy Integrator");
        }

        private GameObject targetPrefab;
        private GameObject meshyModelFBX;
        private RuntimeAnimatorController animatorController;
        private float modelScale = 1.0f;
        private float rotationX = -90.0f; // Mặc định xoay -90 độ trục X vì model Meshy thường bị nằm sấp
        private float rotationY = 180.0f;
        private Vector3 positionOffset = new Vector3(0f, -1f, 0f); // Mặc định hạ xuống -1 để chân chạm đất giống Archer

        private void OnGUI()
        {
            GUILayout.Label("Meshy 3D Asset Integrator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            targetPrefab = (GameObject)EditorGUILayout.ObjectField(
                "Target Prefab (Unit/Enemy)", 
                targetPrefab, 
                typeof(GameObject), 
                false
            );

            meshyModelFBX = (GameObject)EditorGUILayout.ObjectField(
                "Meshy FBX Model", 
                meshyModelFBX, 
                typeof(GameObject), 
                false
            );

            animatorController = (RuntimeAnimatorController)EditorGUILayout.ObjectField(
                "Animator Controller", 
                animatorController, 
                typeof(RuntimeAnimatorController), 
                false
            );

            EditorGUILayout.Space();
            GUILayout.Label("Model Transform Adjustments", EditorStyles.boldLabel);
            modelScale = EditorGUILayout.FloatField("Model Scale Factor", modelScale);
            rotationX = EditorGUILayout.FloatField("Rotation X (vertical tilt)", rotationX);
            rotationY = EditorGUILayout.FloatField("Rotation Y (facing front)", rotationY);
            positionOffset = EditorGUILayout.Vector3Field("Position Offset (ground alignment)", positionOffset);

            EditorGUILayout.Space();

            if (GUILayout.Button("Integrate Model Into Prefab", GUILayout.Height(40)))
            {
                IntegrateModel();
            }
        }

        private void IntegrateModel()
        {
            if (targetPrefab == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Vui lòng kéo Target Prefab (ví dụ: Knight_Unit_Prefab) vào!", "OK");
                return;
            }

            if (meshyModelFBX == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Vui lòng kéo Model FBX được export từ Meshy vào!", "OK");
                return;
            }

            string prefabPath = AssetDatabase.GetAssetPath(targetPrefab);
            if (string.IsNullOrEmpty(prefabPath))
            {
                EditorUtility.DisplayDialog("Lỗi", "Target Prefab phải là một Asset trong project (không phải object trong scene)!", "OK");
                return;
            }

            // Load Prefab dưới dạng Root để chỉnh sửa
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                // 1. Tìm các components quản lý Visual & Animation
                CharacterVisualSlot visualSlot = prefabRoot.GetComponent<CharacterVisualSlot>();
                CharacterAnimationController animController = prefabRoot.GetComponent<CharacterAnimationController>();

                if (visualSlot == null)
                {
                    visualSlot = prefabRoot.AddComponent<CharacterVisualSlot>();
                }

                if (animController == null)
                {
                    animController = prefabRoot.AddComponent<CharacterAnimationController>();
                }

                // Thiết lập/Tìm ModelSlot
                Transform visualTrans = prefabRoot.transform.Find("Visual");
                if (visualTrans == null)
                {
                    GameObject visualGO = new GameObject("Visual");
                    visualGO.transform.SetParent(prefabRoot.transform, false);
                    visualTrans = visualGO.transform;
                }

                Transform modelSlotTrans = visualTrans.Find("ModelSlot");
                if (modelSlotTrans == null)
                {
                    GameObject modelSlotGO = new GameObject("ModelSlot");
                    modelSlotGO.transform.SetParent(visualTrans, false);
                    modelSlotTrans = modelSlotGO.transform;
                }

                Transform placeholderTrans = visualTrans.Find("Placeholder");
                
                // Cấu hình tham chiếu cho CharacterVisualSlot
                visualSlot.visualRoot = visualTrans;
                visualSlot.modelSlot = modelSlotTrans;
                if (placeholderTrans != null)
                {
                    visualSlot.placeholder = placeholderTrans.gameObject;
                }

                // 2. Dọn dẹp model cũ trong ModelSlot
                for (int i = modelSlotTrans.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(modelSlotTrans.GetChild(i).gameObject);
                }

                // 3. Instantiate model mới vào ModelSlot
                GameObject newModel = (GameObject)PrefabUtility.InstantiatePrefab(meshyModelFBX, modelSlotTrans);
                newModel.name = meshyModelFBX.name;

                newModel.transform.localPosition = positionOffset;
                newModel.transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0);
                newModel.transform.localScale = Vector3.one * modelScale;

                // 3.5 Tự động tìm và gán Material trong cùng folder với FBX
                string fbxPath = AssetDatabase.GetAssetPath(meshyModelFBX);
                if (!string.IsNullOrEmpty(fbxPath))
                {
                    string folder = System.IO.Path.GetDirectoryName(fbxPath);
                    string[] matGuids = AssetDatabase.FindAssets("t:Material", new[] { folder });
                    Material targetMat = null;
                    if (matGuids != null && matGuids.Length > 0)
                    {
                        string matPath = AssetDatabase.GUIDToAssetPath(matGuids[0]);
                        targetMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                    }

                    if (targetMat != null)
                    {
                        Renderer[] renderers = newModel.GetComponentsInChildren<Renderer>(true);
                        foreach (var r in renderers)
                        {
                            Material[] mats = new Material[r.sharedMaterials.Length];
                            for (int m = 0; m < mats.Length; m++)
                            {
                                mats[m] = targetMat;
                            }
                            r.sharedMaterials = mats;
                        }
                        Debug.Log($"[Meshy Integrator] Tự động gán Material '{targetMat.name}' cho {renderers.Length} renderers.");
                    }
                    else
                    {
                        Debug.LogWarning("[Meshy Integrator] Không tìm thấy file Material (.mat) nào trong thư mục của FBX để gán!");
                    }
                }

                // 4. Cấu hình Animator trên Model con
                Animator animator = newModel.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = newModel.AddComponent<Animator>();
                }
                
                animator.applyRootMotion = false; // Luôn luôn tắt Root Motion theo quy tắc
                if (animatorController != null)
                {
                    animator.runtimeAnimatorController = animatorController;
                }

                // 5. Cập nhật CharacterAnimationController
                if (animController != null)
                {
                    animController.FindAnimator();
                }

                // 6. Ẩn/Hiện placeholder tự động
                visualSlot.AutoFitModelIfNeeded();

                // Lưu thay đổi vào Prefab Asset
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                Debug.Log($"<color=green>[Meshy Integrator]</color> Đã tích hợp thành công model '{meshyModelFBX.name}' vào prefab '{targetPrefab.name}' và tắt applyRootMotion!");
                
                EditorUtility.DisplayDialog("Thành công", $"Đã tích hợp thành công model {meshyModelFBX.name}!", "OK");
            }
            finally
            {
                // Giải phóng bộ nhớ prefab
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }
    }
}
