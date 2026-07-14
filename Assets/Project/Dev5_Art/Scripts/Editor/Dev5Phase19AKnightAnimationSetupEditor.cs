#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using HonVietThuThanh.Dev5;

namespace HonVietThuThanh.Dev5Editor
{
    /// <summary>
    /// Phase 19A v3: Knight (Ironbound Warlord) Animation Setup.
    /// Menu: Dev5 / Setup Phase 19A - Knight Animation
    ///
    /// Fix log (v3):
    ///   - Dùng Generic rig (không Humanoid) vì Meshy dùng tên bone riêng không khớp chuẩn Unity Humanoid.
    ///   - Không gọi SaveAndReimport() trong ApplyMaterial để tránh lỗi "Hips not found" trong editor loop.
    ///   - Fix Knight root scale: khớp Archer (scale = 1,1,1 không phải 0.01).
    ///   - Dùng T-Pose model FBX làm visual, localPosition.y = -1 như Archer.
    ///   - Không cần Avatar cho Generic rig — Animator.avatar = null là đúng.
    /// </summary>
    public static class Dev5Phase19AKnightAnimationSetupEditor
    {
        // ─── Paths ───────────────────────────────────────────────────────────
        private const string SCENE_REQUIRED     = "Scene_Dev5_Art";
        private const string KNIGHT_PREFAB_PATH = "Assets/Project/Dev5_Art/Prefabs/Heroes/Knight_Unit_Prefab.prefab";
        private const string ANIMATIONS_DIR     = "Assets/Project/Dev5_Art/Animations/Ironbound_Warlord";
        private const string CONTROLLER_PATH    = "Assets/Project/Dev5_Art/Animations/Ironbound_Warlord/Warlord_AnimatorController.controller";
        private const string MODEL_FBX_PATH     = "Assets/Project/Dev5_Art/Models/Ironbound_Warlord/Meshy_AI_Ironbound_Warlord_biped_Character_output.fbx";
        private const string MAT_PATH           = "Assets/Project/Dev5_Art/Materials/M_Unit_Knight.mat";
        private const string TEX_PATH           = "Assets/Project/Dev5_Art/Models/Ironbound_Warlord/Meshy_AI_Ironbound_Warlord_0701110034_texture.png";
        private const string METALLIC_PATH      = "Assets/Project/Dev5_Art/Models/Ironbound_Warlord/Meshy_AI_Ironbound_Warlord_0701110034_texture_metallic.png";

        // ─── Audio paths ──────────────────────────────────────────────────────
        private const string ATTACK_AUDIO_PATH = "Assets/Project/Dev5_Art/Audio/Archer/Audio/Attack/freesound_community-punches-32563.mp3";
        private const string DEATH_AUDIO_PATH  = "Assets/Project/Dev5_Art/Audio/Tank/Voice/voice_tank/Tank_Death.wav";

        // ─── Menu ─────────────────────────────────────────────────────────────
        [MenuItem("Dev5/Setup Phase 19A - Knight Animation")]
        public static void SetupPhase19AKnightAnimation()
        {
            var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            if (!activeScene.name.Equals(SCENE_REQUIRED, System.StringComparison.OrdinalIgnoreCase))
                Debug.LogWarning($"[Phase19AKnightSetup] Scene '{activeScene.name}' (mong đợi '{SCENE_REQUIRED}'). Tiếp tục...");

            Undo.SetCurrentGroupName("Phase 19A Knight Animation Setup v3");
            int undoGroup = Undo.GetCurrentGroup();

            // ── STEP 1: Quét FBX animation ───────────────────────────────────
            string idleFbx   = null;
            string alertFbx  = null;
            string boomFbx   = null;
            string moveFbx   = null;
            string attackFbx = null;
            string deathFbx  = null;

            if (System.IO.Directory.Exists(ANIMATIONS_DIR))
            {
                foreach (string file in System.IO.Directory.GetFiles(ANIMATIONS_DIR, "*.fbx", System.IO.SearchOption.AllDirectories))
                {
                    string norm  = file.Replace('\\', '/');
                    string lower = System.IO.Path.GetFileName(file).ToLower();
                    if      (lower.Contains("alert"))       alertFbx  = norm;
                    else if (lower.Contains("boom_dance"))  boomFbx   = norm;
                    else if (lower.Contains("you_groove"))  moveFbx   = norm;
                    else if (lower.Contains("attack"))      attackFbx = norm;
                    else if (lower.Contains("skill_01"))    deathFbx  = norm;
                }
            }

            // Alert is the real Knight idle. BoomDance remains a fallback only when
            // the imported Alert FBX does not produce a valid animation clip.
            idleFbx = alertFbx ?? boomFbx;

            Debug.Log($"[Phase19AKnightSetup] FBX mapping:\n" +
                      $"  Idle  : {idleFbx   ?? "❌ MISSING"} (Alert preferred, BoomDance fallback)\n" +
                      $"  Move  : {moveFbx   ?? "❌ MISSING"}\n" +
                      $"  Attack: {attackFbx ?? "❌ MISSING"}\n" +
                      $"  Death : {deathFbx  ?? "❌ MISSING"}");

            if (idleFbx == null)
            {
                Debug.LogError("[Phase19AKnightSetup] ❌ Không tìm thấy Alert hoặc BoomDance FBX. Dừng setup.");
                Undo.CollapseUndoOperations(undoGroup);
                return;
            }

            // ── STEP 2: Configure Generic rig + loop (KHÔNG dùng Humanoid) ───
            // Meshy biped bones không có tên chuẩn Unity (Hips, Spine...) → dùng Generic
            ConfigureGenericFBX(MODEL_FBX_PATH, importAnimation: false, isLoop: false);
            ConfigureGenericFBX(idleFbx,   importAnimation: true, isLoop: true);
            ConfigureGenericFBX(moveFbx,   importAnimation: true, isLoop: true);
            ConfigureGenericFBX(attackFbx, importAnimation: true, isLoop: false);
            ConfigureGenericFBX(deathFbx,  importAnimation: true, isLoop: false);

            AssetDatabase.Refresh();

            // ── STEP 3: Lấy clips từ FBX ────────────────────────────────────
            AnimationClip idleClip   = GetFirstValidClip(idleFbx);
            AnimationClip moveClip   = GetFirstValidClip(moveFbx);
            AnimationClip attackClip = GetFirstValidClip(attackFbx);
            AnimationClip deathClip  = GetFirstValidClip(deathFbx);

            if (moveClip == null)
            {
                moveClip = idleClip;
                Debug.LogWarning("[Phase19AKnightSetup] ⚠️ Move clip null → fallback sang Idle.");
            }

            Debug.Log($"[Phase19AKnightSetup] Clips:\n" +
                      $"  Idle  : {(idleClip   != null ? idleClip.name   : "null")}\n" +
                      $"  Move  : {(moveClip   != null ? moveClip.name   : "null")}\n" +
                      $"  Attack: {(attackClip != null ? attackClip.name : "null")}\n" +
                      $"  Death : {(deathClip  != null ? deathClip.name  : "null")}");

            // ── STEP 4: AnimatorController ───────────────────────────────────
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CONTROLLER_PATH);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(CONTROLLER_PATH);
                Debug.Log($"[Phase19AKnightSetup] Tạo controller mới tại: {CONTROLLER_PATH}");
            }

            EnsureParameter(controller, "IsMoving", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "Attack",   AnimatorControllerParameterType.Trigger);
            EnsureParameter(controller, "Death",    AnimatorControllerParameterType.Trigger);
            BuildStateMachine(controller, idleClip, moveClip, attackClip, deathClip);
            EditorUtility.SetDirty(controller);

            // ── STEP 5: Cập nhật Prefab ──────────────────────────────────────
            UpdateKnightPrefab(controller);

            Undo.CollapseUndoOperations(undoGroup);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Phase19AKnightSetup] ✅ Setup Phase 19A Knight Animation v3 HOÀN TẤT!");
            PrintChecklist();
        }

        // ─── Configure Generic FBX (không Humanoid, không Avatar) ────────────
        private static void ConfigureGenericFBX(string fbxPath, bool importAnimation, bool isLoop)
        {
            if (string.IsNullOrEmpty(fbxPath)) return;
            var mi = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            if (mi == null) return;

            bool dirty = false;

            // Dùng Generic rig — không Humanoid
            if (mi.animationType != ModelImporterAnimationType.Generic)
            {
                mi.animationType = ModelImporterAnimationType.Generic;
                dirty = true;
            }

            if (mi.importAnimation != importAnimation)
            {
                mi.importAnimation = importAnimation;
                dirty = true;
            }

            // Cập nhật loop nếu FBX có clip
            if (importAnimation)
            {
                var clips = mi.clipAnimations;
                if (clips == null || clips.Length == 0)
                    clips = mi.defaultClipAnimations;

                if (clips != null && clips.Length > 0)
                {
                    bool loopChanged = false;
                    for (int i = 0; i < clips.Length; i++)
                    {
                        if (clips[i].loopTime != isLoop) { clips[i].loopTime = isLoop; loopChanged = true; }
                    }
                    if (loopChanged) { mi.clipAnimations = clips; dirty = true; }
                }
            }

            if (dirty)
            {
                mi.SaveAndReimport();
                Debug.Log($"[Phase19AKnightSetup] Configured Generic FBX (importAnim={importAnimation}, loop={isLoop}): {System.IO.Path.GetFileName(fbxPath)}");
            }
        }

        // ─── Get first valid clip (bỏ qua clip ẩn __preview__) ───────────────
        private static AnimationClip GetFirstValidClip(string fbxPath)
        {
            if (string.IsNullOrEmpty(fbxPath)) return null;
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(fbxPath))
            {
                if (asset is AnimationClip clip && !clip.name.StartsWith("__"))
                    return clip;
            }
            return null;
        }

        // ─── Ensure animator parameter ────────────────────────────────────────
        private static void EnsureParameter(AnimatorController ctrl, string name, AnimatorControllerParameterType type)
        {
            foreach (var p in ctrl.parameters)
                if (p.name == name && p.type == type) return;
            ctrl.AddParameter(name, type);
        }

        // ─── Build clean State Machine ────────────────────────────────────────
        private static void BuildStateMachine(
            AnimatorController controller,
            AnimationClip idle, AnimationClip move,
            AnimationClip attack, AnimationClip death)
        {
            var sm = controller.layers[0].stateMachine;

            AnimatorState idleState   = FindOrCreateState(sm, "Idle",   idle);
            AnimatorState moveState   = FindOrCreateState(sm, "Move",   move);
            AnimatorState attackState = FindOrCreateState(sm, "Attack", attack);
            AnimatorState deathState  = FindOrCreateState(sm, "Death",  death);

            sm.defaultState = idleState;

            // Xóa transitions cũ
            idleState.transitions   = new AnimatorStateTransition[0];
            moveState.transitions   = new AnimatorStateTransition[0];
            attackState.transitions = new AnimatorStateTransition[0];
            deathState.transitions  = new AnimatorStateTransition[0];
            sm.anyStateTransitions  = new AnimatorStateTransition[0];

            // Idle → Move
            var t = idleState.AddTransition(moveState);
            t.hasExitTime = false; t.duration = 0.1f;
            t.AddCondition(AnimatorConditionMode.If, 0f, "IsMoving");

            // Move → Idle
            t = moveState.AddTransition(idleState);
            t.hasExitTime = false; t.duration = 0.1f;
            t.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsMoving");

            // AnyState → Attack
            var anyAtk = sm.AddAnyStateTransition(attackState);
            anyAtk.hasExitTime = false; anyAtk.duration = 0.1f;
            anyAtk.canTransitionToSelf = false;
            anyAtk.AddCondition(AnimatorConditionMode.If, 0f, "Attack");

            // Attack → Idle (exit time)
            t = attackState.AddTransition(idleState);
            t.hasExitTime = true; t.exitTime = 0.95f; t.duration = 0.15f;
            t.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsMoving");

            // Attack → Move (exit time)
            t = attackState.AddTransition(moveState);
            t.hasExitTime = true; t.exitTime = 0.95f; t.duration = 0.15f;
            t.AddCondition(AnimatorConditionMode.If, 0f, "IsMoving");

            // AnyState → Death
            var anyDth = sm.AddAnyStateTransition(deathState);
            anyDth.hasExitTime = false; anyDth.duration = 0.1f;
            anyDth.canTransitionToSelf = false;
            anyDth.AddCondition(AnimatorConditionMode.If, 0f, "Death");
            // Death không exit
        }

        private static AnimatorState FindOrCreateState(AnimatorStateMachine sm, string stateName, Motion motion)
        {
            foreach (var cs in sm.states)
            {
                if (cs.state.name == stateName) { cs.state.motion = motion; return cs.state; }
            }
            var s = sm.AddState(stateName);
            s.motion = motion;
            return s;
        }

        // ─── Update Knight Prefab ─────────────────────────────────────────────
        private static void UpdateKnightPrefab(AnimatorController controller)
        {
            GameObject modelFbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(MODEL_FBX_PATH);
            if (modelFbxAsset == null)
            {
                Debug.LogError($"[Phase19AKnightSetup] ❌ Không tìm thấy T-Pose model tại: {MODEL_FBX_PATH}");
                return;
            }

            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(KNIGHT_PREFAB_PATH);
            if (prefabAsset == null)
            {
                Debug.LogError($"[Phase19AKnightSetup] ❌ Không tìm thấy Knight prefab tại: {KNIGHT_PREFAB_PATH}");
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefabAsset) as GameObject;
            if (instance == null) { Debug.LogError("[Phase19AKnightSetup] ❌ Không thể instantiate prefab."); return; }

            // ── Fix root scale về (1,1,1) giống Archer ───────────────────────
            // Knight bị scale (0.01, 0.01, 0.01) → model chỉ là chấm nhỏ
            if (instance.transform.localScale != Vector3.one)
            {
                instance.transform.localScale = Vector3.one;
                Debug.Log("[Phase19AKnightSetup] ✅ Đã reset root scale Knight về (1, 1, 1).");
            }

            // ── CharacterAnimationController ─────────────────────────────────
            var animCtrl = instance.GetComponent<CharacterAnimationController>();
            if (animCtrl == null)
                animCtrl = instance.AddComponent<CharacterAnimationController>();

            // ── AudioSources ──────────────────────────────────────────────────
            var audioSrcs = instance.GetComponents<AudioSource>();
            AudioSource attackSrc, deathSrc;
            if      (audioSrcs.Length >= 2) { attackSrc = audioSrcs[0]; deathSrc = audioSrcs[1]; }
            else if (audioSrcs.Length == 1) { attackSrc = audioSrcs[0]; deathSrc = instance.AddComponent<AudioSource>(); }
            else                            { attackSrc = instance.AddComponent<AudioSource>(); deathSrc = instance.AddComponent<AudioSource>(); }

            attackSrc.playOnAwake = false; attackSrc.loop = false;
            deathSrc.playOnAwake  = false; deathSrc.loop  = false;

            var attackAudioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(ATTACK_AUDIO_PATH);
            var deathAudioClip  = AssetDatabase.LoadAssetAtPath<AudioClip>(DEATH_AUDIO_PATH);

            var so = new SerializedObject(animCtrl);
            so.FindProperty("attackAudioSource").objectReferenceValue = attackSrc;
            so.FindProperty("deathAudioSource").objectReferenceValue  = deathSrc;
            so.FindProperty("attackClip").objectReferenceValue        = attackAudioClip;
            so.FindProperty("deathClip").objectReferenceValue         = deathAudioClip;
            so.ApplyModifiedProperties();

            // ── Visual / ModelSlot ────────────────────────────────────────────
            Transform visual    = instance.transform.Find("Visual");
            Transform modelSlot = visual != null ? visual.Find("ModelSlot") : null;

            if (modelSlot == null)
            {
                Debug.LogWarning("[Phase19AKnightSetup] ⚠️ Không tìm thấy Visual/ModelSlot.");
            }
            else
            {
                // Xóa children cũ
                for (int i = modelSlot.childCount - 1; i >= 0; i--)
                    Object.DestroyImmediate(modelSlot.GetChild(i).gameObject);

                // Instantiate T-Pose model
                var modelInstance = PrefabUtility.InstantiatePrefab(modelFbxAsset, modelSlot) as GameObject;
                if (modelInstance == null)
                    modelInstance = Object.Instantiate(modelFbxAsset, modelSlot);

                modelInstance.name = "Meshy_AI_Ironbound_Warlord";
                // localPosition.y = -1 như Archer: root prefab đặt tại cellTop + 1.0,
                // CapsuleCollider cao 2 đơn vị, chân model phải tại localY = -1
                modelInstance.transform.localPosition = new Vector3(0f, -1f, 0f);
                modelInstance.transform.localRotation = Quaternion.identity;
                modelInstance.transform.localScale    = Vector3.one;

                // ── Material (không SaveAndReimport ở đây) ───────────────────
                ApplyMaterialToInstance(modelInstance);

                // ── Animator (Generic rig với Avatar thực tế) ────────────────
                Animator animator = modelInstance.GetComponent<Animator>();
                if (animator == null) animator = modelInstance.AddComponent<Animator>();

                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.updateMode      = AnimatorUpdateMode.Normal;

                // Load Avatar từ T-Pose FBX model
                Avatar avatar = null;
                var subAssets = AssetDatabase.LoadAllAssetsAtPath(MODEL_FBX_PATH);
                foreach (var asset in subAssets)
                {
                    if (asset is Avatar av)
                    {
                        avatar = av;
                        break;
                    }
                }
                animator.avatar = avatar;
                if (avatar != null)
                {
                    Debug.Log($"[Phase19AKnightSetup] ✅ Đã gán Avatar '{avatar.name}' cho Animator.");
                }
                else
                {
                    Debug.LogWarning("[Phase19AKnightSetup] ⚠️ Không tìm thấy Avatar trong model FBX.");
                }

                // Ẩn placeholder
                Transform placeholder = visual.Find("Placeholder");
                if (placeholder != null) placeholder.gameObject.SetActive(false);

                Debug.Log("[Phase19AKnightSetup] ✅ Model đặt vào Visual/ModelSlot với localPosition.y=-1, scale=(1,1,1).");
            }

            // FindAnimator để CharacterAnimationController tìm Animator con
            animCtrl.FindAnimator();

            // Lưu prefab
            PrefabUtility.SaveAsPrefabAsset(instance, KNIGHT_PREFAB_PATH);
            Object.DestroyImmediate(instance);

            // Remap material FBX SAU khi prefab đã được lưu (không gọi trong prefab context)
            RemapFBXMaterial();

            Debug.Log($"[Phase19AKnightSetup] ✅ Knight_Unit_Prefab đã được cập nhật.");
        }

        // ─── Gán Material cho renderers trong instance (không SaveAndReimport) ─
        private static void ApplyMaterialToInstance(GameObject modelInstance)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(MAT_PATH);
            if (mat == null) { Debug.LogWarning($"[Phase19AKnightSetup] ⚠️ Không tìm thấy: {MAT_PATH}"); return; }

            var baseTex  = AssetDatabase.LoadAssetAtPath<Texture2D>(TEX_PATH);
            var metalTex = AssetDatabase.LoadAssetAtPath<Texture2D>(METALLIC_PATH);

            if (baseTex  != null) { mat.SetTexture("_BaseMap", baseTex);  mat.SetTexture("_MainTex", baseTex); }
            if (metalTex != null) { mat.SetTexture("_MetallicGlossMap", metalTex); mat.EnableKeyword("_METALLICGLOSSMAP"); }
            EditorUtility.SetDirty(mat);

            // Gán material trực tiếp cho SkinnedMeshRenderer
            var renderers = modelInstance.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                var mats = new Material[r.sharedMaterials.Length];
                for (int m = 0; m < mats.Length; m++) mats[m] = mat;
                r.sharedMaterials = mats;
            }
            Debug.Log($"[Phase19AKnightSetup] Material '{mat.name}' gán cho {renderers.Length} renderers.");
        }

        // ─── Remap FBX material (gọi SAU khi prefab đã được save) ────────────
        private static void RemapFBXMaterial()
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(MAT_PATH);
            if (mat == null) return;

            var mi = AssetImporter.GetAtPath(MODEL_FBX_PATH) as ModelImporter;
            if (mi == null) return;

            // Load all sub-assets of the FBX model to find the embedded Material names dynamically
            var assets = AssetDatabase.LoadAllAssetsAtPath(MODEL_FBX_PATH);
            bool dirty = false;
            foreach (var asset in assets)
            {
                if (asset != null && asset.GetType() == typeof(Material))
                {
                    var key = new AssetImporter.SourceAssetIdentifier(typeof(Material), asset.name);
                    var remaps = mi.GetExternalObjectMap();
                    if (!remaps.ContainsKey(key) || remaps[key] != mat)
                    {
                        mi.AddRemap(key, mat);
                        dirty = true;
                        Debug.Log($"[Phase19AKnightSetup] Remapped FBX material '{asset.name}' → '{mat.name}'");
                    }
                }
            }

            if (dirty)
            {
                mi.SaveAndReimport();
            }
        }

        // ─── Checklist ────────────────────────────────────────────────────────
        private static void PrintChecklist()
        {
            Debug.Log(
                "[Phase19AKnightSetup] ── CHECKLIST ──────────────────────────────────\n" +
                "□ 1. Mua Knight → Model xuất hiện trên bàn (không còn invisible/chấm nhỏ).\n" +
                "□ 2. Knight đứng yên → Idle animation (Alert) chạy.\n" +
                "□ 3. Knight di chuyển → Move animation (You Groove) chạy.\n" +
                "□ 4. Tấn công → Attack animation play 1 lần rồi quay về Idle/Move.\n" +
                "□ 5. Chết → Death animation (Skill 01) play rồi Destroy.\n" +
                "□ 6. Không bị trượt vị trí (applyRootMotion = false).\n" +
                "□ 7. Texture đúng màu (không trắng).\n" +
                "□ 8. Không có lỗi trong Console sau khi chạy Setup.\n" +
                "───────────────────────────────────────────────────────────────────────"
            );
        }
    }
}
#endif
