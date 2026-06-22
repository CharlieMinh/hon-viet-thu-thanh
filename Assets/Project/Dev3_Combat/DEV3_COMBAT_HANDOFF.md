# Dev3 Combat Handoff

## Required assets

- Projectile prefab: `Assets/Project/Dev3_Combat/Prefabs/PF_Dev3_Projectile.prefab`
- Assign it to `projectilePrefab` on `Dev3PlacementCombatRegistrar` and `Dev3CombatPreviewBootstrap`.
- Assign Dev2's `Enemy_Prototype.prefab` to `dev2EnemyPrefab` on the preview bootstrap.

The projectile is a trigger sphere with a kinematic Rigidbody. Phase 1 projectiles are non-piercing: they clean up immediately after a valid hit and after `lifetimeSeconds` when they miss. A target is damaged at most once per projectile even if it has multiple colliders.

## Placement integration

The registrar always prefers the non-null `HeroObject` supplied by Dev1. Name lookup remains as a compatibility path. `allowDebugFallbackCapsule` defaults to disabled and must only be enabled in an isolated debug scene.

Assign the three shared `HeroData` assets to `heroDataCatalog` so placed heroes use team-owned combat stats.

`Scene_Integration.unity` contains `Dev3_CombatIntegration` with both `PlacementToCombatBridge` and `Dev3PlacementCombatRegistrar`. Dev1 must provide `HeroPlacementManager` in that scene, and Dev2/Dev5 must provide the live enemy wave setup; Dev3 does not recreate those modules.

## Preview verification

Run `Scene_Dev3_Combat.unity` and wait three seconds. The preview log reports:

- attack event, damage, and enemy death;
- projectile cleanup after hit and timeout;
- damage against the real Dev2 `Enemy_Prototype` component.

`EnemyStub` is retained only for isolated Dev3 checks. Integration validation uses Dev2's real enemy prefab.
