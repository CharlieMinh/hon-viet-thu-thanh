using System;
using HonVietThuThanh.Shared;
using UnityEngine;

namespace HonVietThuThanh.Dev1
{
    /// <summary>
    /// Dev1-owned handoff adapter that republishes successful placement events
    /// for future combat systems without depending on combat implementation.
    /// </summary>
    public class PlacementToCombatBridge : MonoBehaviour
    {
        public readonly struct HeroPlacementCombatData
        {
            public HeroPlacementCombatData(HeroType heroType, Vector2Int gridPosition)
                : this(heroType, gridPosition, null)
            {
            }

            public HeroPlacementCombatData(HeroType heroType, Vector2Int gridPosition, GameObject heroObject)
            {
                HeroType = heroType;
                GridPosition = gridPosition;
                Column = gridPosition.x;
                Row = gridPosition.y;
                HeroObject = heroObject;
            }

            public HeroType HeroType { get; }
            public Vector2Int GridPosition { get; }
            public int Column { get; }
            public int Row { get; }
            public GameObject HeroObject { get; }
        }

        public static event Action<HeroPlacementCombatData> OnHeroPlacementReadyForCombat;

        private void OnEnable()
        {
            HeroPlacementManager.OnHeroPlacedWithObject += HandleHeroPlacedWithObject;
        }

        private void OnDisable()
        {
            HeroPlacementManager.OnHeroPlacedWithObject -= HandleHeroPlacedWithObject;
        }

        private void HandleHeroPlacedWithObject(HeroType heroType, Vector2Int gridPosition, GameObject heroObject)
        {
            HeroPlacementCombatData data = new HeroPlacementCombatData(heroType, gridPosition, heroObject);
            OnHeroPlacementReadyForCombat?.Invoke(data);

            string heroObjectStatus = data.HeroObject != null ? "present" : "null";
            Debug.Log(
                $"[Dev1 Combat Bridge] Hero placement ready for combat: {data.HeroType} at column {data.Column}, row {data.Row}. HeroObject: {heroObjectStatus}.",
                this);
        }
    }
}
