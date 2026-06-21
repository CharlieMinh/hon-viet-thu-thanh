# Dev1 Placement Handoff

## 1. Dev1 Status

Dev1 Grid and Hero Placement Phase 1 is implemented, manually tested, merged into `develop`, and ready for other modules to consume.
Dev1 Phase 2 adds a placement preview / ghost system for hover feedback.
Dev1 Phase 3 adds UI-safe placement input so UI clicks do not place heroes on grid cells behind the UI.
Dev1 Phase 4 adds an optional economy permission hook before placement side effects.
Dev1 Phase 5 adds a placement-to-combat handoff bridge for future combat integration.

Completed behavior:

- Generates the official 5 column x 8 row placement grid.
- Total grid cells: 40.
- Allows clicking an empty cell to place a hero placeholder cube.
- Prevents placement on an occupied cell.
- Prevents placement outside the grid because only grid cells receive placement clicks.
- Raises `GameEvents.RaiseHeroPlaced(HeroType, Vector2Int)` after successful placement.
- Includes a Dev1 debug logger for placement event testing.
- Includes a Dev1 scene setup tool.
- Includes `Scene_Dev1_Placement`, test prefab, and test materials.
- Blocks placement while the pointer is over Unity UI when an EventSystem exists.
- Can ask an optional economy service to approve and spend a hero placement cost before spawning.
- Can republish successful placement data through a Dev1-owned combat handoff bridge.

Phase 2 preview behavior:

- Hovering an empty grid cell shows a valid hero preview.
- Moving to another grid cell moves the preview.
- Hovering an occupied cell shows an invalid preview.
- Moving outside the grid hides the preview.
- Invalid hover or invalid click does not raise placement events.

Phase 3 UI-safe input behavior:

- Clicking a UI button or panel does not place a hero.
- UI-blocked clicks do not raise placement events.
- If the scene has no `EventSystem`, placement continues to work normally.
- Hover preview behavior remains unchanged.

Phase 4 economy permission behavior:

- `HeroPlacementManager` can reference an optional serialized `MonoBehaviour` economy service.
- The assigned service must implement `HonVietThuThanh.Shared.IPlacementEconomyService`.
- If no service is assigned, valid placement is allowed.
- If an assigned service does not implement the interface, Dev1 logs one warning and allows placement.
- If the service returns `false`, placement is blocked before hero spawn, occupied state, occupied material, and `GameEvents.RaiseHeroPlaced`.
- Placement preview remains cell-only in Phase 4 and does not check affordability.

Phase 5 combat handoff behavior:

- `PlacementToCombatBridge` subscribes to `GameEvents.OnHeroPlaced` in `OnEnable()`.
- `PlacementToCombatBridge` unsubscribes in `OnDisable()`.
- The bridge republishes only successful placements through a Dev1-owned event.
- UI-blocked, occupied-cell-blocked, outside-grid, and economy-denied placement attempts do not reach the bridge because they do not raise `GameEvents.OnHeroPlaced`.
- Dev1 does not implement combat, targeting, attacks, damage, or Dev3 integration logic.
- Dev1 does not reference Dev3 code.

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
Assets/Project/Dev1_GridPlacement/Scripts/Dev1MockPlacementEconomyService.cs
Assets/Project/Dev1_GridPlacement/Scripts/PlacementToCombatBridge.cs
Assets/Project/Dev1_GridPlacement/Editor/Dev1PlacementSceneSetup.cs
Assets/Project/Dev1_GridPlacement/Scenes/Scene_Dev1_Placement.unity
Assets/Project/Dev1_GridPlacement/Prefabs/PF_Dev1_HeroPlaceholder.prefab
Assets/Project/Dev1_GridPlacement/Materials/MAT_Dev1_Cell.mat
Assets/Project/Dev1_GridPlacement/Materials/MAT_Dev1_CellOccupied.mat
Assets/Project/Dev1_GridPlacement/Materials/MAT_Dev1_PreviewValid.mat
Assets/Project/Dev1_GridPlacement/Materials/MAT_Dev1_PreviewInvalid.mat
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
8. Hover an empty cell and confirm a valid preview appears.
9. Hover an occupied cell and confirm an invalid preview appears.
10. Move outside the grid and confirm the preview hides.
11. Click outside the grid and confirm there is no error.

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
- Dev1 valid preview material
- Dev1 invalid preview material
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
3. `HeroPlacementManager` first checks whether placement should be blocked because the pointer is over Unity UI.
4. `HeroPlacementManager` checks that the cell exists and is not occupied.
5. `HeroPlacementManager` resolves the temporary Dev1 placement cost for the selected hero type.
6. If an economy service is assigned, `TrySpendForPlacement(heroType, cost)` must return true.
7. A hero placeholder cube is spawned at the cell center.
8. The cell is marked occupied.
9. The occupied material is applied if assigned.
10. `GameEvents.RaiseHeroPlaced(...)` is raised.
11. If an active `PlacementToCombatBridge` exists, it receives `GameEvents.OnHeroPlaced` and republishes combat handoff data.

Economy contract:

```csharp
public interface IPlacementEconomyService
{
    bool TrySpendForPlacement(HeroType heroType, int cost);
}
```

`TrySpendForPlacement` should return true only when the service accepts the placement and deducts the cost. It should return false without deducting when the player lacks enough gold or Linh Khi.

When the player hovers a cell:

1. `GridCell.OnMouseEnter()` notifies `HeroPlacementManager`.
2. `HeroPlacementManager` shows or moves one reusable preview object.
3. Empty cells use the valid preview material.
4. Occupied cells use the invalid preview material.
5. `GridCell.OnMouseExit()` hides the preview when the mouse leaves the active cell.
6. Preview actions do not raise `GameEvents`.

## 6. Event Contract

Dev1 raises this shared event after successful placement:

```csharp
GameEvents.RaiseHeroPlaced(HeroType heroType, Vector2Int gridPosition);
```

Coordinate rules:

- `Vector2Int.x = column`
- `Vector2Int.y = row`
- Valid columns: `0..4`
- Valid rows: `0..7`
- Total cells: `5 columns x 8 rows = 40`

Current default hero type:

```text
HeroType.ThanhGiong
```

Dev1 does not raise this event on failed placement.
Dev1 does not raise this event for hover preview updates.
Dev1 does not raise this event when placement is blocked by UI.
Dev1 does not raise this event when placement is blocked by economy.

## 7. What Dev3 Combat Can Consume Later

Dev3 can listen to `GameEvents.OnHeroPlaced` directly, or use the Dev1-owned combat handoff bridge when the team wants a dedicated adapter boundary.

Useful data for Dev3:

- `HeroType` tells which hero type was placed.
- `Vector2Int` tells the grid coordinate.
- The placed hero GameObject is currently a placeholder cube, so Dev3 should not rely on final model components yet.

Recommended bridge event:

```csharp
PlacementToCombatBridge.OnHeroPlacementReadyForCombat
```

Payload type:

```csharp
PlacementToCombatBridge.HeroPlacementCombatData
```

Payload fields:

- `HeroType HeroType`
- `Vector2Int GridPosition`
- `int Column`
- `int Row`

Bridge Console log for manual testing:

```text
[Dev1 Combat Bridge] Hero placement ready for combat: ThanhGiong at column X, row Y.
```

Recommended next steps for Dev3:

- Add combat components to a future hero prefab or use placement events to register placed heroes with a combat system.
- Subscribe to `PlacementToCombatBridge.OnHeroPlacementReadyForCombat` from a Dev3-owned component when combat code exists.
- Unsubscribe safely when the Dev3-owned component is disabled or destroyed.

## 8. What Dev4 GameManager Can Consume Later

Dev4 can listen to `GameEvents.OnHeroPlaced` to update game state.

Possible Dev4 uses:

- Log placement events.
- Track placed hero count.
- Implement `IPlacementEconomyService` to approve and deduct placement costs.
- Validate future match rules.
- Update future UI state after a hero is placed.

Dev1 currently does not own gold, base HP, wave state, or victory/loss rules. Dev1 only exposes a temporary cost table and optional permission hook until Dev4 owns the real economy.

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
- Placement has temporary Dev1 hero costs, but no final shared `HeroData` yet.
- Placement preview does not show affordability yet.
- No UI hero selection yet.
- UI-safe input is implemented, but Dev1 does not provide a full UI.
- No drag preview yet. Phase 2 only supports hover preview.
- No remove or sell hero feature yet.
- No enemy interaction yet.
- No combat behavior yet.
- Phase 5 exposes handoff data only; it does not implement combat.
- No final art model integration yet.

## 11. Manual Test Checklist

Before handoff or integration, verify:

- `Scene_Dev1_Placement.unity` opens without missing script errors.
- Setup tool can run from the Unity menu.
- Grid is visible in Play Mode.
- Grid size is 5 columns x 8 rows.
- Total grid cells: 40.
- Valid coordinates are column `0..4` and row `0..7`.
- Camera sees the full grid.
- Hovering an empty cell shows the valid preview.
- Moving between cells moves the preview.
- Hovering an occupied cell shows the invalid preview.
- Moving outside the grid hides the preview.
- Clicking an empty cell places one hero placeholder cube.
- Clicking the same occupied cell does not create another hero.
- Clicking over Unity UI does not place a hero.
- Clicking outside the grid causes no error.
- Console logs the placement event.
- No red Console errors appear.
- `GameEvents.RaiseHeroPlaced(HeroType, Vector2Int)` is raised only after successful placement.
- `GameEvents.RaiseHeroPlaced(HeroType, Vector2Int)` is not raised by UI-blocked clicks.
- With no economy service assigned, valid placement still succeeds.
- With `Dev1MockPlacementEconomyService` assigned and enough Linh Khi, placement succeeds and deducts cost.
- With `Dev1MockPlacementEconomyService` assigned and insufficient Linh Khi, placement is blocked.
- Economy-blocked placement does not spawn a hero.
- Economy-blocked placement does not mark the cell occupied.
- Economy-blocked placement does not apply the occupied material.
- Economy-blocked placement does not raise `GameEvents.RaiseHeroPlaced(HeroType, Vector2Int)`.
- With a temporary `PlacementToCombatBridge` active, valid placement logs `[Dev1 Combat Bridge]`.
- UI-blocked placement does not log `[Dev1 Combat Bridge]`.
- Occupied-cell placement does not log `[Dev1 Combat Bridge]`.
- Outside-grid clicks do not log `[Dev1 Combat Bridge]`.
- Economy-blocked placement does not log `[Dev1 Combat Bridge]`.
