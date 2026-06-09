# Dev1 Grid Placement Setup

## Branch

Work on:

```bash
git checkout feature/dev1
```

## Scene

Open:

```text
Assets/Project/Dev1_GridPlacement/Scenes/Scene_Dev1_Placement.unity
```

## Setup

Run the Unity menu item:

```text
Tools > Hon Viet Thu Thanh > Dev1 > Setup Placement Scene
```

This creates or reuses:

- `Dev1_PlacementManager`
- `Dev1_HeroPlacementDebugLogger`
- `Dev1_GridRoot`
- `Dev1_HeroRoot`
- Dev1 cell and occupied materials
- Dev1 hero placeholder prefab

## Test

1. Press Play.
2. Click a visible grid cell.
3. A hero cube should appear on that cell.
4. Click the same cell again.
5. No second hero should appear.
6. Check the Console for:

```text
[Dev1 Placement] Hero placed: ThanhGiong at column X, row Y
```

## Expected Behavior

- Grid is visible.
- Grid is 5 rows x 8 columns.
- Each cell is clickable.
- A hero placeholder cube appears on a valid clicked cell.
- The same cell cannot be used twice.
- `GameEvents.RaiseHeroPlaced` fires after successful placement.

## Not In Dev1 Phase 1

Dev1 does not implement:

- enemy spawning
- wave logic
- combat
- projectiles
- GameManager
- economy
- full UI
- final hero models
