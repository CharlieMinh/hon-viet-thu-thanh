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
            {
                HeroType = heroType;
                GridPosition = gridPosition;
                Column = gridPosition.x;
                Row = gridPosition.y;
            }

            public HeroType HeroType { get; }
            public Vector2Int GridPosition { get; }
            public int Column { get; }
            public int Row { get; }
        }

        public static event Action<HeroPlacementCombatData> OnHeroPlacementReadyForCombat;

        private void OnEnable()
        {
            GameEvents.OnHeroPlaced += HandleHeroPlaced;
        }

        private void OnDisable()
        {
            GameEvents.OnHeroPlaced -= HandleHeroPlaced;
        }

        private void HandleHeroPlaced(HeroType heroType, Vector2Int gridPosition)
        {
            HeroPlacementCombatData data = new HeroPlacementCombatData(heroType, gridPosition);
            OnHeroPlacementReadyForCombat?.Invoke(data);

            Debug.Log(
                $"[Dev1 Combat Bridge] Hero placement ready for combat: {data.HeroType} at column {data.Column}, row {data.Row}.",
                this);
        }
    }
}
