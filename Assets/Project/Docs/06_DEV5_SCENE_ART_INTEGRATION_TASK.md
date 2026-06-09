# 06 - Dev5 Scene, Art & Integration Task

## 1. Role Summary

Dev5 owns scene setup, art folders, placeholder prefabs, audio folders, camera, lighting, and integration scene management. In Phase 1, Dev5 makes sure the test environment is simple, readable, and ready for module integration.

## 2. Branch To Use

Use branch:

```bash
feature/dev5
```

## 3. Folder Ownership

Primary folder:

```text
Assets/Project/Dev5_Art/
```

Dev5 may work in:

- `Models/`
- `Materials/`
- `Audio/`
- `Prefabs/`
- `Scenes/`

## 4. Scene Ownership

Dev5 owns:

```text
Assets/Project/Dev5_Art/Scenes/Scene_Dev5_Art.unity
```

Dev5 / leader controls:

```text
Assets/Project/Dev5_Art/Scenes/Scene_Integration.unity
```

## 5. What To Implement In Phase 1

- Create simple test ground.
- Setup fixed isometric camera at about 45 degrees.
- Ensure lighting is visible.
- Manage `Scene_Integration.unity`.
- Add placeholder materials if needed.
- Keep integration scene clean.
- Help place prefabs from Dev1-Dev4 after modules are ready.

## 6. What Not To Implement Yet

- Final low-poly hero models.
- Final enemy models.
- Final audio.
- Final particles.
- Final map Phu Dong art.
- Heavy post-processing.
- Gameplay scripts owned by Dev1-Dev4.
- Scene polish before the core loop works.

## 7. Required Scripts / Classes To Create Later

Dev5 should avoid gameplay scripts in Phase 1.

Possible utility scripts later:

- `CameraRig.cs` if a fixed camera helper is needed.
- `SimpleSceneBootstrap.cs` only if integration requires it and the team agrees.

## 8. Required Scene GameObjects To Create Later

In Dev5 art scene:

- `Ground`
- `Main Camera`
- `Directional Light`
- Optional placeholder environment objects

In integration scene:

- `Main Camera`
- `Directional Light`
- `Ground`
- Dev1 grid / placement setup
- Dev2 enemy / wave setup
- Dev3 combat prefab setup
- Dev4 GameManager setup

Only add module objects after the owner confirms their setup.

## 9. Input / Output Contract With Other Modules

Input:

- Dev1 grid prefab/setup notes.
- Dev2 enemy spawner prefab/setup notes.
- Dev3 hero combat/projectile setup notes.
- Dev4 GameManager setup notes.

Output:

- Clean integration scene.
- Camera and lighting setup.
- Shared placeholder materials/prefabs where needed.

Expected behavior:

- Dev5 does not silently change another dev's module scene.
- Integration changes should be communicated to all module owners.

## 10. Step-By-Step Implementation Checklist

- [ ] Setup fixed isometric camera.
- [ ] Add visible directional light.
- [ ] Add simple ground plane or cube.
- [ ] Create placeholder materials if needed.
- [ ] Keep art scene clean.
- [ ] Wait for module handoff notes.
- [ ] Add module prefabs to integration scene one at a time.
- [ ] Test after each module is added.
- [ ] Document any integration scene requirements.

## 11. Self-Test Checklist

- [ ] Camera sees the play area.
- [ ] Lighting makes placeholders visible.
- [ ] Integration scene contains no random debug objects.
- [ ] Scene can enter Play Mode without immediate errors.
- [ ] Any module object in integration scene was approved by its owner.

## 12. Definition Of Done

- Dev5 scene has basic ground, camera, and lighting.
- Integration scene is clean and ready for module assembly.
- No final art is imported for Phase 1.
- Dev5 can support the Phase 1 acceptance test.

## 13. Handoff Notes For Integration

Tell the team:

- Camera position and rotation.
- Ground scale and world origin.
- Any tag/layer assumptions.
- Which module prefabs are in integration scene.
- Which module still needs setup.

## 14. Suggested Commit Messages

```bash
git commit -m "feat(dev5): setup phase 1 isometric test scene"
git commit -m "feat(dev5): add placeholder materials"
git commit -m "chore(dev5): assemble initial integration scene"
```

