# Dev2 Phase 1 Handoff

## Dev3

- Enemy prefab placeholder nằm tại `Assets/Project/Dev2_EnemyWave/Prefabs/Enemy_Prototype.prefab`.
- Component `HonVietThuThanh.Dev2_EnemyWave.Enemy` implement cả `IDamageable` và `ITargetable`.
- Projectile chỉ cần gọi `TakeDamage(float amount)` lên component `Enemy` hoặc bất kỳ component nào đọc ra được qua `IDamageable`.
- `ITargetable.GetPosition()` trả về `transform.position` hiện tại của enemy.
- `ITargetable.IsAlive()` trả `false` ngay sau khi enemy chết hoặc chạm base để projectile/hệ target không giữ mục tiêu lỗi thời.
- Collider nằm ngay trên root cube của prefab placeholder, nên raycast/trigger hit trực tiếp vào object root là đủ.

## Dev4

- `GameEvents.RaiseEnemySpawned(EnemyType, GameObject)` được bắn mỗi lần Dev2 spawn ra 1 enemy instance.
- `GameEvents.RaiseEnemyDied(GameObject enemy, int goldReward)` được bắn đúng 1 lần cho mỗi enemy chết vì hết HP.
- `GameEvents.RaiseEnemyReachedBase(GameObject enemy)` được bắn đúng 1 lần cho mỗi enemy chạm đích lane.
- Enemy chết sẽ không còn được tính là reached base; enemy đã reached base cũng không còn bắn death event.
- `GameEvents.RaiseWaveStarted(int waveIndex)` và `GameEvents.RaiseWaveCompleted(int waveIndex)` đang dùng `waveIndex` zero-based.

## Dev5

- Scene prototype: `Assets/Project/Dev2_EnemyWave/Scenes/Scene_Dev2_EnemyWave.unity`.
- Prefab enemy: `Assets/Project/Dev2_EnemyWave/Prefabs/Enemy_Prototype.prefab`.
- Object bắt buộc để mang sang integration scene:
  - `EnemySpawner`
  - `WaveManager`
  - `LanePath`
  - `EnemyRoot`
  - `EnemyPool`
- Prototype hiện dùng straight lane với 2 mốc `LaneStart` và `LaneEnd`; khi căn theo board chỉ cần đặt lại 2 transform này.
- Debug controls trong scene prototype:
  - `Space`: spawn 1 enemy test
  - `K`: gây damage lên enemy đầu tiên còn sống
  - `R`: reset prototype và chạy lại
  - `N`: chạy wave kế tiếp thủ công
- Nếu mang vào `Scene_Integration`, có thể giữ `EnemySpawner`, `WaveManager`, `LanePath`, `EnemyPool`; `EnemyWaveDebugInput` chỉ nên giữ tạm trong lúc tích hợp/test.
