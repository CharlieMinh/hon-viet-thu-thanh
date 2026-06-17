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

        private GameObject projectilePrefab;
        private GameObject obstaclePrefab;

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
            GameObject hero = CreateCombatHero(data.HeroType, data.GridPosition, worldPosition);
            if (hero == null)
            {
                Debug.LogWarning($"[Dev3 Combat] Placement received for unsupported hero type {data.HeroType}; no Dev3 combat script exists yet.", this);
                return;
            }

            registeredPlacements.Add(new RegisteredPlacement(data.HeroType, data.GridPosition, hero));
            Debug.Log($"[Dev3 Combat] Registered placed hero for combat: {data.HeroType} at {data.GridPosition}.", hero);
        }

        private Vector3 GridToWorld(Vector2Int gridPosition)
        {
            return combatOrigin + new Vector3(gridPosition.x * gridCellSize, 1f, gridPosition.y * gridCellSize);
        }

        private GameObject CreateCombatHero(HeroType heroType, Vector2Int gridPosition, Vector3 worldPosition)
        {
            GameObject hero = FindPlacedHero(heroType, gridPosition);
            bool createdFallbackHero = hero == null;
            if (createdFallbackHero)
            {
                hero = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                hero.name = $"Dev3_CombatHero_{heroType}_{registeredPlacements.Count}";
            }

            hero.transform.position = worldPosition;

            switch (heroType)
            {
                case HeroType.ThanhGiong:
                    var thanhGiong = GetOrAddHeroAttack<ThanhGiong_Attack>(hero);
                    if (thanhGiong == null)
                    {
                        DestroyFallbackHeroIfNeeded(hero, createdFallbackHero);
                        return null;
                    }

                    thanhGiong.attackDamage = 10f;
                    thanhGiong.attackRange = 5f;
                    thanhGiong.attackSpeed = 1f;
                    thanhGiong.projectilePrefab = GetProjectilePrefab();
                    return hero;

                case HeroType.SonTinh:
                    var sonTinh = GetOrAddHeroAttack<SonTinh_Attack>(hero);
                    if (sonTinh == null)
                    {
                        DestroyFallbackHeroIfNeeded(hero, createdFallbackHero);
                        return null;
                    }

                    sonTinh.attackDamage = 10f;
                    sonTinh.attackRange = 5f;
                    sonTinh.attackSpeed = 1f;
                    sonTinh.obstaclePrefab = GetObstaclePrefab();
                    return hero;

                case HeroType.ChuDongTu:
                    var chuDongTu = GetOrAddHeroAttack<ChuDongTu_Attack>(hero);
                    if (chuDongTu == null)
                    {
                        DestroyFallbackHeroIfNeeded(hero, createdFallbackHero);
                        return null;
                    }

                    chuDongTu.attackDamage = 8f;
                    chuDongTu.attackRange = 5f;
                    chuDongTu.attackSpeed = 1f;
                    return hero;

                default:
                    DestroyFallbackHeroIfNeeded(hero, createdFallbackHero);
                    return null;
            }
        }

        private static GameObject FindPlacedHero(HeroType heroType, Vector2Int gridPosition)
        {
            string expectedName = $"Hero_{heroType}_{gridPosition.x}_{gridPosition.y}";
            return GameObject.Find(expectedName);
        }

        private static T GetOrAddHeroAttack<T>(GameObject hero) where T : HeroBase
        {
            HeroBase existingHero = hero.GetComponent<HeroBase>();
            if (existingHero != null && !(existingHero is T))
            {
                Debug.LogWarning($"[Dev3 Combat] {hero.name} already has combat script {existingHero.GetType().Name}; skipping duplicate combat registration.", hero);
                return null;
            }

            T attack = hero.GetComponent<T>();
            if (attack == null)
            {
                attack = hero.AddComponent<T>();
            }

            return attack;
        }

        private static void DestroyFallbackHeroIfNeeded(GameObject hero, bool createdFallbackHero)
        {
            if (createdFallbackHero && hero != null)
            {
                Destroy(hero);
            }
        }

        private GameObject GetProjectilePrefab()
        {
            if (projectilePrefab != null)
            {
                return projectilePrefab;
            }

            projectilePrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectilePrefab.name = "Dev3_PlacementProjectilePrefab";
            projectilePrefab.transform.position = new Vector3(0f, -50f, 0f);
            projectilePrefab.transform.localScale = Vector3.one * 0.25f;
            projectilePrefab.SetActive(false);

            Collider collider = projectilePrefab.GetComponent<Collider>();
            collider.isTrigger = true;

            var rb = projectilePrefab.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            projectilePrefab.AddComponent<Projectile>();
            return projectilePrefab;
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
