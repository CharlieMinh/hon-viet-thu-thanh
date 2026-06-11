namespace HonVietThuThanh.Shared
{
    /// <summary>
    /// Provides the placement system with an optional economy permission hook.
    /// Implementations should deduct the placement cost only when they return true.
    /// </summary>
    public interface IPlacementEconomyService
    {
        bool TrySpendForPlacement(HeroType heroType, int cost);
    }
}
