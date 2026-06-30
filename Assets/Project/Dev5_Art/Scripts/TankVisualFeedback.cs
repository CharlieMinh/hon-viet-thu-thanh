using System.Collections;
using UnityEngine;

namespace HonVietThuThanh.Dev5
{
    /// <summary>
    /// Visual-only feedback for the static Tank model.
    /// This script never changes gameplay root movement, colliders, health, damage, targeting, or economy.
    /// </summary>
    public class TankVisualFeedback : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Renderer bodyRenderer;

        [Header("Idle")]
        [SerializeField] private float idleAmplitude = 0.035f;
        [SerializeField] private float idleFrequency = 1.4f;
        [SerializeField] private float idleScalePulse = 0.015f;

        [Header("Movement")]
        [SerializeField] private float moveLeanAngle = 4f;
        [SerializeField] private float moveLeanSpeed = 10f;

        [Header("Attack")]
        [SerializeField] private float attackLungeDistance = 0.18f;
        [SerializeField] private float attackTiltAngle = 8f;
        [SerializeField] private float attackOutDuration = 0.12f;
        [SerializeField] private float attackReturnDuration = 0.16f;

        [Header("Hit Flash")]
        [SerializeField] private Color hitFlashColor = new Color(1f, 0.82f, 0.36f, 1f);
        [SerializeField] private float hitFlashDuration = 0.14f;

        [Header("Death")]
        [SerializeField] private float deathTiltAngle = 72f;
        [SerializeField] private float deathFallDuration = 0.8f;
        [SerializeField] private float deathSinkDistance = 0.08f;

        private MaterialPropertyBlock propertyBlock;

        private PlaceableUnit placeableUnit;
        private Health health;
        private TankVoiceFeedback voiceFeedback;
        private Vector3 baseLocalPosition;
        private Quaternion baseLocalRotation;
        private Vector3 baseLocalScale;
        private Color baseColor = Color.white;
        private bool hasBaseColor;
        private bool isMoving;
        private bool isDead;
        private int previousHealth = -1;
        private float attackTimer;
        private float hitFlashTimer;
        private Coroutine deathRoutine;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
            placeableUnit = GetComponent<PlaceableUnit>();
            health = GetComponent<Health>();
            voiceFeedback = GetComponent<TankVoiceFeedback>();
            ResolveReferences();
            CaptureBasePose();
            CaptureBaseColor();
        }

        private void OnEnable()
        {
            if (health != null)
            {
                previousHealth = health.CurrentHealth;
                health.OnHealthChanged += HandleHealthChanged;
                health.OnDeath += HandleDeath;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnHealthChanged -= HandleHealthChanged;
                health.OnDeath -= HandleDeath;
            }
        }

        private void Update()
        {
            if (visualRoot == null || isDead)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            if (attackTimer > 0f)
            {
                attackTimer = Mathf.Max(0f, attackTimer - deltaTime);
            }

            if (hitFlashTimer > 0f)
            {
                hitFlashTimer = Mathf.Max(0f, hitFlashTimer - deltaTime);
            }

            ApplyPose();
            ApplyHitFlash();
        }

        public void SetMoving(bool moving)
        {
            isMoving = moving;
        }

        public void PlayAttack()
        {
            if (isDead)
            {
                return;
            }

            attackTimer = attackOutDuration + attackReturnDuration;
            if (voiceFeedback != null)
            {
                voiceFeedback.PlayAttackVoice();
            }
        }

        private void ResolveReferences()
        {
            if (visualRoot == null)
            {
                Transform model = transform.Find("Visual/ModelSlot/PF_Tank_Visual/VisualRoot/Tank_Model");
                if (model != null)
                {
                    visualRoot = model;
                }
            }

            if (bodyRenderer == null && visualRoot != null)
            {
                bodyRenderer = visualRoot.GetComponentInChildren<Renderer>(true);
            }
        }

        private void CaptureBasePose()
        {
            if (visualRoot == null)
            {
                return;
            }

            baseLocalPosition = visualRoot.localPosition;
            baseLocalRotation = visualRoot.localRotation;
            baseLocalScale = visualRoot.localScale;
        }

        private void CaptureBaseColor()
        {
            if (bodyRenderer == null || bodyRenderer.sharedMaterial == null)
            {
                return;
            }

            Material material = bodyRenderer.sharedMaterial;
            if (material.HasProperty(BaseColorId))
            {
                baseColor = material.GetColor(BaseColorId);
                hasBaseColor = true;
            }
            else if (material.HasProperty(ColorId))
            {
                baseColor = material.GetColor(ColorId);
                hasBaseColor = true;
            }
        }

        private void ApplyPose()
        {
            bool placed = placeableUnit == null || placeableUnit.IsPlacedOnBoard;
            float idlePhase = Time.time * idleFrequency * Mathf.PI * 2f;
            float idleAmount = placed ? Mathf.Sin(idlePhase) : 0f;
            Vector3 localPosition = baseLocalPosition + Vector3.up * (idleAmount * idleAmplitude);
            Vector3 localScale = baseLocalScale * (1f + (placed ? idleAmount * idleScalePulse : 0f));

            float attack01 = GetAttackCurve();
            localPosition += Vector3.forward * (attack01 * attackLungeDistance);

            float moveLean = isMoving ? Mathf.Sin(Time.time * moveLeanSpeed) * moveLeanAngle : 0f;
            Quaternion rotationOffset = Quaternion.Euler(
                (-attackTiltAngle * attack01) + moveLean,
                0f,
                0f);

            visualRoot.localPosition = localPosition;
            visualRoot.localRotation = baseLocalRotation * rotationOffset;
            visualRoot.localScale = localScale;
        }

        private float GetAttackCurve()
        {
            float total = attackOutDuration + attackReturnDuration;
            if (attackTimer <= 0f || total <= 0f)
            {
                return 0f;
            }

            float elapsed = total - attackTimer;
            if (elapsed <= attackOutDuration)
            {
                return Mathf.SmoothStep(0f, 1f, elapsed / Mathf.Max(attackOutDuration, 0.0001f));
            }

            float returnElapsed = elapsed - attackOutDuration;
            return Mathf.SmoothStep(1f, 0f, returnElapsed / Mathf.Max(attackReturnDuration, 0.0001f));
        }

        private void ApplyHitFlash()
        {
            if (bodyRenderer == null || !hasBaseColor)
            {
                return;
            }

            float flash01 = hitFlashDuration > 0f ? Mathf.Clamp01(hitFlashTimer / hitFlashDuration) : 0f;
            Color currentColor = Color.Lerp(baseColor, hitFlashColor, flash01);

            if (propertyBlock == null)
            {
                propertyBlock = new MaterialPropertyBlock();
            }

            bodyRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, currentColor);
            propertyBlock.SetColor(ColorId, currentColor);
            bodyRenderer.SetPropertyBlock(propertyBlock);
        }

        private void HandleHealthChanged(int current, int max)
        {
            if (previousHealth >= 0 && current < previousHealth)
            {
                hitFlashTimer = hitFlashDuration;
            }

            previousHealth = current;
        }

        private void HandleDeath()
        {
            if (isDead)
            {
                return;
            }

            isDead = true;
            if (deathRoutine != null)
            {
                StopCoroutine(deathRoutine);
            }

            deathRoutine = StartCoroutine(PlayDeathRoutine());
        }

        private IEnumerator PlayDeathRoutine()
        {
            float elapsed = 0f;
            while (elapsed < deathFallDuration && visualRoot != null)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / Mathf.Max(deathFallDuration, 0.0001f));
                float eased = Mathf.SmoothStep(0f, 1f, t);

                visualRoot.localPosition = baseLocalPosition + Vector3.down * (deathSinkDistance * eased);
                visualRoot.localRotation = baseLocalRotation * Quaternion.Euler(deathTiltAngle * eased, 0f, 0f);
                visualRoot.localScale = Vector3.Lerp(baseLocalScale, baseLocalScale * 0.92f, eased);
                yield return null;
            }
        }
    }
}
