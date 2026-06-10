# Dev1 Grid Placement Setup

## Branch

Work on:

```bash
git checkout feature/dev1-ui-safe-placement
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
- Dev1 valid and invalid preview materials
- Dev1 hero placeholder prefab

## Test

1. Press Play.
2. Hover an empty visible grid cell.
3. A valid hero preview should appear on that cell.
4. Move the mouse to another cell.
5. The preview should move to the new cell.
6. Click an empty grid cell.
7. A real hero cube should appear on that cell.
8. Hover or click the same occupied cell again.
9. The preview should show the invalid state and no second hero should appear.
10. Move the mouse outside the grid.
11. The preview should hide.
12. If the scene has a UI button or panel, click it while a grid cell is behind it.
13. No hero should be placed from that UI click.
14. Check the Console for:

```text
[Dev1 Placement] Hero placed: ThanhGiong at column X, row Y
```

## Expected Behavior

- Grid is visible.
- Grid is 5 rows x 8 columns.
- Each cell is clickable.
- Empty cell hover shows a valid hero preview.
- Occupied cell hover shows an invalid hero preview.
- Moving outside the grid hides the preview.
- A hero placeholder cube appears on a valid clicked cell.
- The same cell cannot be used twice.
- Clicking over Unity UI does not place a hero.
- `GameEvents.RaiseHeroPlaced` fires after successful placement.
- `GameEvents.RaiseHeroPlaced` does not fire after invalid placement.
- `GameEvents.RaiseHeroPlaced` does not fire after a UI-blocked click.

## Not In Dev1 Phase 3

Dev1 does not implement:

- enemy spawning
- wave logic
- combat
- projectiles
- GameManager
- economy
- full UI implementation
- drag preview
- remove or sell hero
- final hero models
