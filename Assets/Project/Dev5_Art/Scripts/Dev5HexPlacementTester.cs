using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Project.Dev5Art
{
    public sealed class Dev5HexPlacementTester : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Transform spawnedPiecesRoot;
        [SerializeField] private string hexTileNamePrefix = "HexTile_R";
        [SerializeField] private float pieceYOffset = 0.75f;
        [SerializeField] private Vector3 heroScale = new Vector3(0.9f, 1.0f, 0.9f);
        [SerializeField] private Vector3 enemyScale = new Vector3(0.9f, 0.9f, 0.9f);
        [SerializeField] private Color heroColor = new Color(0.2f, 0.45f, 1.0f, 1.0f);
        [SerializeField] private Color enemyColor = new Color(1.0f, 0.2f, 0.15f, 1.0f);

        private readonly Dictionary<Transform, GameObject> occupiedTiles = new Dictionary<Transform, GameObject>();

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (spawnedPiecesRoot == null)
            {
                GameObject root = GameObject.Find("PlacementTestPieces");
                if (root == null)
                {
                    root = new GameObject("PlacementTestPieces");
                }

                spawnedPiecesRoot = root.transform;
            }
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            Vector2 mousePosition = mouse.position.ReadValue();
            if (mouse.leftButton.wasPressedThisFrame)
            {
                TryPlacePiece(PrimitiveType.Cube, "Hero", heroScale, heroColor, mousePosition);
            }
            else if (mouse.rightButton.wasPressedThisFrame)
            {
                TryPlacePiece(PrimitiveType.Sphere, "Enemy", enemyScale, enemyColor, mousePosition);
            }
#else
            Vector2 mousePosition = Input.mousePosition;
            if (Input.GetMouseButtonDown(0))
            {
                TryPlacePiece(PrimitiveType.Cube, "Hero", heroScale, heroColor, mousePosition);
            }
            else if (Input.GetMouseButtonDown(1))
            {
                TryPlacePiece(PrimitiveType.Sphere, "Enemy", enemyScale, enemyColor, mousePosition);
            }
#endif
        }

        private void TryPlacePiece(PrimitiveType primitiveType, string label, Vector3 scale, Color color, Vector2 mousePosition)
        {
            Transform tile = GetHexTileUnderMouse(mousePosition);
            if (tile == null)
            {
                Debug.Log("[Dev5HexPlacementTester] Click ignored: pointer is not over a hex tile.");
                return;
            }

            if (occupiedTiles.ContainsKey(tile))
            {
                Debug.Log("[Dev5HexPlacementTester] Tile already occupied: " + tile.name);
                return;
            }

            GameObject piece = GameObject.CreatePrimitive(primitiveType);
            piece.name = label + "_On_" + tile.name;
            piece.transform.SetParent(spawnedPiecesRoot, true);
            piece.transform.position = tile.position + Vector3.up * pieceYOffset;
            piece.transform.localScale = scale;

            Renderer renderer = piece.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }

            occupiedTiles.Add(tile, piece);
            Debug.Log("[Dev5HexPlacementTester] Placed " + label + " on " + tile.name);
        }

        private Transform GetHexTileUnderMouse(Vector2 mousePosition)
        {
            if (targetCamera == null)
            {
                return null;
            }

            Ray ray = targetCamera.ScreenPointToRay(mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                return null;
            }

            Transform current = hit.collider.transform;
            while (current != null)
            {
                if (current.name.StartsWith(hexTileNamePrefix))
                {
                    return current;
                }

                current = current.parent;
            }

            return null;
        }
    }
}
