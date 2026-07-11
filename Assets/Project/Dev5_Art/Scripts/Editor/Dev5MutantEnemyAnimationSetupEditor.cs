#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace HonVietThuThanh.Dev5Editor
{
    /// <summary>
    /// Creates the Dev5-only Mutant animator controller and prefab without touching integration assets.
    /// </summary>
    public static class Dev5MutantEnemyAnimationSetupEditor
    {
        private const string BasePrefabPath = "Assets/Project/Dev5_Art/Prefabs/Enemies/Goblin_Enemy_Prefab.prefab";
        private const string OutputPrefabPath = "Assets/Project/Dev5_Art/Prefabs/Enemies/Mutant_Enemy_Prefab.prefab";
        private const string ControllerPath = "Assets/Project/Dev5_Art/Animations/Enemies/EnemyArcher/Controllers/Mutant_Animator.controller";
        private const string ModelPath = "Assets/Project/Dev5_Art/Models/Enemies/EnemyArcher/FBX/Mutant.fbx";
        private const string IdlePath = "Assets/Project/Dev5_Art/Animations/Enemies/EnemyArcher/Source/Mutant@MutantIdle.fbx";
        private const string WalkPath = "Assets/Project/Dev5_Art/Animations/Enemies/EnemyArcher/Source/Mutant@MutantWalking.fbx";
        private const string RunPath = "Assets/Project/Dev5_Art/Animations/Enemies/EnemyArcher/Source/Mutant@MutantRun.fbx";
        private const string AttackPath = "Assets/Project/Dev5_Art/Animations/Enemies/EnemyArcher/Source/Mutant@MutantActtackPunch.fbx";
        private const string DeathPath = "Assets/Project/Dev5_Art/Animations/Enemies/EnemyArcher/Source/Mutant@StandingDeathLeft.fbx";

        [MenuItem("Dev5/Setup Mutant Enemy Animation")]
        public static void SetupMutantEnemyAnimation()
        {
            // Configure Animation Events on FBX clips before loading them
            ConfigureAnimationEvents(WalkPath, "MutantWalking", (0.3f, "OnFootstep"), (0.8f, "OnFootstep"));
            ConfigureAnimationEvents(RunPath, "MutantRun", (0.25f, "OnFootstep"), (0.65f, "OnFootstep"));
            ConfigureAnimationEvents(AttackPath, "MutantActtackPunch", (0.4f, "OnAttackImpact"));

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            }

            AnimationClip idleClip = LoadPrimaryClip(IdlePath, "MutantIdle");
            AnimationClip walkClip = LoadPrimaryClip(WalkPath, "MutantWalking");
            AnimationClip runClip = LoadPrimaryClip(RunPath, "MutantRun");
            AnimationClip attackClip = LoadPrimaryClip(AttackPath, "MutantActtackPunch");
            AnimationClip deathClip = LoadPrimaryClip(DeathPath, "StandingDeathLeft");

            if (idleClip == null || walkClip == null || runClip == null || attackClip == null || deathClip == null)
            {
                Debug.LogError("[MutantSetup] Missing one or more required animation clips. Aborting setup.");
                return;
            }

            SetupController(controller, idleClip, walkClip, runClip, attackClip, deathClip);
            CreateOrUpdatePrefab(controller);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MutantSetup] Mutant enemy animation setup completed.");
        }

        private static void ConfigureAnimationEvents(string fbxPath, string clipName, params (float time, string functionName)[] events)
        {
            ModelImporter modelImporter = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            if (modelImporter == null)
            {
                Debug.LogWarning($"[MutantSetup] ModelImporter not found for '{fbxPath}'");
                return;
            }

            ModelImporterClipAnimation[] clips = modelImporter.clipAnimations;
            if (clips == null || clips.Length == 0)
            {
                clips = modelImporter.defaultClipAnimations;
            }

            if (clips == null || clips.Length == 0)
            {
                Debug.LogWarning($"[MutantSetup] No clip animations found in '{fbxPath}'");
                return;
            }

            bool changed = false;
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i].name == clipName || clips.Length == 1)
                {
                    List<AnimationEvent> existingEvents = clips[i].events != null ? 
                        new List<AnimationEvent>(clips[i].events) : new List<AnimationEvent>();
                    
                    bool anyNew = false;
                    foreach (var evt in events)
                    {
                        bool exists = false;
                        foreach (var existing in existingEvents)
                        {
                            if (existing.functionName == evt.functionName && Mathf.Abs(existing.time - evt.time) < 0.05f)
                            {
                                exists = true;
                                break;
                            }
                        }

                        if (!exists)
                        {
                            AnimationEvent newEvent = new AnimationEvent();
                            newEvent.functionName = evt.functionName;
                            newEvent.time = evt.time;
                            existingEvents.Add(newEvent);
                            anyNew = true;
                        }
                    }

                    if (anyNew)
                    {
                        clips[i].events = existingEvents.ToArray();
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                modelImporter.clipAnimations = clips;
                modelImporter.SaveAndReimport();
                Debug.Log($"[MutantSetup] Successfully injected animation events into FBX: {fbxPath}");
            }
        }

        private static void SetupController(
            AnimatorController controller,
            AnimationClip idleClip,
            AnimationClip walkClip,
            AnimationClip runClip,
            AnimationClip attackClip,
            AnimationClip deathClip)
        {
            EnsureParameter(controller, "IsMoving", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "IsRunning", AnimatorControllerParameterType.Bool);
            EnsureParameter(controller, "Attack", AnimatorControllerParameterType.Trigger);
            EnsureParameter(controller, "Death", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            AnimatorState idleState = FindOrCreateState(stateMachine, "Idle", idleClip);
            AnimatorState walkState = FindOrCreateState(stateMachine, "Walk", walkClip);
            AnimatorState runState = FindOrCreateState(stateMachine, "Run", runClip);
            AnimatorState attackState = FindOrCreateState(stateMachine, "Attack", attackClip);
            AnimatorState deathState = FindOrCreateState(stateMachine, "Death", deathClip);

            stateMachine.defaultState = idleState;

            idleState.transitions = new AnimatorStateTransition[0];
            walkState.transitions = new AnimatorStateTransition[0];
            runState.transitions = new AnimatorStateTransition[0];
            attackState.transitions = new AnimatorStateTransition[0];
            deathState.transitions = new AnimatorStateTransition[0];
            stateMachine.anyStateTransitions = new AnimatorStateTransition[0];

            AnimatorStateTransition idleToWalk = idleState.AddTransition(walkState);
            idleToWalk.hasExitTime = false;
            idleToWalk.duration = 0.1f;
            idleToWalk.AddCondition(AnimatorConditionMode.If, 0f, "IsMoving");
            idleToWalk.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsRunning");

            AnimatorStateTransition walkToIdle = walkState.AddTransition(idleState);
            walkToIdle.hasExitTime = false;
            walkToIdle.duration = 0.1f;
            walkToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsMoving");

            AnimatorStateTransition walkToRun = walkState.AddTransition(runState);
            walkToRun.hasExitTime = false;
            walkToRun.duration = 0.1f;
            walkToRun.AddCondition(AnimatorConditionMode.If, 0f, "IsRunning");

            AnimatorStateTransition runToWalk = runState.AddTransition(walkState);
            runToWalk.hasExitTime = false;
            runToWalk.duration = 0.1f;
            runToWalk.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsRunning");
            runToWalk.AddCondition(AnimatorConditionMode.If, 0f, "IsMoving");

            AnimatorStateTransition runToIdle = runState.AddTransition(idleState);
            runToIdle.hasExitTime = false;
            runToIdle.duration = 0.1f;
            runToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsMoving");

            AnimatorStateTransition anyToAttack = stateMachine.AddAnyStateTransition(attackState);
            anyToAttack.hasExitTime = false;
            anyToAttack.duration = 0.1f;
            anyToAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");

            AnimatorStateTransition attackToIdle = attackState.AddTransition(idleState);
            attackToIdle.hasExitTime = true;
            attackToIdle.exitTime = 1f;
            attackToIdle.duration = 0.15f;

            AnimatorStateTransition anyToDeath = stateMachine.AddAnyStateTransition(deathState);
            anyToDeath.hasExitTime = false;
            anyToDeath.duration = 0.1f;
            anyToDeath.AddCondition(AnimatorConditionMode.If, 0f, "Death");
        }

        private static void CreateOrUpdatePrefab(AnimatorController controller)
        {
            GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BasePrefabPath);
            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (basePrefab == null || modelPrefab == null)
            {
                Debug.LogError("[MutantSetup] Missing base prefab or model prefab.");
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(basePrefab) as GameObject;
            if (instance == null)
            {
                Debug.LogError("[MutantSetup] Failed to instantiate base prefab.");
                return;
            }

            instance.name = "Mutant_Enemy_Prefab";

            Transform visual = instance.transform.Find("Visual");
            Transform modelSlot = visual != null ? visual.Find("ModelSlot") : null;
            Transform placeholder = visual != null ? visual.Find("Placeholder") : null;
            if (visual == null || modelSlot == null)
            {
                Object.DestroyImmediate(instance);
                Debug.LogError("[MutantSetup] Base prefab is missing Visual/ModelSlot.");
                return;
            }

            for (int i = modelSlot.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(modelSlot.GetChild(i).gameObject);
            }

            if (placeholder != null)
            {
                placeholder.gameObject.SetActive(false);
            }

            GameObject modelInstance = PrefabUtility.InstantiatePrefab(modelPrefab) as GameObject;
            if (modelInstance == null)
            {
                Object.DestroyImmediate(instance);
                Debug.LogError("[MutantSetup] Failed to instantiate Mutant model.");
                return;
            }

            modelInstance.transform.SetParent(modelSlot, false);
            modelInstance.transform.localPosition = Vector3.zero;
            modelInstance.transform.localRotation = Quaternion.identity;
            modelInstance.transform.localScale = Vector3.one;

            Animator animator = modelInstance.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Object.DestroyImmediate(modelInstance);
                Object.DestroyImmediate(instance);
                Debug.LogError("[MutantSetup] Mutant model has no Animator component.");
                return;
            }

            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            HonVietThuThanh.Dev5.EnemyAnimationController animationController = instance.GetComponent<HonVietThuThanh.Dev5.EnemyAnimationController>();
            if (animationController == null)
            {
                animationController = instance.AddComponent<HonVietThuThanh.Dev5.EnemyAnimationController>();
            }
            animationController.RebindAnimator();

            PrefabUtility.SaveAsPrefabAsset(instance, OutputPrefabPath);
            Object.DestroyImmediate(instance);
        }

        private static AnimationClip LoadPrimaryClip(string assetPath, string preferredName)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            AnimationClip clip = assets
                .OfType<AnimationClip>()
                .FirstOrDefault(c => c.name == preferredName);

            if (clip != null)
            {
                return clip;
            }

            return assets
                .OfType<AnimationClip>()
                .FirstOrDefault(c => !c.name.StartsWith("__preview__"));
        }

        private static void EnsureParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
        {
            foreach (AnimatorControllerParameter parameter in controller.parameters)
            {
                if (parameter.name == name && parameter.type == type)
                {
                    return;
                }
            }

            controller.AddParameter(name, type);
        }

        private static AnimatorState FindOrCreateState(AnimatorStateMachine stateMachine, string name, Motion motion)
        {
            foreach (ChildAnimatorState childState in stateMachine.states)
            {
                if (childState.state.name == name)
                {
                    childState.state.motion = motion;
                    return childState.state;
                }
            }

            AnimatorState state = stateMachine.AddState(name);
            state.motion = motion;
            return state;
        }
    }
}
#endif
