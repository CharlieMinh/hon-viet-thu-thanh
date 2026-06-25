# Dev2 Phase 1 Handoff

## Dev3

- Enemy placeholder prefab is at `Assets/Project/Dev2_EnemyWave/Prefabs/Enemy_Prototype.prefab`.
- `HonVietThuThanh.Dev2_EnemyWave.Enemy` implements both `IDamageable` and `ITargetable`.
- Projectiles only need to call `TakeDamage(float amount)` on the `Enemy` component, or any component reached through `IDamageable`.
- `ITargetable.GetPosition()` returns the enemy's current `transform.position`.
- `ITargetable.IsAlive()` returns `false` immediately after the enemy dies or reaches base, so targeting systems can drop stale targets.
- The placeholder cube keeps its collider on the root object, so direct raycast or trigger hits on the root are sufficient.

## Dev4

- `GameEvents.RaiseEnemySpawned(EnemyType, GameObject)` fires every time Dev2 spawns one enemy instance.
- `GameEvents.RaiseEnemyDied(GameObject enemy, int goldReward)` fires exactly once for each enemy that dies from HP reaching zero.
- `GameEvents.RaiseEnemyReachedBase(GameObject enemy)` fires exactly once for each enemy that reaches the lane end.
- An enemy that dies will not also count as reached base, and an enemy that reaches base will not also fire the death event.
- `GameEvents.RaiseWaveStarted(int waveIndex)` and `GameEvents.RaiseWaveCompleted(int waveIndex)` both use zero-based `waveIndex`.

## Dev5

- Prototype scene: `Assets/Project/Dev2_EnemyWave/Scenes/Scene_Dev2_EnemyWave.unity`.
- Enemy prefab: `Assets/Project/Dev2_EnemyWave/Prefabs/Enemy_Prototype.prefab`.
- Required objects to bring into the integration scene:
- `EnemySpawner`
- `WaveManager`
- `LanePath`
- `EnemyRoot`
- `EnemyPool`
- The prototype currently uses a straight lane with `LaneStart` and `LaneEnd`; when aligning to the board, only those transforms need to be repositioned.
- Debug controls in the prototype scene:
- `Space`: spawn one test enemy
- `K`: damage the first alive enemy
- `R`: reset the prototype and start again
- `N`: start the next wave manually
- If moved into `Scene_Integration`, `EnemySpawner`, `WaveManager`, `LanePath`, and `EnemyPool` can remain; `EnemyWaveDebugInput` should stay temporary for integration testing only.
