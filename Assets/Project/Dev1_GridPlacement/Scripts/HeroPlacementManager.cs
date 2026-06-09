using HonVietThuThanh.Shared;
using UnityEngine;

namespace HonVietThuThanh.Dev1
{
    /// <summary>
    /// Builds the Phase 1 placement grid, validates placement clicks,
    /// spawns placeholder heroes, and raises the shared hero placed event.
    /// </summary>
    public class HeroPlacementManager : MonoBehaviour
    {
        [SerializeField, Min(1)] private int rows = 5;
        [SerializeField, Min(1)] private int columns = 8;
        [SerializeField, Min(0.1f)] private float cellSize = 1f;
        [SerializeField] private Vector3 gridOrigin = Vector3.zero;
        [SerializeField] private HeroType selectedHeroType = HeroType.ThanhGiong;
        [SerializeField] private GameObject heroPlaceholderPrefab;
        [SerializeField] private Material cellMaterial;
        [SerializeField] private Material occupiedMaterial;
        [SerializeField] private Transform gridRoot;
        [SerializeField] private Transform heroRoot;
        [SerializeField] private bool generateGridOnStart = true;

        private GridCell[,] cells;

        /// <summary>
        /// Gets the generated placement cells indexed by column, then row.
        /// </summary>
        public GridCell[,] Cells => cells;

        private void Start()
        {
            if (generateGridOnStart)
            {
                GenerateGrid();
            }
        }

        /// <summary>
        /// Generates a visible 5x8 default placement grid for Phase 1 testing.
        /// </summary>
        public void GenerateGrid()
        {
            EnsureRoots();
            ClearGeneratedGridCells();

            cells = new GridCell[columns, rows];

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    Vector2Int gridPosition = new Vector2Int(column, row);
                    Vector3 worldPosition = GetCellCenter(gridPosition);
                    GameObject cellObject = GameObject.CreatePrimitive(PrimitiveType.Cube);

                    cellObject.name = $"GridCell_{column}_{row}";
                    cellObject.transform.SetParent(gridRoot, false);
                    cellObject.transform.position = worldPosition;
                    cellObject.transform.localScale = new Vector3(cellSize * 0.95f, 0.1f, cellSize * 0.95f);

                    Renderer renderer = cellObject.GetComponent<Renderer>();
                    if (renderer != null && cellMaterial != null)
                    {
                        renderer.sharedMaterial = cellMaterial;
                    }

                    GridCell cell = cellObject.AddComponent<GridCell>();
                    cell.Initialize(gridPosition, this);
                    cells[column, row] = cell;
                }
            }
        }

        /// <summary>
        /// Attempts to place the currently selected hero on the provided cell.
        /// </summary>
        /// <param name="cell">The clicked grid cell.</param>
        /// <returns>True when placement succeeds.</returns>
        public bool TryPlaceHero(GridCell cell)
        {
            if (cell == null || !cell.CanPlace())
            {
                return false;
            }

            GameObject hero = CreateHeroPlaceholder(cell.transform.position + Vector3.up * 0.6f);
            Vector2Int gridPosition = cell.GridPosition;

            hero.name = $"Hero_{selectedHeroType}_{gridPosition.x}_{gridPosition.y}";
            cell.SetPlacedHero(hero);
            ApplyOccupiedMaterial(cell);

            GameEvents.RaiseHeroPlaced(selectedHeroType, gridPosition);
            return true;
        }

        private void EnsureRoots()
        {
            if (gridRoot == null)
            {
                GameObject root = new GameObject("Dev1_GridRoot");
                root.transform.SetParent(transform, false);
                gridRoot = root.transform;
            }

            if (heroRoot == null)
            {
                GameObject root = new GameObject("Dev1_HeroRoot");
                root.transform.SetParent(transform, false);
                heroRoot = root.transform;
            }
        }

        private void ClearGeneratedGridCells()
        {
            if (gridRoot == null)
            {
                return;
            }

            for (int i = gridRoot.childCount - 1; i >= 0; i--)
            {
                Transform child = gridRoot.GetChild(i);
                if (child.name.StartsWith("GridCell_"))
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private GameObject CreateHeroPlaceholder(Vector3 position)
        {
            GameObject hero;
            if (heroPlaceholderPrefab != null)
            {
                hero = Instantiate(heroPlaceholderPrefab, position, Quaternion.identity, heroRoot);
            }
            else
            {
                hero = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hero.transform.SetParent(heroRoot, false);
                hero.transform.position = position;
                hero.transform.localScale = Vector3.one * 0.75f;
            }

            return hero;
        }

        private Vector3 GetCellCenter(Vector2Int gridPosition)
        {
            return gridOrigin + new Vector3(gridPosition.x * cellSize, 0f, gridPosition.y * cellSize);
        }

        private void ApplyOccupiedMaterial(GridCell cell)
        {
            if (occupiedMaterial == null)
            {
                return;
            }

            Renderer renderer = cell.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = occupiedMaterial;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    Vector3 center = gridOrigin + new Vector3(column * cellSize, 0f, row * cellSize);
                    Gizmos.DrawWireCube(center, new Vector3(cellSize, 0.05f, cellSize));
                }
            }
        }
    }
}
