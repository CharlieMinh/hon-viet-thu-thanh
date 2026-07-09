using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Visual-only selection ring for a placeable unit. It owns no gameplay state.
    /// </summary>
    [DisallowMultipleComponent]
    public class SelectableUnitVisual : MonoBehaviour
    {
        private const string RingObjectName = "Selection_Ring";

        [Header("Ring")]
        [SerializeField] private Color ringColor = new Color(1f, 0.62f, 0.02f, 1f);
        [SerializeField, Min(0.5f)] private float minimumDiameter = 1.35f;
        [SerializeField, Min(0f)] private float diameterPadding = 0.35f;
        [SerializeField, Min(0.005f)] private float discThickness = 0.025f;
        [SerializeField] private float verticalOffset = 0.08f;

        [Header("Pulse")]
        [SerializeField] private bool pulse = true;
        [SerializeField, Min(0f)] private float pulseAmount = 0.08f;
        [SerializeField, Min(0.1f)] private float pulseSpeed = 3f;

        private GameObject ringObject;
        private MeshRenderer ringRenderer;
        private Material ringMaterial;
        private Vector3 baseRingScale = Vector3.one;
        private bool selected;

        public void ShowSelected(bool isSelected)
        {
            selected = isSelected;

            if (selected)
            {
                EnsureRing();
                PositionRingAtFeet();
            }

            if (ringObject != null)
            {
                ringObject.SetActive(selected);
                ringObject.transform.localScale = baseRingScale;
            }
        }

        private void Update()
        {
            if (!selected || ringObject == null)
            {
                return;
            }

            PositionRingAtFeet();
            if (!pulse)
            {
                ringObject.transform.localScale = baseRingScale;
                return;
            }

            float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            ringObject.transform.localScale = new Vector3(
                baseRingScale.x * scale,
                baseRingScale.y,
                baseRingScale.z * scale);
        }

        private void OnDisable()
        {
            ShowSelected(false);
        }

        private void OnDestroy()
        {
            if (ringMaterial != null)
            {
                Destroy(ringMaterial);
                ringMaterial = null;
            }
        }

        private void EnsureRing()
        {
            if (ringObject != null)
            {
                return;
            }

            ringObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ringObject.name = RingObjectName;
            ringObject.transform.SetParent(transform, true);
            ringObject.layer = gameObject.layer;

            Collider ringCollider = ringObject.GetComponent<Collider>();
            if (ringCollider != null)
            {
                DestroyImmediate(ringCollider);
            }

            ringRenderer = ringObject.GetComponent<MeshRenderer>();
            ringMaterial = CreateRingMaterial();
            ringRenderer.sharedMaterial = ringMaterial;
            ringRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ringRenderer.receiveShadows = false;

            ringObject.SetActive(false);
        }

        private Material CreateRingMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader)
            {
                name = "Runtime_Unit_Selection_Ring",
                color = ringColor
            };

            material.SetColor("_Color", ringColor);
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", ringColor);
            }

            return material;
        }

        private void PositionRingAtFeet()
        {
            if (ringObject == null)
            {
                return;
            }

            Physics.SyncTransforms();
            Bounds bounds = CalculateUnitBounds();
            float diameter = Mathf.Max(minimumDiameter, Mathf.Max(bounds.size.x, bounds.size.z) + diameterPadding);
            Vector3 worldPosition = transform.position;
            worldPosition.y = bounds.min.y + verticalOffset;
            ringObject.transform.position = worldPosition;
            ringObject.transform.rotation = Quaternion.identity;
            baseRingScale = new Vector3(diameter, discThickness, diameter);
        }

        private Bounds CalculateUnitBounds()
        {
            if (TryCalculateColliderBounds(out Bounds colliderBounds))
            {
                return colliderBounds;
            }

            return CalculateRendererBounds();
        }

        private bool TryCalculateColliderBounds(out Bounds bounds)
        {
            Collider[] colliders = GetComponentsInChildren<Collider>();
            bool hasBounds = false;
            bounds = new Bounds(transform.position, Vector3.zero);

            foreach (Collider candidate in colliders)
            {
                if (candidate == null || candidate.transform == ringObject?.transform)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = candidate.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(candidate.bounds);
                }
            }

            return hasBounds;
        }

        private Bounds CalculateRendererBounds()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            bool hasBounds = false;
            Bounds bounds = new Bounds(transform.position, Vector3.zero);

            foreach (Renderer candidate in renderers)
            {
                if (candidate == null || candidate == ringRenderer)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = candidate.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(candidate.bounds);
                }
            }

            return hasBounds ? bounds : new Bounds(transform.position, Vector3.one);
        }
    }
}
