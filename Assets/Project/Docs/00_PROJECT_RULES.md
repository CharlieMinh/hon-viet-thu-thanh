# 00 - Project Rules

## Project Overview

Hon Viet Thu Thanh is a Unity 3D tower defense + strategy game. The Phase 1 goal is to build a rough playable prototype, not a polished game.

Core direction:

- Unity 3D tower defense + strategy.
- Lane-defense inspiration from Plants vs. Zombies.
- Tactical placement inspiration from TFT-style board positioning.
- Vietnamese mythology and history theme.
- Low-poly 3D with a fixed isometric camera.
- Event-driven module architecture.
- 5 developers, 5 independent modules.

## Phase 1 Goal

Phase 1 should prove that the game loop can run:

1. A hero can be placed on a 5x8 grid.
2. Enemies can spawn and move down lanes.
3. Heroes can detect and attack enemies.
4. Enemy death and enemy reaching base produce events.
5. GameManager can track base HP and gold through logs.
6. Integration scene can run without crashes.

## Branch Mapping

| Branch | Purpose |
|---|---|
| `main` | Stable shared baseline. |
| `develop` | Integration branch. |
| `feature/dev1` | Dev1 Grid & Hero Placement work. |
| `feature/dev2` | Dev2 Enemy & Wave work. |
| `feature/dev3` | Dev3 Combat & Hero Skills work. |
| `feature/dev4` | Dev4 GameManager, UI & Rules work. |
| `feature/dev5` | Dev5 Scene, Art, Prefab, Audio & Integration work. |

## Ownership Rules

- Each developer owns their module folder and module scene.
- Do not edit another developer's scene without permission.
- Do not edit `Assets/Project/Shared` without notifying the team.
- `Scene_Integration.unity` is controlled only by Dev5 or the leader.
- Do not push directly to `main` or `develop` during normal feature work.
- Pull `develop` before starting work each day.
- Commit small and often.

## Architecture Rules

Use events and shared contracts instead of direct cross-module object searching.

Preferred:

- `GameEvents` for cross-module notifications.
- `HeroType` and `EnemyType` enums for shared identity.
- `IDamageable` for anything that can receive damage.
- `ITargetable` for anything combat can target.

Avoid in Phase 1:

- Hard dependencies on another developer's concrete class.
- `FindObjectOfType<T>()` as a normal communication path.
- Editing another module's scripts to make your module work.
- Building final art, final balance, or advanced UI before the core loop works.

## Phase 1 Non-Goals

Do not spend Phase 1 on:

- Final 3D models.
- Final animations.
- Final audio.
- Advanced skill effects.
- Shop, reroll, synergy, or TFT economy.
- Multiple chapters.
- Multiplayer.
- Complex pathfinding if straight lane movement is enough for the prototype.

## Definition of Project Phase 1 Done

Phase 1 is done when the integration scene can demonstrate:

- Place a hero on a grid cell.
- Spawn at least one enemy.
- Enemy moves along a lane.
- Hero detects and attacks enemy.
- Projectile damages enemy through `IDamageable`.
- Enemy can die and fire `OnEnemyDied`.
- Enemy can reach base and fire `OnEnemyReachedBase`.
- GameManager logs base HP / gold changes.
- Scene runs for at least 1 minute without crashing.

