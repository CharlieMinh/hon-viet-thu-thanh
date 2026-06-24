using HonVietThuThanh.Dev2_EnemyWave;
using UnityEngine;
using UnityEngine.Rendering;

namespace HonVietThuThanh.Dev5_Art
{
    /// <summary>
    /// Draws a lightweight runtime guide from the enemy spawn point to the base.
    /// This is visual-only and does not alter pathfinding or wave behaviour.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LanePath), typeof(LineRenderer))]
    public sealed class PathReadabilityMarker : MonoBehaviour
    {
        [SerializeField, Min(0.02f)] private float width = 0.16f;
        [SerializeField] private float heightOffset = 0.18f;
        [SerializeField] private Color spawnColor = new(0.2f, 0.9f, 0.45f, 0.95f);
        [SerializeField] private Color baseColor = new(1f, 0.35f, 0.12f, 0.95f);

        private Material runtimeMaterial;

        private void Awake()
        {
            LanePath path = GetComponent<LanePath>();
            LineRenderer line = GetComponent<LineRenderer>();

            if (path == null || line == null || !path.HasValidPath)
            {
                if (line != null) line.enabled = false;
                return;
            }

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                line.enabled = false;
                Debug.LogWarning("[PathReadabilityMarker] Sprites/Default shader was not found; path guide disabled.", this);
                return;
            }

            runtimeMaterial = new Material(shader)
            {
                name = "Runtime Path Readability Material"
            };

            line.material = runtimeMaterial;
            line.useWorldSpace = true;
            line.loop = false;
            line.positionCount = path.WaypointCount;
            line.startWidth = width;
            line.endWidth = width;
            line.startColor = spawnColor;
            line.endColor = baseColor;
            line.numCapVertices = 4;
            line.numCornerVertices = 3;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;

            Vector3 lift = Vector3.up * heightOffset;
            for (int index = 0; index < path.WaypointCount; index++)
            {
                line.SetPosition(index, path.GetWaypointPosition(index) + lift);
            }
        }

        private void OnDestroy()
        {
            if (runtimeMaterial != null)
            {
                Destroy(runtimeMaterial);
            }
        }
    }
}
