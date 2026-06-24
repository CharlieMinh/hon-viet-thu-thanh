using System;
using HonVietThuThanh.Shared;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HonVietThuThanh.Dev1
{
    /// <summary>
    /// Builds the placement grid, validates placement clicks, shows a hover preview,
    /// spawns placeholder heroes, and raises the shared hero placed event.
    /// </summary>
    public class HeroPlacementManager : MonoBehaviour
    {
        [System.Serializable]
        private struct HeroPlacementCostEntry
        {
            // Unity serialization assigns these fields from the Inspector.
#pragma warning disable CS0649
            public HeroType heroType;
            [Min(0)] public int cost;
#pragma warning restore CS0649
        }

        [SerializeField, Min(1)] private int rows = 8;
        [SerializeField, Min(1)] private int columns = 5;
        [SerializeField, Min(0.1f)] private float cellSize = 1f;
        [SerializeField] private Vector3 gridOrigin = Vector3.zero;
        [SerializeField] private HeroType selectedHeroType = HeroType.ThanhGiong;
        [SerializeField] private GameObject heroPlaceholderPrefab;
        [SerializeField] private Material cellMaterial;
        [SerializeField] private Material occupiedMaterial;
        [SerializeField] private Material validPreviewMaterial;
        [SerializeField] private Material invalidPreviewMaterial;
        [SerializeField] private Transform gridRoot;
        [SerializeField] private Transform heroRoot;
        [SerializeField] private bool generateGridOnStart = true;
        [SerializeField, Min(0f)] private float previewHeightOffset = 0.6f;
        [SerializeField] private bool showPlacementPreview = true;
        [SerializeField] private bool blockPlacementOverUI = true;
        [SerializeField] private MonoBehaviour economyServiceBehaviour;
        [SerializeField, Min(0)] private int defaultHeroCost = 50;
        [SerializeField] private HeroPlacementCostEntry[] heroPlacementCosts;

        private GridCell[,] cells;
        private GameObject previewInstance;
        private GridCell currentPreviewCell;
        private Material runtimeValidPreviewMaterial;
        private Material runtimeInvalidPreviewMaterial;
        private IPlacementEconomyService economyService;
        private bool hasWarnedInvalidEconomyService;

        /// <summary>
        /// Gets the generated placement cells indexed by column, then row.
        /// </summary>
        public GridCell[,] Cells => cells;

        public static event Action<HeroType, Vector2Int, GameObject> OnHeroPlacedWithObject;

        private void OnEnable()
        {
            GameEvents.OnHeroSelected += HandleHeroSelected;
        }

        private void OnDisable()
        {
            GameEvents.OnHeroSelected -= HandleHeroSelected;
        }

        private void HandleHeroSelected(HeroType heroType)
        {
            selectedHeroType = heroType;
            UpdatePreviewState(currentPreviewCell);

            Debug.Log($"[HeroPlacementManager] Selected hero changed to {selectedHeroType}");
        }

        private void Start()
        {
            if (generateGridOnStart)
            {
                GenerateGrid();
            }
        }

        /// <summary>
        /// Generates the official 5-column x 8-row Dev1 placement grid.
        /// </summary>
        public void GenerateGrid()
        {
            EnsureRoots();
            HidePlacementPreview();
            ClearGeneratedGridCells();

            cells = new GridCell[columns, rows];

            for (int column = 0; column < columns; column++)
            {
                for (int row = 0; row < rows; row++)
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
            if (ShouldBlockPlacementForUI())
            {
                return false;
            }

            if (cell == null || !cell.CanPlace())
            {
                UpdatePreviewState(cell);
                return false;
            }

            if (!TryPayPlacementCost(selectedHeroType))
            {
                UpdatePreviewState(cell);
                return false;
            }

            GameObject hero = CreateHeroPlaceholder(GetHeroWorldPosition(cell));
            Vector2Int gridPosition = cell.GridPosition;

            hero.name = $"Hero_{selectedHeroType}_{gridPosition.x}_{gridPosition.y}";
            cell.SetPlacedHero(hero);
            ApplyOccupiedMaterial(cell);
            UpdatePreviewState(cell);

            GameEvents.RaiseHeroPlaced(selectedHeroType, gridPosition);
            OnHeroPlacedWithObject?.Invoke(selectedHeroType, gridPosition, hero);
            return true;
        }

        private bool ShouldBlockPlacementForUI()
        {
            return blockPlacementOverUI && IsPointerOverUI();
        }

        private bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        private IPlacementEconomyService GetEconomyService()
        {
            if (economyServiceBehaviour == null)
            {
                economyService = null;

                if (!hasWarnedInvalidEconomyService)
                {
                    Debug.LogError(
                        $"{nameof(HeroPlacementManager)} missing economy service. Placement is blocked to prevent free heroes.",
                        this);
                    hasWarnedInvalidEconomyService = true;
                }

                return null;
            }

            if (economyService != null)
            {
                return economyService;
            }

            economyService = economyServiceBehaviour as IPlacementEconomyService;

            if (economyService == null && !hasWarnedInvalidEconomyService)
            {
                Debug.LogError(
                    $"{nameof(HeroPlacementManager)} economy service '{economyServiceBehaviour.name}' does not implement {nameof(IPlacementEconomyService)}. Placement is blocked.",
                    this);
                hasWarnedInvalidEconomyService = true;
            }

            return economyService;
        }

        private int GetPlacementCost(HeroType heroType)
        {
            if (heroPlacementCosts != null)
            {
                foreach (HeroPlacementCostEntry entry in heroPlacementCosts)
                {
                    if (entry.heroType == heroType)
                    {
                        return Mathf.Max(0, entry.cost);
                    }
                }
            }

            Debug.LogWarning(
                $"[HeroPlacementManager] Missing placement cost for {heroType}. Using default cost: {defaultHeroCost}",
                this);

            return Mathf.Max(0, defaultHeroCost);
        }

        private bool TryPayPlacementCost(HeroType heroType)
        {
            IPlacementEconomyService service = GetEconomyService();

            if (service == null)
            {
                return false;
            }

            int cost = GetPlacementCost(heroType);
            return service.TrySpendForPlacement(heroType, cost);
        }

        /// <summary>
        /// Shows or moves the placement preview when the mouse enters a grid cell.
        /// </summary>
        /// <param name="cell">The grid cell currently under the mouse.</param>
        public void HandleCellHoverEnter(GridCell cell)
        {
            if (!showPlacementPreview || cell == null)
            {
                return;
            }

            currentPreviewCell = cell;
            ShowPlacementPreview(cell);
        }

        /// <summary>
        /// Hides the placement preview when the mouse leaves the active grid cell.
        /// </summary>
        /// <param name="cell">The grid cell the mouse just left.</param>
        public void HandleCellHoverExit(GridCell cell)
        {
            if (currentPreviewCell != cell)
            {
                return;
            }

            currentPreviewCell = null;
            HidePlacementPreview();
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

        private void ShowPlacementPreview(GridCell cell)
        {
            EnsurePreviewInstance();

            if (previewInstance == null)
            {
                return;
            }

            previewInstance.transform.position = GetHeroWorldPosition(cell);
            previewInstance.SetActive(true);
            UpdatePreviewState(cell);
        }

        private void HidePlacementPreview()
        {
            if (previewInstance != null)
            {
                previewInstance.SetActive(false);
            }
        }

        private void UpdatePreviewState(GridCell cell)
        {
            if (!showPlacementPreview || previewInstance == null || cell == null)
            {
                return;
            }

            Material previewMaterial = cell.CanPlace() ? GetValidPreviewMaterial() : GetInvalidPreviewMaterial();
            ApplyMaterialToRenderers(previewInstance, previewMaterial);
        }

        private void EnsurePreviewInstance()
        {
            if (previewInstance != null)
            {
                return;
            }

            EnsureRoots();

            if (heroPlaceholderPrefab != null)
            {
                previewInstance = Instantiate(heroPlaceholderPrefab, heroRoot);
            }
            else
            {
                previewInstance = GameObject.CreatePrimitive(PrimitiveType.Cube);
                previewInstance.transform.SetParent(heroRoot, false);
                previewInstance.transform.localScale = Vector3.one * 0.75f;
            }

            previewInstance.name = "Dev1_HeroPlacementPreview";
            DisablePreviewColliders(previewInstance);
            previewInstance.SetActive(false);
        }

        private void DisablePreviewColliders(GameObject preview)
        {
            Collider[] colliders = preview.GetComponentsInChildren<Collider>(true);
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }
        }

        private Material GetValidPreviewMaterial()
        {
            if (validPreviewMaterial != null)
            {
                return validPreviewMaterial;
            }

            if (runtimeValidPreviewMaterial == null)
            {
                runtimeValidPreviewMaterial = CreateRuntimePreviewMaterial(new Color(0.2f, 0.9f, 0.35f, 0.55f));
            }

            return runtimeValidPreviewMaterial;
        }

        private Material GetInvalidPreviewMaterial()
        {
            if (invalidPreviewMaterial != null)
            {
                return invalidPreviewMaterial;
            }

            if (runtimeInvalidPreviewMaterial == null)
            {
                runtimeInvalidPreviewMaterial = CreateRuntimePreviewMaterial(new Color(1f, 0.2f, 0.15f, 0.55f));
            }

            return runtimeInvalidPreviewMaterial;
        }

        private Material CreateRuntimePreviewMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            return new Material(shader)
            {
                color = color
            };
        }

        private void ApplyMaterialToRenderers(GameObject target, Material material)
        {
            if (target == null || material == null)
            {
                return;
            }

            Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                renderer.sharedMaterial = material;
            }
        }

        private Vector3 GetHeroWorldPosition(GridCell cell)
        {
            return cell.transform.position + Vector3.up * previewHeightOffset;
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

            for (int column = 0; column < columns; column++)
            {
                for (int row = 0; row < rows; row++)
                {
                    Vector3 center = gridOrigin + new Vector3(column * cellSize, 0f, row * cellSize);
                    Gizmos.DrawWireCube(center, new Vector3(cellSize, 0.05f, cellSize));
                }
            }
        }
    }
}
