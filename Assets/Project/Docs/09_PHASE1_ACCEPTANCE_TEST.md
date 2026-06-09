# 09 - Phase 1 Acceptance Test

## Purpose

This document defines the minimum test for accepting Phase 1 of Hon Viet Thu Thanh.

Phase 1 is accepted when the team can show the core prototype loop in Unity with placeholder assets.

## Test Scene

Primary acceptance scene:

```text
Assets/Project/Dev5_Art/Scenes/Scene_Integration.unity
```

Module scenes can be used to debug individual failures.

## Required Setup

Before running the test:

- [ ] Shared contracts compile.
- [ ] Integration scene opens.
- [ ] Main Camera exists.
- [ ] Directional Light exists.
- [ ] Ground is visible.
- [ ] Grid is visible.
- [ ] GameManager exists.
- [ ] Enemy spawner exists.
- [ ] Hero placeholder can be placed.

## Acceptance Flow

1. Enter Play Mode.
2. Place one hero on a valid grid cell.
3. Confirm placement log or visible placeholder.
4. Spawn one enemy.
5. Confirm enemy moves from lane start to lane end.
6. Confirm hero detects enemy in range.
7. Confirm hero rotates toward enemy.
8. Confirm projectile sphere is fired.
9. Confirm projectile damages enemy.
10. Confirm enemy dies or reaches base.
11. Confirm GameManager logs state change.
12. Keep scene running for at least 1 minute.

## Expected Results

Dev1:

- [ ] 5x8 grid exists.
- [ ] Valid placement works.
- [ ] Duplicate placement is blocked.
- [ ] `OnHeroPlaced` fires.

Dev2:

- [ ] Enemy spawns.
- [ ] Enemy moves.
- [ ] Enemy has HP.
- [ ] Enemy can die.
- [ ] `OnEnemyDied` fires.
- [ ] `OnEnemyReachedBase` fires if enemy reaches end.

Dev3:

- [ ] Hero detects enemy.
- [ ] Hero rotates toward enemy.
- [ ] Projectile spawns.
- [ ] Projectile damages enemy through `IDamageable`.

Dev4:

- [ ] baseHP starts at 100.
- [ ] gold starts at 150.
- [ ] Base HP changes when enemy reaches base.
- [ ] Enemy death logs or updates gold.
- [ ] Logs are readable.

Dev5:

- [ ] Scene is visible.
- [ ] Camera angle is usable.
- [ ] Lighting is clear.
- [ ] No final art is required.
- [ ] Scene is clean enough for demo.

## Failure Conditions

Phase 1 is not accepted if:

- Play Mode crashes.
- Scene has blocking compile errors.
- Hero cannot be placed.
- Enemy cannot spawn.
- Enemy cannot move.
- Projectile cannot damage enemy.
- GameManager does not react to events.
- Scene cannot run for 1 minute.

## Bug Priority

Critical:

- Compile errors.
- Crash.
- Missing scene references that block Play Mode.

Major:

- Placement does not work.
- Enemy movement does not work.
- Combat damage does not work.
- GameManager event handling does not work.

Minor:

- Logs are unclear.
- Camera angle is awkward.
- Placeholder colors are confusing.

Cosmetic:

- Visual style is not final.
- Placeholder objects are ugly.

## Acceptance Sign-Off

Each dev should confirm:

- My module passes its self-test.
- My module works in integration scene or has clear handoff notes.
- I did not modify another dev's scene without permission.
- I did not modify shared contracts without notifying the team.

