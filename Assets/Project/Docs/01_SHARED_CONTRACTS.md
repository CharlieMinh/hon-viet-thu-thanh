# 01 - Shared Contracts

## Purpose

`Assets/Project/Shared` contains contracts used by multiple modules. These files should be small, stable, and changed carefully because all developers may depend on them.

Shared ownership:

- Folder: `Assets/Project/Shared`
- Review expectation: notify the team before changing shared contracts.
- Phase 1 goal: create only the minimum contracts needed for placement, enemy, combat, and GameManager logs.

## Folder Layout

```text
Assets/Project/Shared/
  Enums/
  Interfaces/
  Events/
  Data/
```

## Required Shared Contracts To Create Later

### Enums

Create later in `Assets/Project/Shared/Enums`:

- `HeroType.cs`
- `EnemyType.cs`

Suggested Phase 1 values:

```csharp
public enum HeroType
{
    ThanhGiong,
    SonTinh,
    ChuDongTu
}

public enum EnemyType
{
    LinhXamLuoc,
    XeThietGiap,
    BossNho
}
```

### Interfaces

Create later in `Assets/Project/Shared/Interfaces`:

- `IDamageable.cs`
- `ITargetable.cs`

Suggested Phase 1 shape:

```csharp
public interface IDamageable
{
    void TakeDamage(float amount);
}

public interface ITargetable
{
    UnityEngine.Vector3 GetPosition();
    bool IsAlive();
}
```

### Events

Create later in `Assets/Project/Shared/Events`:

- `GameEvents.cs`

Suggested Phase 1 events:

```csharp
public static event Action<HeroType, Vector2Int> OnHeroPlaced;
public static event Action<GameObject, int> OnEnemyDied;
public static event Action<GameObject> OnEnemyReachedBase;
```

Optional events if the team needs them:

```csharp
public static event Action<EnemyType, GameObject> OnEnemySpawned;
public static event Action<int> OnWaveStarted;
public static event Action<int> OnWaveCompleted;
public static event Action<HeroType, GameObject> OnHeroAttacked;
```

### Data

Create later in `Assets/Project/Shared/Data` only when Phase 2 starts or when Phase 1 needs simple tuning assets.

Likely future ScriptableObjects:

- `HeroData`
- `EnemyData`
- `WaveData`

For Phase 1, hardcoded placeholder values are acceptable if they keep modules moving.

## Input / Output Contract

Dev1 outputs:

- `OnHeroPlaced(HeroType heroType, Vector2Int gridPosition)`

Dev2 outputs:

- `OnEnemyDied(GameObject enemy, int goldReward)`
- `OnEnemyReachedBase(GameObject enemy)`

Dev3 consumes:

- Enemy objects that implement `IDamageable`
- Enemy objects that can be targeted or discovered in range

Dev4 consumes:

- `OnHeroPlaced`
- `OnEnemyDied`
- `OnEnemyReachedBase`

Dev5 consumes:

- All module outputs in `Scene_Integration.unity`

## Rules For Editing Shared

- Keep contracts simple.
- Do not add module-specific behavior to shared contracts.
- Do not rename shared events without notifying every dev.
- Do not change method signatures after other modules start using them unless the team agrees.
- Prefer adding a new event over making a shared event carry unrelated data.

## Shared Definition of Done

- Contracts compile.
- Contracts are documented enough for all devs to understand.
- No shared script references a specific Dev1/Dev2/Dev3/Dev4/Dev5 implementation class.
- No circular dependency between module folders.

