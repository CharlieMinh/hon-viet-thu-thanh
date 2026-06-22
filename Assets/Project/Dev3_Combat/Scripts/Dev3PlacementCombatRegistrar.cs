using System.Collections.Generic;
using HonVietThuThanh.Dev1;
using HonVietThuThanh.Shared;
using UnityEngine;

namespace HonVietThuThanh.Combat
{
    public class Dev3PlacementCombatRegistrar : MonoBehaviour
    {
        [SerializeField] private List<RegisteredPlacement> registeredPlacements = new List<RegisteredPlacement>();
        [SerializeField] private Vector3 combatOrigin = Vector3.zero;
        [SerializeField] private float gridCellSize = 1f;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private GameObject obstaclePrefab;
        [SerializeField] private bool allowDebugFallbackCapsule;
        [SerializeField] private HeroData[] heroDataCatalog;

        private void OnEnable()
        {
            PlacementToCombatBridge.OnHeroPlacementReadyForCombat += RegisterPlacedHero;
        }

        private void OnDisable()
        {
            PlacementToCombatBridge.OnHeroPlacementReadyForCombat -= RegisterPlacedHero;
        }

        private void RegisterPlacedHero(PlacementToCombatBridge.HeroPlacementCombatData data)
        {
            Vector3 worldPosition = GridToWorld(data.GridPosition);
            GameObject hero = ResolveCombatHeroObject(data, worldPosition, out string source, out bool createdFallback);
            if (hero == null)
            {
                Debug.LogWarning($"[Dev3 Combat] Placement received for {data.HeroType}, but no hero object could be resolved.", this);
                return;
            }

            if (!TryConfigureCombatHero(hero, data.HeroType, out bool reusedExistingComponent))
            {
                if (createdFallback)
                {
                    Destroy(hero);
                }

                return;
            }

            if (!IsRegistered(hero))
            {
                registeredPlacements.Add(new RegisteredPlacement(data.HeroType, data.GridPosition, hero));
            }
            Debug.Log(
                $"[Dev3 Combat] Registered placed hero for combat: {data.HeroType} at {data.GridPosition} using {source}. Duplicate component avoided: {reusedExistingComponent}.",
                hero);
        }

        private Vector3 GridToWorld(Vector2Int gridPosition)
        {
            return combatOrigin + new Vector3(gridPosition.x * gridCellSize, 1f, gridPosition.y * gridCellSize);
        }

        private GameObject ResolveCombatHeroObject(
            PlacementToCombatBridge.HeroPlacementCombatData data,
            Vector3 worldPosition,
            out string source,
            out bool createdFallback)
        {
            createdFallback = false;

            if (data.HeroObject != null)
            {
                source = "HeroObject payload";
                return data.HeroObject;
            }

            GameObject namedHero = FindPlacedHeroByName(data.HeroType, data.Column, data.Row);
            if (namedHero != null)
            {
                source = "name lookup fallback";
                return namedHero;
            }

            if (allowDebugFallbackCapsule)
            {
                createdFallback = true;
                source = "debug capsule fallback";
                return CreateCombatHeroShell(data.HeroType, worldPosition);
            }

            source = "none";
            Debug.LogWarning(
                $"[Dev3 Combat] No placed hero object found for {data.HeroType} at {data.GridPosition}. Debug fallback is disabled.",
                this);
            return null;
        }

        private GameObject FindPlacedHeroByName(HeroType heroType, int column, int row)
        {
            string expectedName = $"Hero_{heroType}_{column}_{row}";
            GameObject exactMatch = GameObject.Find(expectedName);
            if (exactMatch != null)
            {
                return exactMatch;
            }

            string prefix = $"Hero_{heroType}";
            string coordinateSuffix = $"_{column}_{row}";
            Transform[] sceneObjects = FindObjectsByType<Transform>();
            foreach (Transform sceneObject in sceneObjects)
            {
                if (sceneObject.name.StartsWith(prefix) && sceneObject.name.EndsWith(coordinateSuffix))
                {
                    return sceneObject.gameObject;
                }
            }

            return null;
        }

        private GameObject CreateCombatHeroShell(HeroType heroType, Vector3 worldPosition)
        {
            string heroName = $"Dev3_CombatHero_{heroType}_{registeredPlacements.Count}";
            GameObject hero = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hero.name = heroName;
            hero.transform.position = worldPosition;
            return hero;
        }

        private bool TryConfigureCombatHero(GameObject hero, HeroType heroType, out bool reusedExistingComponent)
        {
            reusedExistingComponent = false;
            HeroBase existingHeroBase = hero.GetComponent<HeroBase>();
            switch (heroType)
            {
                case HeroType.ThanhGiong:
                    if (existingHeroBase != null && !(existingHeroBase is ThanhGiong_Attack))
                    {
                        LogMismatchedHeroComponent(hero, heroType, existingHeroBase);
                        return false;
                    }

                    var thanhGiong = hero.GetComponent<ThanhGiong_Attack>();
                    if (thanhGiong == null)
                    {
                        thanhGiong = hero.AddComponent<ThanhGiong_Attack>();
                    }
                    else
                    {
                        reusedExistingComponent = true;
                    }

                    ApplyStats(thanhGiong, heroType, 10f, 5f, 1f);
                    thanhGiong.projectilePrefab = GetProjectilePrefab();
                    return true;

                case HeroType.SonTinh:
                    if (existingHeroBase != null && !(existingHeroBase is SonTinh_Attack))
                    {
                        LogMismatchedHeroComponent(hero, heroType, existingHeroBase);
                        return false;
                    }

                    var sonTinh = hero.GetComponent<SonTinh_Attack>();
                    if (sonTinh == null)
                    {
                        sonTinh = hero.AddComponent<SonTinh_Attack>();
                    }
                    else
                    {
                        reusedExistingComponent = true;
                    }

                    ApplyStats(sonTinh, heroType, 10f, 5f, 1f);
                    sonTinh.obstaclePrefab = GetObstaclePrefab();
                    return true;

                case HeroType.ChuDongTu:
                    if (existingHeroBase != null && !(existingHeroBase is ChuDongTu_Attack))
                    {
                        LogMismatchedHeroComponent(hero, heroType, existingHeroBase);
                        return false;
                    }

                    var chuDongTu = hero.GetComponent<ChuDongTu_Attack>();
                    if (chuDongTu == null)
                    {
                        chuDongTu = hero.AddComponent<ChuDongTu_Attack>();
                    }
                    else
                    {
                        reusedExistingComponent = true;
                    }

                    ApplyStats(chuDongTu, heroType, 8f, 5f, 1f);
                    return true;

                default:
                    Debug.LogWarning($"[Dev3 Combat] Placement received for unsupported hero type {heroType}; no Dev3 combat script exists yet.", this);
                    return false;
            }
        }

        private void LogMismatchedHeroComponent(GameObject hero, HeroType heroType, HeroBase existingHeroBase)
        {
            Debug.LogWarning(
                $"[Dev3 Combat] Placement for {heroType} resolved to {hero.name}, but it already has {existingHeroBase.GetType().Name}. Skipping to avoid duplicate or mismatched combat components.",
                hero);
        }

        private GameObject GetProjectilePrefab()
        {
            if (projectilePrefab == null)
            {
                Debug.LogError("[Dev3 Combat] Projectile prefab is not assigned on Dev3PlacementCombatRegistrar.", this);
            }

            return projectilePrefab;
        }

        private bool IsRegistered(GameObject hero)
        {
            foreach (RegisteredPlacement placement in registeredPlacements)
            {
                if (placement.heroObject == hero)
                {
                    return true;
                }
            }

            return false;
        }

        private void ApplyStats(HeroBase hero, HeroType heroType, float defaultDamage, float defaultRange, float defaultSpeed)
        {
            HeroData data = null;
            if (heroDataCatalog != null)
            {
                foreach (HeroData candidate in heroDataCatalog)
                {
                    if (candidate != null && candidate.heroType == heroType)
                    {
                        data = candidate;
                        break;
                    }
                }
            }

            hero.attackDamage = data != null ? data.damage : defaultDamage;
            hero.attackRange = data != null ? data.attackRange : defaultRange;
            hero.attackSpeed = data != null ? data.attackSpeed : defaultSpeed;
        }

        private GameObject GetObstaclePrefab()
        {
            if (obstaclePrefab != null)
            {
                return obstaclePrefab;
            }

            obstaclePrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstaclePrefab.name = "Dev3_PlacementObstaclePrefab";
            obstaclePrefab.transform.position = new Vector3(2f, -50f, 0f);
            obstaclePrefab.SetActive(false);
            obstaclePrefab.AddComponent<Obstacle>();
            return obstaclePrefab;
        }

        [System.Serializable]
        private struct RegisteredPlacement
        {
            public RegisteredPlacement(HeroType heroType, Vector2Int gridPosition, GameObject heroObject)
            {
                this.heroType = heroType;
                this.gridPosition = gridPosition;
                this.heroObject = heroObject;
            }

            public HeroType heroType;
            public Vector2Int gridPosition;
            public GameObject heroObject;
        }
    }
}
