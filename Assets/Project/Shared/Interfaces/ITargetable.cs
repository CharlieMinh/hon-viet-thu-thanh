using UnityEngine;

namespace HonVietThuThanh.Shared
{
    /// <summary>
    /// Represents an object that combat systems can locate and validate as a
    /// target without depending on enemy implementation classes.
    /// </summary>
    public interface ITargetable
    {
        /// <summary>
        /// Gets the world position used by combat systems for targeting.
        /// </summary>
        /// <returns>The current world position of the target.</returns>
        Vector3 GetPosition();

        /// <summary>
        /// Reports whether this target is still valid for combat.
        /// </summary>
        /// <returns>True when the target is alive and can still be targeted.</returns>
        bool IsAlive();
    }
}
