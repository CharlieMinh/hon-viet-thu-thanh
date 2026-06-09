using UnityEngine;

namespace HonVietThuThanh.Dev1
{
    /// <summary>
    /// Represents one clickable placement cell in the Dev1 grid.
    /// The cell stores its grid coordinate and delegates placement requests
    /// to <see cref="HeroPlacementManager"/>.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class GridCell : MonoBehaviour
    {
        private HeroPlacementManager manager;

        /// <summary>
        /// Gets the logical grid coordinate for this cell.
        /// X is the column, Y is the row.
        /// </summary>
        public Vector2Int GridPosition { get; private set; }

        /// <summary>
        /// Gets whether this cell already has a placed hero.
        /// </summary>
        public bool IsOccupied { get; private set; }

        /// <summary>
        /// Gets the hero GameObject placed on this cell, if any.
        /// </summary>
        public GameObject PlacedHero { get; private set; }

        /// <summary>
        /// Initializes the cell with its logical grid position and placement manager.
        /// </summary>
        /// <param name="gridPosition">The coordinate for this cell.</param>
        /// <param name="manager">The manager that handles placement requests.</param>
        public void Initialize(Vector2Int gridPosition, HeroPlacementManager manager)
        {
            GridPosition = gridPosition;
            this.manager = manager;
        }

        /// <summary>
        /// Returns true when a hero can be placed on this cell.
        /// </summary>
        /// <returns>True if the cell is not occupied.</returns>
        public bool CanPlace()
        {
            return !IsOccupied;
        }

        /// <summary>
        /// Marks this cell as occupied by the provided hero.
        /// Passing null clears the occupied state.
        /// </summary>
        /// <param name="hero">The hero placed on this cell.</param>
        public void SetPlacedHero(GameObject hero)
        {
            PlacedHero = hero;
            IsOccupied = hero != null;
        }

        private void OnMouseDown()
        {
            if (manager != null)
            {
                manager.TryPlaceHero(this);
            }
        }
    }
}
