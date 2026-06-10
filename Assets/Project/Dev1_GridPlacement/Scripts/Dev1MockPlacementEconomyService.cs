using HonVietThuThanh.Shared;
using UnityEngine;

namespace HonVietThuThanh.Dev1
{
    /// <summary>
    /// Dev1-only test economy service for manually validating placement payment
    /// before the production economy implementation exists.
    /// </summary>
    public class Dev1MockPlacementEconomyService : MonoBehaviour, IPlacementEconomyService
    {
        [SerializeField, Min(0)] private int startingLinhKhi = 150;
        [SerializeField, Min(0)] private int currentLinhKhi = 150;
        [SerializeField] private bool resetCurrentOnStart = true;
        [SerializeField] private bool logTransactions = true;

        public int CurrentLinhKhi => currentLinhKhi;

        private void Start()
        {
            if (resetCurrentOnStart)
            {
                currentLinhKhi = startingLinhKhi;
            }
        }

        public bool TrySpendForPlacement(HeroType heroType, int cost)
        {
            int sanitizedCost = Mathf.Max(0, cost);
            if (currentLinhKhi < sanitizedCost)
            {
                if (logTransactions)
                {
                    Debug.Log(
                        $"[Dev1 Mock Economy] Blocked {heroType} placement. Cost: {sanitizedCost}, Linh Khi: {currentLinhKhi}.",
                        this);
                }

                return false;
            }

            currentLinhKhi -= sanitizedCost;
            if (logTransactions)
            {
                Debug.Log(
                    $"[Dev1 Mock Economy] Paid {sanitizedCost} Linh Khi for {heroType}. Remaining: {currentLinhKhi}.",
                    this);
            }

            return true;
        }
    }
}
