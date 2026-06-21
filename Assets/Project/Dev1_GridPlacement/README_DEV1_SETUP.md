# Dev1 Grid Placement Setup

## Branch

Work on:

```bash
git checkout feature/dev1-phase5-combat-handoff
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

## Official Grid Orientation

- Official grid: 5 columns x 8 rows.
- Total cells: 40.
- Coordinate convention: `Vector2Int(column, row)`.
- `Vector2Int.x = column`, valid range `0..4`.
- `Vector2Int.y = row`, valid range `0..7`.
- `GameEvents.OnHeroPlaced` uses the same `Vector2Int(column, row)` payload.

## Phase 4 Economy Hook

Dev1 placement can optionally ask an economy service for permission before placing a hero.

Shared contract:

```csharp
HonVietThuThanh.Shared.IPlacementEconomyService.TrySpendForPlacement(HeroType heroType, int cost)
```

`HeroPlacementManager` exposes:

- `Economy Service Behaviour`: optional `MonoBehaviour` that implements `IPlacementEconomyService`.
- `Default Hero Cost`: fallback placement cost.
- `Hero Placement Costs`: temporary Dev1 cost overrides keyed by `HeroType`.

If no economy service is assigned, placement is allowed so Dev1 remains usable before Dev4 economy exists.
If an assigned service denies payment, Dev1 blocks placement before spawning a hero, marking the cell occupied, changing the occupied material, or raising `GameEvents.RaiseHeroPlaced`.

Dev1 includes `Dev1MockPlacementEconomyService` for manual testing only. Add it to a temporary scene GameObject and assign that component to `HeroPlacementManager.Economy Service Behaviour` to test enough and insufficient Linh Khi cases. Do not save scene changes unless the team explicitly approves a scene setup update.

## Phase 5 Combat Handoff Bridge

Dev1 includes `PlacementToCombatBridge` as a Dev1-owned adapter for future combat integration.

The bridge:

- Subscribes to `GameEvents.OnHeroPlaced` in `OnEnable()`.
- Unsubscribes in `OnDisable()`.
- Reacts only after successful placement because `GameEvents.OnHeroPlaced` is raised only after Dev1 placement succeeds.
- Republishes placement data through `PlacementToCombatBridge.OnHeroPlacementReadyForCombat`.
- Does not implement combat logic.
- Does not reference Dev3 code.

Bridge payload:

```csharp
PlacementToCombatBridge.HeroPlacementCombatData
```

Fields:

- `HeroType HeroType`
- `Vector2Int GridPosition`
- `int Column`
- `int Row`

For manual testing, add `PlacementToCombatBridge` to a temporary scene GameObject and press Play. Do not save scene changes unless the team explicitly approves a scene setup update.

Expected bridge Console log after a valid placement:

```text
[Dev1 Combat Bridge] Hero placement ready for combat: ThanhGiong at column X, row Y.
```

Future Dev3 code can subscribe to:

```csharp
PlacementToCombatBridge.OnHeroPlacementReadyForCombat
```

Dev3 should unsubscribe safely when disabled or destroyed. Dev1 does not create combat components, choose targets, deal damage, or manage attacks.

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
15. For Phase 4, leave `Economy Service Behaviour` empty and confirm placement still works.
16. Add a temporary `Dev1MockPlacementEconomyService`, assign it to the placement manager, and confirm enough Linh Khi allows placement and deducts cost.
17. Lower mock current Linh Khi below the selected hero cost and confirm placement is blocked with no spawned hero and no placement event.
18. For Phase 5, add a temporary `PlacementToCombatBridge` scene object and confirm valid placement logs the Dev1 combat bridge message.
19. Confirm UI-blocked, occupied-cell-blocked, outside-grid, and economy-denied placement attempts do not log the Dev1 combat bridge message.

## Expected Behavior

- Grid is visible.
- Grid is 5 columns x 8 rows.
- Total grid cells: 40.
- Valid coordinates are column `0..4` and row `0..7`.
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
- If no economy service is assigned, valid placement is allowed.
- If an economy service denies payment, placement is blocked before spawn, occupied state, occupied material, and placement event.
- `PlacementToCombatBridge.OnHeroPlacementReadyForCombat` fires only after successful placement.
- The combat handoff bridge does not fire after UI-blocked, invalid, occupied-cell, outside-grid, or economy-denied placement attempts.

## Not In Dev1 Phase 5

Dev1 does not implement:

- enemy spawning
- wave logic
- combat
- projectiles
- GameManager
- final economy ownership
- full UI implementation
- drag preview
- remove or sell hero
- final hero models
