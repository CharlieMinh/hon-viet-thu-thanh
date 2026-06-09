# 03 - Dev2 Enemy & Wave Task

## 1. Role Summary

Dev2 owns enemy spawning, enemy movement, enemy health, and wave events. In Phase 1, Dev2 proves that enemy placeholder cubes can spawn, move along lanes, take damage, die, or reach the base.

## 2. Branch To Use

Use branch:

```bash
feature/dev2
```

## 3. Folder Ownership

Primary folder:

```text
Assets/Project/Dev2_EnemyWave/
```

Dev2 may work in:

- `Scripts/`
- `Prefabs/`
- `Scenes/`
- `Materials/`

## 4. Scene Ownership

Dev2 owns:

```text
Assets/Project/Dev2_EnemyWave/Scenes/Scene_Dev2_EnemyWave.unity
```

## 5. What To Implement In Phase 1

- Spawn enemy placeholder cube at lane start.
- Move enemy from lane start to lane end.
- Give enemy HP.
- Let enemy receive damage through `IDamageable`.
- Destroy or deactivate enemy when HP reaches 0.
- Fire `OnEnemyDied`.
- Fire `OnEnemyReachedBase` when enemy reaches end.
- Keep wave logic simple.

## 6. What Not To Implement Yet

- Final enemy models.
- Boss skills.
- Phased wave balance.
- Complex NavMesh or A* pathfinding.
- Enemy attack animations.
- Projectile logic, except receiving damage from Dev3.
- UI.

## 7. Required Scripts / Classes To Create Later

Suggested scripts:

- `Enemy.cs`
- `EnemyMover.cs`
- `EnemySpawner.cs`
- `WaveManager.cs`
- `LanePath.cs`

Use shared contracts:

- `EnemyType`
- `IDamageable`
- `ITargetable`
- `GameEvents.OnEnemyDied`
- `GameEvents.OnEnemyReachedBase`

## 8. Required Scene GameObjects To Create Later

In Dev2 scene:

- `EnemySpawner`
- `LaneStart`
- `LaneEnd`
- `EnemyRoot`
- Enemy placeholder prefab
- Main Camera
- Directional Light

Optional for testing:

- A debug button or keyboard shortcut to spawn one enemy.

## 9. Input / Output Contract With Other Modules

Input:

- Damage from Dev3 projectile through `IDamageable.TakeDamage(float amount)`.

Output:

- `OnEnemyDied(GameObject enemy, int goldReward)`
- `OnEnemyReachedBase(GameObject enemy)`

Expected enemy behavior:

- Enemy exposes current position.
- Enemy can be detected by Dev3.
- Enemy is removed cleanly after death or reaching base.

## 10. Step-By-Step Implementation Checklist

- [ ] Create enemy placeholder prefab.
- [ ] Add collider to enemy.
- [ ] Add HP field.
- [ ] Implement `TakeDamage`.
- [ ] Implement death check.
- [ ] Fire `OnEnemyDied` on death.
- [ ] Create lane start and lane end transforms.
- [ ] Move enemy toward lane end.
- [ ] Fire `OnEnemyReachedBase` at lane end.
- [ ] Remove enemy after reaching base.
- [ ] Add simple spawn trigger.
- [ ] Test in `Scene_Dev2_EnemyWave.unity`.

## 11. Self-Test Checklist

- [ ] Enemy spawns at lane start.
- [ ] Enemy moves toward lane end.
- [ ] Enemy reaches lane end and fires event.
- [ ] Enemy can receive damage.
- [ ] Enemy dies when HP reaches 0.
- [ ] Dead enemy does not also count as reaching base.
- [ ] Scene runs without null reference errors.

## 12. Definition Of Done

- One enemy can spawn and move.
- Enemy HP and death work.
- `OnEnemyDied` works.
- `OnEnemyReachedBase` works.
- Dev2 scene demonstrates enemy logic without needing other modules.

## 13. Handoff Notes For Integration

Tell Dev5:

- Enemy prefab path.
- Required spawner setup.
- Lane start/end setup.
- Event behavior.

Tell Dev3:

- How projectile should damage enemy.
- Which component implements `IDamageable`.

Tell Dev4:

- Event payloads for enemy death and base damage.

## 14. Suggested Commit Messages

```bash
git commit -m "feat(dev2): add enemy placeholder movement"
git commit -m "feat(dev2): add enemy health and death event"
git commit -m "feat(dev2): add base reach event"
```

