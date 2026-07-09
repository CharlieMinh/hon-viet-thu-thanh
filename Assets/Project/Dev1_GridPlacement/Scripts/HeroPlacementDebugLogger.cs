using HonVietThuThanh.Shared;
using UnityEngine;

namespace HonVietThuThanh.Dev1
{
    /// <summary>
    /// Logs Dev1 placement events for Phase 1 testing.
    /// This script does not own gameplay logic.
    /// </summary>
    public class HeroPlacementDebugLogger : MonoBehaviour
    {
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
            Debug.Log($"[Dev1 Placement] Hero placed: {heroType} at column {gridPosition.x}, row {gridPosition.y}");
        }
    }
}
