# 07 - Integration Checklist

## Purpose

This checklist is for Dev5 and the leader when combining Dev1-Dev4 work into `Scene_Integration.unity`.

Integration scene:

```text
Assets/Project/Dev5_Art/Scenes/Scene_Integration.unity
```

## Before Integration

- [ ] All developers have pushed their feature branches.
- [ ] Each module has been tested in its own scene.
- [ ] `develop` is up to date.
- [ ] Integration is done from `develop`, not from `main`.
- [ ] Shared contracts compile.
- [ ] No one is editing `Scene_Integration.unity` except Dev5 or leader.

## Integration Order

Recommended order:

1. Shared contracts.
2. Dev1 grid and placement.
3. Dev2 enemy and wave.
4. Dev3 combat and projectile.
5. Dev4 GameManager and logs/UI.
6. Dev5 camera, lighting, ground, and final integration cleanup.

## Dev1 Integration Checks

- [ ] Grid appears in integration scene.
- [ ] Grid size is 5x8.
- [ ] Cells are clickable.
- [ ] Hero placeholder cube can be placed.
- [ ] Duplicate placement is rejected.
- [ ] `OnHeroPlaced` fires.

## Dev2 Integration Checks

- [ ] Enemy spawner exists.
- [ ] Lane start and lane end are aligned with board.
- [ ] Enemy placeholder cube spawns.
- [ ] Enemy moves toward base.
- [ ] Enemy can die.
- [ ] `OnEnemyDied` fires.
- [ ] `OnEnemyReachedBase` fires.

## Dev3 Integration Checks

- [ ] Hero combat component exists on placed hero or test hero.
- [ ] Enemy can be detected in range.
- [ ] Hero rotates toward enemy.
- [ ] Projectile sphere spawns.
- [ ] Projectile reaches enemy.
- [ ] Projectile calls `IDamageable.TakeDamage`.

## Dev4 Integration Checks

- [ ] GameManager exists in scene.
- [ ] baseHP starts at 100.
- [ ] gold starts at 150.
- [ ] Enemy reaching base subtracts HP.
- [ ] Enemy death logs or adds gold.
- [ ] Hero placement logs correctly.

## Dev5 Integration Checks

- [ ] Camera sees the board.
- [ ] Lighting is visible.
- [ ] Ground is simple and not distracting.
- [ ] No final art or heavy assets are imported for Phase 1.
- [ ] Scene has no random temporary objects.

## Full Phase 1 Flow

- [ ] Enter Play Mode.
- [ ] Place one hero.
- [ ] Spawn one enemy.
- [ ] Enemy moves along lane.
- [ ] Hero detects enemy.
- [ ] Hero shoots projectile.
- [ ] Projectile damages enemy.
- [ ] Enemy dies or reaches base.
- [ ] GameManager logs state change.
- [ ] Scene runs for 1 minute without crash.

## Common Integration Problems

- Missing `.meta` files.
- Scene object references broken after prefab movement.
- Tags/layers assumed by one module but not created.
- Event subscribed multiple times.
- Event not unsubscribed on disable.
- Object names changed without telling module owner.
- Integration scene changed by multiple people at once.

## Integration Done

Integration is done when the team can run one simple demonstration from start to finish without manual inspector changes during Play Mode.

