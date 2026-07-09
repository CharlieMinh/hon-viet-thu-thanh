namespace HonVietThuThanh.Shared
{
    /// <summary>
    /// Represents any object that can receive damage. Dev3 combat and
    /// projectile systems use this contract to damage Dev2 enemies without
    /// depending on enemy implementation classes.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Applies damage to this object.
        /// </summary>
        /// <param name="amount">The amount of damage to apply.</param>
        void TakeDamage(float amount);
    }
}
