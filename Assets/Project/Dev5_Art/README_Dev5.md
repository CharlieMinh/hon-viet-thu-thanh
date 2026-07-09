# Dev5 Art & Integration Support Handoff Note

This document serves as a handoff guide for integrating art assets and supporting Dev1 (Grid System) and Dev2 (Enemy Path System).

## Scene Information
* **Visual Preview Scene**: [Scene_Dev5_Art.unity](file:///d:/Unity_game/hon-viet-thu-thanh/Assets/Project/Dev5_Art/Scenes/Scene_Dev5_Art.unity)
  * This is the main Dev5 art scene containing the terrain, camera setup, directional light, VFX, and looping audio.
  * Zero terrain import/load errors or missing scripts.
* **Future Integration Scene**: [Scene_Integration.unity](file:///d:/Unity_game/hon-viet-thu-thanh/Assets/Project/Dev5_Art/Scenes/Scene_Integration.unity)
  * Set up for future integration when Dev2, Dev3, and Dev4 modules are verified and clean.

## Placement & Support Placeholders
In [Scene_Dev5_Art.unity](file:///d:/Unity_game/hon-viet-thu-thanh/Assets/Project/Dev5_Art/Scenes/Scene_Dev5_Art.unity), under the root-level **`Placeholders`** GameObject:

### 1. Dev1 (Grid Placement Area)
* **GameObject**: `GridPlacementArea` (Green visual box)
* **Position**: `[270.0, 118.5, 510.0]` (World coordinates)
* **Scale**: `[10.0, 0.2, 40.0]` (Matches width 10, length 40 adjacent to the enemy path)
* **Usage**: Place the tower grid system here.

### 2. Dev2 (Enemy Path / Spawn / Base)
* **Spawn / Start Point**: 
  * **GameObject**: `SpawnPoint` (Red sphere)
  * **Position**: `[280.0, 120.0, 470.0]`
  * **Usage**: Start point for enemy spawns.
* **Base / End Point**:
  * **GameObject**: `BasePoint` (Blue sphere)
  * **Position**: `[280.0, 120.0, 550.0]`
  * **Usage**: End point / base target for enemies.
* **Enemy Path / Lane**:
  * **GameObject**: `EnemyPath` (Orange visual lane)
  * **Position**: `[280.0, 118.5, 510.0]`
  * **Scale**: `[5.0, 0.2, 80.0]` (A straight path of width 5, length 80 connecting the SpawnPoint and BasePoint)
  * **Usage**: Layout path for enemy movement logic.

## Phase 1 Ground Cleanup Scenes

The existing Dev5 scenes are preserved. Phase 1 cleanup work is isolated in new scenes so no existing Dev5 scene content is overwritten.

### Dev5 Art Preview
* **Scene**: `Assets/Project/Dev5_Art/Scenes/Scene_Dev5_Phase1_GroundCleanup.unity`
* **Purpose**: Clean Phase 1 visual preview with simple readable ground, camera, lighting, and integration support placeholders only.
* **Ground**: `Ground`
  * **Position**: `[0.0, -0.05, 0.0]`
  * **Scale**: `[32.0, 0.1, 44.0]`
  * **World origin**: centered on `[0.0, 0.0, 0.0]`
  * **Material**: `Assets/Project/Dev5_Art/Materials/M_Phase1_Ground.mat`
* **Main Camera**:
  * **Position**: `[0.0, 18.0, -24.0]`
  * **Rotation**: `[55.0, 0.0, 0.0]`
  * **Tag**: `MainCamera`
* **Directional Light**:
  * **Position**: `[0.0, 8.0, -6.0]`
  * **Rotation**: `[50.0, -30.0, 0.0]`
* **GridPlacementArea**:
  * **Position**: `[-7.0, 0.18, 0.0]`
  * **Structure**: parent object containing `HexPlacementTiles`
  * **Hex board**: `7 columns x 5 rows`
  * **Tile naming**: `HexTile_R##_C##`
  * **Tile radius**: `1.18`
  * **Layout**: staggered odd rows, flat-top visual hexes for a Teamfight Tactics-style placement board
  * **Material**: `Assets/Project/Dev5_Art/Materials/M_Phase1_HexPlacementTile.mat`
  * **Note**: this is visual/integration support only; Dev1 placement logic is not added yet.
* **Placement test logic**:
  * **Script**: `Assets/Project/Dev5_Art/Scripts/Dev5HexPlacementTester.cs`
  * **Attached to**: `GridPlacementArea`
  * **Left mouse click**: places a simple hero cube on the clicked empty hex tile.
  * **Right mouse click**: places a simple enemy sphere on the clicked empty hex tile.
  * **Placement rule**: clicks outside hex tiles are ignored, and occupied hex tiles reject additional pieces.
  * **Runtime spawned parent**: `PlacementTestPieces`
  * **Enemy test spawning**: enemies are spawned on clicked hex tiles. `EnemyPath` and `SpawnPoint` are intentionally omitted from this scene.
  * **Purpose**: lightweight Dev5 visual test only; this does not replace Dev1 placement/runtime ownership.
* **BasePoint**:
  * **Position**: `[6.0, 0.8, 18.0]`
  * **Scale**: `[1.4, 1.4, 1.4]`
  * **Material**: `Assets/Project/Dev5_Art/Materials/M_Phase1_BasePoint.mat`

### Dev5 Integration Support Shell
* **Scene**: `Assets/Project/Dev5_Art/Scenes/Scene_Dev5_Phase1_IntegrationSupport.unity`
* **Purpose**: Clean support shell only. Contains `Main Camera`, `Directional Light`, and simple `Ground`.
* **Ground**:
  * **Position**: `[0.0, -0.05, 0.0]`
  * **Scale**: `[32.0, 0.1, 44.0]`
  * **World origin**: centered on `[0.0, 0.0, 0.0]`
* **Main Camera**:
  * **Position**: `[0.0, 18.0, -24.0]`
  * **Rotation**: `[55.0, 0.0, 0.0]`
  * **Tag**: `MainCamera`

### Tag, Layer, and Module Assumptions
* Phase 1 placeholder objects use the default layer and `Untagged`, except `Main Camera`, which uses `MainCamera`.
* Dev1, Dev2, Dev3, and Dev4 runtime prefabs are not added yet because module owners have not approved runtime object integration for this Dev5 support shell.
* The Phase 1 placement board now uses visual hex tiles to match the intended Teamfight Tactics-style direction. Future map assets can replace or decorate the simple `Ground` while keeping the hex board origin and tile naming stable.
* No Terrain object or `Assets/Project/Dev5_Art/Terrain/New Terrain 3.asset` dependency is used by the new Phase 1 scenes.
