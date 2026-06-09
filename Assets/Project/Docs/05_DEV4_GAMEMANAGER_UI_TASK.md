# 05 - Dev4 GameManager, UI & Rules Task

## 1. Role Summary

Dev4 owns GameManager, game state, Phase 1 economy logs, base HP logs, and minimal UI/rules. In Phase 1, Dev4 proves that game state can react to events from placement and enemies.

## 2. Branch To Use

Use branch:

```bash
feature/dev4
```

## 3. Folder Ownership

Primary folder:

```text
Assets/Project/Dev4_GameManager/
```

Dev4 may work in:

- `Scripts/`
- `UI/`
- `Prefabs/`
- `Scenes/`

## 4. Scene Ownership

Dev4 owns:

```text
Assets/Project/Dev4_GameManager/Scenes/Scene_Dev4_UI.unity
```

## 5. What To Implement In Phase 1

- Manage `baseHP = 100`.
- Manage `gold = 150`.
- Listen to `OnEnemyReachedBase`.
- Subtract base HP when enemy reaches base.
- Listen to `OnEnemyDied`.
- Optionally add gold when enemy dies.
- Listen to `OnHeroPlaced`.
- Log placement / gold / base HP changes.
- Keep UI minimal or log-only.

## 6. What Not To Implement Yet

- Final UI art.
- Final HUD layout.
- Pause menu.
- Win/lose panel polish.
- Full hero selection panel.
- Shop/reroll/synergy.
- Full economy balance.
- Editing enemy, combat, or grid internals.

## 7. Required Scripts / Classes To Create Later

Suggested scripts:

- `GameManager.cs`
- `GameState.cs` if needed
- `SimpleHud.cs` if minimal UI is used
- `Phase1DebugLogger.cs`

Use shared contracts:

- `GameEvents.OnHeroPlaced`
- `GameEvents.OnEnemyDied`
- `GameEvents.OnEnemyReachedBase`
- `HeroType`

## 8. Required Scene GameObjects To Create Later

In Dev4 scene:

- `GameManager`
- Optional `Canvas`
- Optional text for gold
- Optional text for base HP
- Main Camera
- Directional Light

For Phase 1, console logs are acceptable.

## 9. Input / Output Contract With Other Modules

Input:

- `OnHeroPlaced(HeroType heroType, Vector2Int gridPosition)`
- `OnEnemyDied(GameObject enemy, int goldReward)`
- `OnEnemyReachedBase(GameObject enemy)`

Output:

- Updated internal `baseHP`.
- Updated internal `gold`.
- Debug logs for state changes.

Expected behavior:

- GameManager should subscribe on enable and unsubscribe on disable.
- GameManager should not directly search for enemy or hero systems.

## 10. Step-By-Step Implementation Checklist

- [ ] Create `GameManager` script.
- [ ] Add serialized starting base HP set to 100.
- [ ] Add serialized starting gold set to 150.
- [ ] Subscribe to enemy reached base event.
- [ ] Subtract base HP and log result.
- [ ] Subscribe to enemy died event.
- [ ] Optionally add gold and log result.
- [ ] Subscribe to hero placed event.
- [ ] Log hero placement.
- [ ] Add simple UI text only if time allows.
- [ ] Test in `Scene_Dev4_UI.unity`.

## 11. Self-Test Checklist

- [ ] GameManager starts with base HP 100.
- [ ] GameManager starts with gold 150.
- [ ] Enemy reached base event subtracts HP.
- [ ] Enemy died event logs or adds gold.
- [ ] Hero placed event logs correctly.
- [ ] No duplicate event subscription after scene reload.
- [ ] Scene runs without null reference errors.

## 12. Definition Of Done

- Base HP is tracked.
- Gold is tracked.
- Required events are consumed.
- Logs are clear enough for Phase 1 testing.
- Dev4 scene demonstrates state changes without final UI.

## 13. Handoff Notes For Integration

Tell Dev1:

- Whether placement should check gold in Phase 1 or only log placement.

Tell Dev2:

- How much damage enemy reaching base should cause, if not default.

Tell Dev5:

- Required GameManager prefab or scene object setup.
- Whether a Canvas is required for integration.

## 14. Suggested Commit Messages

```bash
git commit -m "feat(dev4): add phase 1 game manager state"
git commit -m "feat(dev4): log enemy and placement events"
git commit -m "feat(dev4): add minimal debug hud"
```

