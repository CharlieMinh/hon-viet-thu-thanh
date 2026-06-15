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
            GameObject hero = CreateCombatHero(data.HeroType, worldPosition);
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

        private GameObject CreateCombatHero(HeroType heroType, Vector3 worldPosition)
        {
            string heroName = $"Dev3_CombatHero_{heroType}_{registeredPlacements.Count}";
            GameObject hero = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hero.name = heroName;
            hero.transform.position = worldPosition;

            switch (heroType)
            {
                case HeroType.ThanhGiong:
                    var thanhGiong = hero.AddComponent<ThanhGiong_Attack>();
                    thanhGiong.attackDamage = 10f;
                    thanhGiong.attackRange = 5f;
                    thanhGiong.attackSpeed = 1f;
                    thanhGiong.projectilePrefab = GetProjectilePrefab();
                    return hero;

                case HeroType.SonTinh:
                    var sonTinh = hero.AddComponent<SonTinh_Attack>();
                    sonTinh.attackDamage = 10f;
                    sonTinh.attackRange = 5f;
                    sonTinh.attackSpeed = 1f;
                    sonTinh.obstaclePrefab = GetObstaclePrefab();
                    return hero;

                case HeroType.ChuDongTu:
                    var chuDongTu = hero.AddComponent<ChuDongTu_Attack>();
                    chuDongTu.attackDamage = 8f;
                    chuDongTu.attackRange = 5f;
                    chuDongTu.attackSpeed = 1f;
                    return hero;

                default:
                    Destroy(hero);
                    return null;
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
