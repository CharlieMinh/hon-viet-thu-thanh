# Dev1 Phase 1 Handoff

## 1. Dev1 Phase 1 Status

Dev1 Grid and Hero Placement Phase 1 is implemented, manually tested, merged into `develop`, and ready for other modules to consume.

Completed behavior:

- Generates a 5 row x 8 column placement grid.
- Allows clicking an empty cell to place a hero placeholder cube.
- Prevents placement on an occupied cell.
- Prevents placement outside the grid because only grid cells receive placement clicks.
- Raises `GameEvents.RaiseHeroPlaced(HeroType, Vector2Int)` after successful placement.
- Includes a Dev1 debug logger for placement event testing.
- Includes a Dev1 scene setup tool.
- Includes `Scene_Dev1_Placement`, test prefab, and test materials.

## 2. Folder Structure

Dev1 owns this folder:

```text
Assets/Project/Dev1_GridPlacement/
```

Important Dev1 files:

```text
Assets/Project/Dev1_GridPlacement/Scripts/GridCell.cs
Assets/Project/Dev1_GridPlacement/Scripts/HeroPlacementManager.cs
Assets/Project/Dev1_GridPlacement/Scripts/HeroPlacementDebugLogger.cs
Assets/Project/Dev1_GridPlacement/Editor/Dev1PlacementSceneSetup.cs
Assets/Project/Dev1_GridPlacement/Scenes/Scene_Dev1_Placement.unity
Assets/Project/Dev1_GridPlacement/Prefabs/PF_Dev1_HeroPlaceholder.prefab
Assets/Project/Dev1_GridPlacement/Materials/MAT_Dev1_Cell.mat
Assets/Project/Dev1_GridPlacement/Materials/MAT_Dev1_CellOccupied.mat
```

## 3. How To Open And Test Scene_Dev1_Placement

1. Checkout or pull the latest `develop` branch.
2. Open the Unity project.
3. Open this scene:

```text
Assets/Project/Dev1_GridPlacement/Scenes/Scene_Dev1_Placement.unity
```

4. Press Play.
5. Click any visible grid cell.
6. Confirm a hero placeholder cube appears on the clicked cell.
7. Click the same cell again and confirm no second hero is placed.
8. Click outside the grid and confirm there is no error.

Expected Console log:

```text
[Dev1 Placement] Hero placed: ThanhGiong at column X, row Y
```

## 4. How To Run The Setup Tool

Open the Dev1 placement scene, then run:

```text
Tools > Hon Viet Thu Thanh > Dev1 > Setup Placement Scene
```

The setup tool creates or reuses:

- `Dev1_PlacementManager`
- `Dev1_HeroPlacementDebugLogger`
- `Dev1_GridRoot`
- `Dev1_HeroRoot`
- Dev1 cell material
- Dev1 occupied cell material
- Dev1 hero placeholder prefab

It also assigns the prefab, materials, grid root, hero root, and a basic isometric test camera view.

## 5. How Placement Works

`HeroPlacementManager` generates the placement grid at runtime when `generateGridOnStart` is enabled.

Each generated cell:

- Is named `GridCell_column_row`.
- Has a visible cube mesh.
- Has a `BoxCollider`.
- Has a `GridCell` component.
- Stores its grid coordinate as `Vector2Int`.

When the player clicks a cell:

1. `GridCell.OnMouseDown()` receives the click.
2. `GridCell` delegates to `HeroPlacementManager.TryPlaceHero(this)`.
3. `HeroPlacementManager` checks that the cell exists and is not occupied.
4. A hero placeholder cube is spawned at the cell center.
5. The cell is marked occupied.
6. The occupied material is applied if assigned.
7. `GameEvents.RaiseHeroPlaced(...)` is raised.

## 6. Event Contract

Dev1 raises this shared event after successful placement:

```csharp
GameEvents.RaiseHeroPlaced(HeroType heroType, Vector2Int gridPosition);
```

Coordinate rules:

- `Vector2Int.x = column`
- `Vector2Int.y = row`
- Valid columns: `0..7`
- Valid rows: `0..4`

Current default hero type:

```text
HeroType.ThanhGiong
```

Dev1 does not raise this event on failed placement.

## 7. What Dev3 Combat Can Consume Later

Dev3 can listen to `GameEvents.OnHeroPlaced` to know when and where a hero was placed.

Useful data for Dev3:

- `HeroType` tells which hero type was placed.
- `Vector2Int` tells the grid coordinate.
- The placed hero GameObject is currently a placeholder cube, so Dev3 should not rely on final model components yet.

Recommended next step for Dev3:

- Add combat components to a future hero prefab or use placement events to register placed heroes with a combat system.

## 8. What Dev4 GameManager Can Consume Later

Dev4 can listen to `GameEvents.OnHeroPlaced` to update game state.

Possible Dev4 uses:

- Log placement events.
- Track placed hero count.
- Apply future gold cost rules.
- Validate future match rules.
- Update future UI state after a hero is placed.

Dev1 currently does not manage gold, base HP, wave state, or victory/loss rules.

## 9. What Dev5 Integration Can Consume Later

Dev5 can use the Dev1 scene, setup tool, prefab, and materials as a simple integration baseline.

Possible Dev5 uses:

- Copy or recreate the grid setup in `Scene_Integration`.
- Replace placeholder visuals with approved low-poly assets later.
- Adjust camera, lighting, and ground layout around the grid.
- Keep the same placement event contract while improving presentation.

Dev5 should avoid changing Dev1 gameplay scripts unless coordinated with Dev1.

## 10. Current Limitations

- Hero is a placeholder cube.
- No gold cost yet.
- No UI hero selection yet.
- No drag preview yet.
- No remove or sell hero feature yet.
- No enemy interaction yet.
- No combat behavior yet.
- No final art model integration yet.

## 11. Manual Test Checklist

Before handoff or integration, verify:

- `Scene_Dev1_Placement.unity` opens without missing script errors.
- Setup tool can run from the Unity menu.
- Grid is visible in Play Mode.
- Grid size is 5 rows x 8 columns.
- Camera sees the full grid.
- Clicking an empty cell places one hero placeholder cube.
- Clicking the same occupied cell does not create another hero.
- Clicking outside the grid causes no error.
- Console logs the placement event.
- No red Console errors appear.
- `GameEvents.RaiseHeroPlaced(HeroType, Vector2Int)` is raised only after successful placement.
