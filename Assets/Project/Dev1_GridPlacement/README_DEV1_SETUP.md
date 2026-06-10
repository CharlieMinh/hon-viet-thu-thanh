# Dev1 Grid Placement Setup

## Branch

Work on:

```bash
git checkout feature/dev1-phase4-economy-permission-hook
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
- If no economy service is assigned, valid placement is allowed.
- If an economy service denies payment, placement is blocked before spawn, occupied state, occupied material, and placement event.

## Not In Dev1 Phase 4

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
