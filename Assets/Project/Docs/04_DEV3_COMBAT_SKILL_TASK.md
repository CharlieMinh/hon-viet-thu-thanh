# 04 - Dev3 Combat & Hero Skills Task

## 1. Role Summary

Dev3 owns combat behavior, targeting, projectile logic, and early hero skill placeholders. In Phase 1, Dev3 proves that a hero placeholder can detect an enemy, rotate toward it, shoot a projectile sphere, and damage the enemy through `IDamageable`.

## 2. Branch To Use

Use branch:

```bash
feature/dev3
```

## 3. Folder Ownership

Primary folder:

```text
Assets/Project/Dev3_Combat/
```

Dev3 may work in:

- `Scripts/`
- `Prefabs/`
- `Scenes/`
- `Materials/`

## 4. Scene Ownership

Dev3 owns:

```text
Assets/Project/Dev3_Combat/Scenes/Scene_Dev3_Combat.unity
```

## 5. What To Implement In Phase 1

- Hero detects enemies in attack range.
- Hero chooses a target.
- Hero rotates toward target.
- Hero shoots projectile placeholder sphere.
- Projectile moves toward target.
- Projectile calls `IDamageable.TakeDamage`.
- Keep one simple hero attack enough for Phase 1.

## 6. What Not To Implement Yet

- Final hero skills.
- Full Thánh Gióng pierce behavior.
- Sơn Tinh obstacle system.
- Chử Đồng Tử healing system.
- Final VFX or particles.
- Animation.
- Advanced target priority unless simple nearest-target works.
- UI.

## 7. Required Scripts / Classes To Create Later

Suggested scripts:

- `HeroCombat.cs`
- `TargetDetector.cs`
- `Projectile.cs`
- `SimpleShooter.cs`

Optional later:

- `ThanhGiongSkill.cs`
- `SonTinhSkill.cs`
- `ChuDongTuSkill.cs`

Use shared contracts:

- `IDamageable`
- `ITargetable`
- `HeroType`
- `GameEvents.OnHeroAttacked` if needed

## 8. Required Scene GameObjects To Create Later

In Dev3 scene:

- Hero placeholder cube
- Enemy test target with `IDamageable`
- Projectile placeholder prefab
- Combat test root
- Main Camera
- Directional Light

## 9. Input / Output Contract With Other Modules

Input:

- Hero GameObjects from Dev1 placement.
- Enemy GameObjects from Dev2 wave system.
- Enemy component implementing `IDamageable`.

Output:

- Damage applied through `IDamageable.TakeDamage(float amount)`.
- Optional `OnHeroAttacked(HeroType heroType, GameObject target)` event.

Expected behavior:

- Combat should not need to know the exact Dev2 enemy class if `IDamageable` is present.
- Projectile should fail safely if target is destroyed before impact.

## 10. Step-By-Step Implementation Checklist

- [ ] Create hero combat placeholder object.
- [ ] Create projectile placeholder sphere prefab.
- [ ] Detect enemies in range using collider or distance query.
- [ ] Select a target.
- [ ] Rotate hero toward target.
- [ ] Shoot at fixed attack interval.
- [ ] Move projectile toward target.
- [ ] On hit, call `TakeDamage`.
- [ ] Destroy projectile after hit or timeout.
- [ ] Test against a simple damageable target.
- [ ] Test in `Scene_Dev3_Combat.unity`.

## 11. Self-Test Checklist

- [ ] Hero does not attack when no enemy is in range.
- [ ] Hero attacks when enemy enters range.
- [ ] Hero rotates toward target.
- [ ] Projectile spawns and moves.
- [ ] Projectile damages enemy.
- [ ] Projectile cleans itself up.
- [ ] Scene runs without null reference errors.

## 12. Definition Of Done

- Hero placeholder can attack.
- Projectile placeholder can damage an enemy.
- Combat uses `IDamageable`.
- Dev3 scene demonstrates combat without needing full Dev1/Dev2 systems.

## 13. Handoff Notes For Integration

Tell Dev1:

- What combat component must be on hero prefab.

Tell Dev2:

- What interface enemy must implement.

Tell Dev5:

- Hero combat prefab setup.
- Projectile prefab setup.
- Required tags/layers if used.

## 14. Suggested Commit Messages

```bash
git commit -m "feat(dev3): add basic hero targeting"
git commit -m "feat(dev3): add projectile damage flow"
git commit -m "fix(dev3): handle destroyed projectile targets"
```

