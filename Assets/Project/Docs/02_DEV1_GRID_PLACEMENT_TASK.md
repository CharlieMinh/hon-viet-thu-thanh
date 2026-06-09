# 02 - Dev1 Grid & Hero Placement Task

## 1. Role Summary

Dev1 owns the grid and hero placement system. In Phase 1, Dev1 proves that the player can click a 3D grid cell and place a hero placeholder cube.

## 2. Branch To Use

Use branch:

```bash
feature/dev1
```

## 3. Folder Ownership

Primary folder:

```text
Assets/Project/Dev1_GridPlacement/
```

Dev1 may work in:

- `Scripts/`
- `Prefabs/`
- `Scenes/`
- `Materials/`

Do not edit other module folders unless the team agrees.

## 4. Scene Ownership

Dev1 owns:

```text
Assets/Project/Dev1_GridPlacement/Scenes/Scene_Dev1_Placement.unity
```

## 5. What To Implement In Phase 1

- Create a visible 3D grid with size 5 columns x 8 rows.
- Allow click on a valid cell.
- Place a hero placeholder cube at the clicked cell.
- Prevent placement outside the grid.
- Prevent placing more than one hero in the same cell.
- Fire `OnHeroPlaced(HeroType, Vector2Int)` after successful placement.
- Use placeholder materials/colors so placement is easy to see.

## 6. What Not To Implement Yet

- Final hero models.
- Final hero selection UI.
- Hero cost / gold checks, unless Dev4 asks for a simple hook.
- Complex grid editing tools.
- Enemy, wave, combat, projectile, win/lose logic.
- Advanced pathfinding or obstacle logic.

## 7. Required Scripts / Classes To Create Later

Suggested scripts:

- `GridManager.cs`
- `GridCell.cs`
- `HeroPlacementManager.cs`
- `HeroPrefabEntry.cs`

Use shared contracts:

- `HeroType`
- `GameEvents.OnHeroPlaced`

## 8. Required Scene GameObjects To Create Later

In Dev1 scene:

- `GridRoot`
- `PlacementManager`
- Cell objects or generated cell visuals
- Hero placeholder prefab
- Main Camera
- Directional Light

In integration scene, Dev5 may later create or place these with Dev1 guidance.

## 9. Input / Output Contract With Other Modules

Input:

- Selected `HeroType` from UI or temporary Dev1 test selector.

Output:

- `OnHeroPlaced(HeroType heroType, Vector2Int gridPosition)`

Placement data expected by other modules:

- The grid position must be stable.
- The placed hero GameObject should be positioned consistently with the grid.
- Occupied cells must be tracked by Dev1.

## 10. Step-By-Step Implementation Checklist

- [ ] Create grid coordinate convention: x = column, y = row.
- [ ] Decide world origin for grid.
- [ ] Generate or place 5x8 visible cells.
- [ ] Add collider to each clickable cell.
- [ ] Implement click detection.
- [ ] Convert clicked cell to `Vector2Int`.
- [ ] Track occupied cells.
- [ ] Instantiate hero placeholder cube on valid cell.
- [ ] Reject click if cell is occupied.
- [ ] Fire `OnHeroPlaced` only after successful placement.
- [ ] Add debug logs for placement success/failure.
- [ ] Test in `Scene_Dev1_Placement.unity`.

## 11. Self-Test Checklist

- [ ] Can place a hero on an empty cell.
- [ ] Cannot place two heroes on the same cell.
- [ ] Clicking outside grid does nothing.
- [ ] All 5x8 cells can be clicked.
- [ ] `OnHeroPlaced` fires once per valid placement.
- [ ] Scene runs without null reference errors.

## 12. Definition Of Done

- 5x8 grid is visible.
- Hero placeholder cube can be placed.
- Occupied-cell validation works.
- Event output works.
- Dev1 scene demonstrates the system without needing other modules.

## 13. Handoff Notes For Integration

Tell Dev5:

- Grid world origin.
- Cell size.
- Hero placeholder prefab path.
- Required manager GameObjects.
- How `OnHeroPlaced` is fired.

Tell Dev4:

- How UI should pass selected `HeroType` to placement.

## 14. Suggested Commit Messages

```bash
git commit -m "feat(dev1): add 3d placement grid"
git commit -m "feat(dev1): add hero placeholder placement"
git commit -m "fix(dev1): prevent duplicate cell placement"
```

