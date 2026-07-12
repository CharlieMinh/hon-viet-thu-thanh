# Dev2 Fix Report — Wave Start Input and 3-Wave Runtime Flow

**Dự án:** Hồn Việt Thủ Thành (Unity 3D)  
**Branch:** `feature/dev2-fix-wave-start-input`  
**Tác giả:** Antigravity Agent  
**Ngày:** 2026-06-21  
**Trạng thái:** **PARTIAL PASS**

---

## 1. Summary

Investigated leader's preview findings for Dev2 Enemy & Wave System. Most reported
issues were **already fixed in the current codebase** (OnWaveStartRequested event,
WaveManager subscription, Wave 3 configuration). Applied targeted improvements for
compile-time safety and event-driven wave flow correctness.

---

## 2. Leader Findings Verified

| # | Leader Finding | Verification Result |
|---|---|---|
| 1 | EnemyWaveDebugInput throws Input System runtime exception | ⚠️ **Partially valid** — Code was already using New Input System correctly, but lacked compile guards. Fixed. |
| 2 | WaveManager does not consume OnWaveStartRequested | ❌ **Not confirmed** — Already subscribed in OnEnable/OnDisable with handler. |
| 3 | Wave completion / Wave 2 progression not verified at runtime | ✅ **Valid concern** — Code logic is sound but runtime verification needs Play Mode. |
| 4 | Wave 3 is not configured | ❌ **Not confirmed** — Scene YAML already has 3 waves: W1(5 LinhXamLuoc), W2(8 LinhXamLuoc + 1 XeThietGiap), W3(10 LinhXamLuoc + 2 XeThietGiap + 1 TuongDich). |

### New Issues Found During Inspection
1. **autoStartOnPlay was `true`** — WaveManager auto-started Wave 1, conflicting with Dev4 event-driven flow. Fixed to `false`.
2. **Missing compile guards** — EnemyWaveDebugInput had hard dependency on `UnityEngine.InputSystem` without `#if ENABLE_INPUT_SYSTEM`. Fixed.
3. **Wave overlap risk** — `IsWaveRunning` only checked spawn coroutine, not unresolved enemies. New `IsWaveActive` property prevents premature next-wave starts.
4. **Post-final-wave state handling was incomplete** — after Wave 3 finished, starting Wave 4 was blocked correctly but runtime verification found reset/debug controls could appear inactive. Fixed by completing the final-wave state cleanly and limiting the "no more waves" guard to `StartNextWave` only.

---

## 3. Fixes Implemented

### Fix 1: EnemyWaveDebugInput.cs — Compile Guards
- Added `#if ENABLE_INPUT_SYSTEM` / `#elif ENABLE_LEGACY_INPUT_MANAGER` compile guards.
- Import `using UnityEngine.InputSystem` is now conditional.
- Script safely compiles regardless of project input system configuration.
- Added legacy Input Manager fallback path for projects using both backends.
- Script remains debug/testing only; production flow uses GameEvents.

### Fix 2: WaveManager.cs — autoStartOnPlay Default
- Changed `autoStartOnPlay` default from `true` to `false`.
- Scene serialized value also updated from `1` to `0`.
- WaveManager now waits for `GameEvents.OnWaveStartRequested` or manual `StartNextWaveManually()` call.
- Dev4 Start Wave UI button can now reliably control wave start.

### Fix 3: WaveManager.cs — Wave Active Guard
- Added `IsWaveActive` property: returns `true` when spawning OR when enemies are still alive.
- `HandleWaveStartRequested` and `StartNextWaveManually` now use `IsWaveActive` instead of `IsWaveRunning`.
- Prevents starting a new wave while enemies from current wave are still alive.
- Added `TotalWaveCount` and `AllWavesCompleted` properties for external queries.

### Fix 4: WaveManager.cs + EnemyWaveDebugInput.cs — Post-Final-Wave Safety
- `WaveManager.TryCompleteWave()` now marks the current wave as completed and guards `RaiseWaveCompleted(...)` so it fires exactly once per wave.
- After the final wave completes, wave-running state is cleared normally; only `StartNextWave` is blocked when no waves remain.
- Debug/reset controls are not globally blocked after Wave 3.
- `EnemyWaveDebugInput` now logs a targeted "no more waves" message only for `N`, while `R` reset/restart remains available for debug use.
- Official restart/win UI remains Dev4/GameManager responsibility.

---

## 4. Files Changed

| File | Change Type | Description |
|---|---|---|
| `Assets/Project/Dev2_EnemyWave/Scripts/EnemyWaveDebugInput.cs` | **Modified** | Added compile guards for New/Legacy Input System |
| `Assets/Project/Dev2_EnemyWave/Scripts/WaveManager.cs` | **Modified** | `autoStartOnPlay` → false; added `IsWaveActive` guard; added completion/reset safety for post-final-wave state |
| `Assets/Project/Dev2_EnemyWave/Scripts/EnemyWaveDebugInput.cs` | **Modified** | Added compile guards and restricted "no more waves" blocking to `StartNextWave` only |
| `Assets/Project/Dev2_EnemyWave/Scenes/Scene_Dev2_EnemyWave.unity` | **Modified** | `autoStartOnPlay: 1` → `0` in serialized WaveManager |
| `Assets/Project/Dev2_EnemyWave/DEV2_FIX_REPORT.md` | **New** | This report |

### Files NOT Changed (Verified Correct)
- `Assets/Project/Shared/Events/GameEvents.cs` — OnWaveStartRequested already exists (line 67), RaiseWaveStartRequested already exists (line 153).
- `Assets/Project/Dev2_EnemyWave/Scripts/Enemy.cs` — Death/base-reach mutual exclusivity confirmed.
- `Assets/Project/Dev2_EnemyWave/Scripts/EnemySpawner.cs` — Spawning and enemy release logic correct.
- `Assets/Project/Dev2_EnemyWave/Scripts/EnemyPool.cs` — Pool logic correct.
- `Assets/Project/Dev2_EnemyWave/Scripts/EnemyMover.cs` — Movement and base-reach trigger correct.
- `Assets/Project/Dev2_EnemyWave/Scripts/EnemyWaveEventLogger.cs` — Event logging correct.
- `Assets/Project/Shared/Interfaces/IDamageable.cs` — Interface unchanged.
- `Assets/Project/Shared/Interfaces/ITargetable.cs` — Interface unchanged.
- `Assets/Project/Shared/Enums/EnemyType.cs` — Enum values: LinhXamLuoc, PhaoBinh, XeThietGiap, TuongDich.
- `Assets/Project/Shared/Data/EnemyData.cs` — ScriptableObject unchanged.

---

## 5. Current Runtime Test Result

| Test | Result | Notes |
|---|---|---|
| Project compiles without errors | ✅ Expected | Compile guards ensure safe compilation |
| Play Mode starts without Input System exception | ⏳ Needs manual verify | Compile guards should resolve this |
| `GameEvents.RaiseWaveStartRequested()` starts Wave 1 | ⏳ Needs manual verify | WaveManager subscribes in OnEnable |
| Wave 1 spawns 5 LinhXamLuoc | ⏳ Needs manual verify | Scene configured correctly |
| Wave 1 completes and fires `OnWaveCompleted(1)` once | ⏳ Needs manual verify | Completion event is now emitted once after the wave fully resolves |
| Wave 2 starts (8 LinhXamLuoc + 1 XeThietGiap) | ⏳ Needs manual verify | Code flow correct |
| Wave 2 completes and fires `OnWaveCompleted(2)` once | ⏳ Needs manual verify | Code flow correct |
| Wave 3 starts (10 LinhXamLuoc + 2 XeThietGiap + 1 TuongDich) | ⏳ Needs manual verify | Scene configured correctly |
| Wave 3 completes and fires `OnWaveCompleted(3)` once | ⏳ Needs manual verify | Final-wave completion now latches exactly once |
| Extra start request after Wave 3 does not crash | ✅ Expected | `StartWave` bounds-checks waveIndex |
| Reset/restart/debug controls still work after Wave 3 | ⏳ Needs manual verify | Only `StartNextWave` is blocked when no waves remain |
| Enemy death fires `OnEnemyDied(GameObject, int)` correctly | ✅ Confirmed | Code path verified |
| Enemy base reach fires `OnEnemyReachedBase(GameObject)` correctly | ✅ Confirmed | Code path verified |
| Dead enemy does not also reach base | ✅ Confirmed | `IsAlive()` guard in MarkReachedBase |
| Reached-base enemy does not also die | ✅ Confirmed | `IsAlive()` guard in TakeDamage/HandleDeath |
| Enemy cleanup works | ✅ Confirmed | ReleaseSelf → pool release |
| Dev3 can use IDamageable/ITargetable | ✅ Confirmed | Interfaces unchanged |
| Dev4 can control Dev2 through GameEvents only | ✅ Confirmed | No direct coupling |

---

## 6. Current Dev2 Status

**PARTIAL PASS** — All code changes are applied and verified through static analysis.
Runtime Play Mode verification requires manual testing in Unity Editor.

### Wave Configuration (Scene-Serialized)
- **Wave 1:** 5× LinhXamLuoc (interval: 1.25s)
- **Wave 2:** 8× LinhXamLuoc (interval: 1.0s) + 1× XeThietGiap (interval: 2.0s)
- **Wave 3:** 10× LinhXamLuoc (interval: 0.8s) + 2× XeThietGiap (interval: 1.5s) + 1× TuongDich/Boss (interval: 3.0s)

### EnemyData Assets
- `EnemyData_LinhXamLuoc`: HP=20, Speed=2.5, Gold=10, BaseDmg=10
- `EnemyData_XeThietGiap`: HP=60, Speed=1.5, Gold=30, BaseDmg=20
- `EnemyData_Boss (TuongDich)`: HP=250, Speed=1.0, Gold=100, BaseDmg=50

---

## 7. Remaining Risks / Limitations

1. **Runtime verification pending** — Play Mode tests need to be done manually in Unity Editor.
2. **Scene serialized values take precedence** — If Unity Editor reserializes the scene after opening, ensure `autoStartOnPlay` remains unchecked in Inspector.
3. **Single enemy prefab** — All enemy types use `Enemy_Prototype.prefab` (cube placeholder). Visual distinction between types is not implemented yet.
4. **autoAdvanceToNextWave is `false` in scene** — Waves only advance via `OnWaveStartRequested` or debug N key. This is the correct behavior for Dev4 integration.
5. **Official restart flow is still out of scope for Dev2** — The current fix keeps reset/restart available as debug-only after Wave 3. Production restart/win UX still belongs to Dev4/GameManager.
6. **(Minor) Debug keys unresponsive when Game View loses focus** — With the New Input System, `Keyboard.current.wasPressedThisFrame` only registers input when the Game View has OS-level keyboard focus. Clicking Inspector, Console, or Scene View during Play Mode causes all debug keys (Space/K/R/N) to stop responding. Fix: click back on Game View or Alt+Tab. This is standard Unity Editor behavior, not a bug. It does not affect production flow since waves are triggered via `GameEvents.OnWaveStartRequested`, not keyboard input.

---

## 8. Next Steps for Project Progress

### Immediate (Dev4)
- Dev4 should connect Start Wave UI button to `GameEvents.RaiseWaveStartRequested()`.
- Dev4 should consume `OnWaveCompleted(3)` for win condition.
- Dev4 should consume `OnWaveStarted(int)` to update wave counter UI.

### Integration (Dev5)
- Dev5 should align LanePath start/end transforms with `Scene_Integration` map layout.
- Dev5 can copy EnemySpawner, WaveManager, LanePath, EnemyPool objects from Dev2 prototype scene.

### Future (Dev2)
- Replace placeholder cube enemy prefab with real 3D models per enemy type.
- Add distinct visual appearance per EnemyType (color, scale, model).
- Align EnemyData stats with gameplay balance testing.
- No contract changes needed — all public events and interfaces remain stable.

### Manual Verification Steps
1. Open Unity Editor with `Scene_Dev2_EnemyWave`.
2. Enter Play Mode — verify no Input System exception in Console.
3. Press `N` or call `GameEvents.RaiseWaveStartRequested()` — verify Wave 1 starts.
4. Wait for Wave 1 enemies to die/reach base — verify `[Dev2] Wave completed -> 1` in Console.
5. Press `N` again — verify Wave 2 starts.
6. Repeat for Wave 3.
7. Press `N` after Wave 3 — verify "no more waves" log, no crash.
8. Press `R` after Wave 3 — verify prototype resets and can be started again for debug.

## 9. Additional Runtime Verification Note

New issue found during runtime verification:  
After completing Wave 3, WaveManager correctly prevents starting a non-existent next wave, but the debug/reset controls become inactive. This indicates an incomplete post-final-wave state handling or an input guard that blocks non-start controls after all waves are completed.

Expected fix:  
Only StartNextWave should be blocked when no waves remain. Reset/restart/debug controls should still work after Wave 3 completion.
