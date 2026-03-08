using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PollenSackSlot
{
    public string colorID;      // Must match the FlowerDefinition idName (e.g., "Pink")
    public GameObject sackObject; // The child GameObject of the bee

    [HideInInspector] public bool isFull = false;
    [HideInInspector] public Quaternion initialRotation; // save the initial rotation of the sack for potential future use
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("VFX")]
    [SerializeField] private ParticleSystem boostParticles; // Reference to the smoke system
    [Header("Tap to Fly Settings")]
    [SerializeField] private float tapBoostPower = 2.0f; // How much force each click adds
    [SerializeField] private float tapDecay = 5.0f;      // How fast the boost fades away
    private float currentTapBoost = 0f;                // The current accumulated boost force
    [Header("Horizontal Movement")]
    public float sideSpeed = 12f;
    public float xLimit = 2.04f;

    [Header("Visual Settings")]
    [Tooltip("Drag the child GameObject that contains the Bee Sprite/Animator here")]
    public Transform beeVisualNode;
    [SerializeField] private float tiltAngle = 15f;
    [SerializeField] private float tiltSpeed = 10f;

    [Header("Vertical Arcade Physics")]
    public float baseRiseSpeed = 4.0f;
    public float weightPerPollen = 5.5f;

    [Header("Pollen Sack Management")]
    public PollenSackSlot[] sackSlots;

    [Header("Wobble Animation")]
    [SerializeField] private float wobbleSpeed = 5.0f;
    [SerializeField] private float wobbleAngle = 12.0f;

    [Header("Animation Keys")]
    public string isFlyingParam = "bFlying";
    public string collectTrigger = "Collect";
    public string depositTrigger = "Deposit";
    public string pollenCountParam = "PollenCount";

    private Rigidbody2D rb;
    private Animator animator;
    private float targetX;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.gravityScale = 0;
        targetX = transform.position.x;

        InitializeSacks();
    }

    private void InitializeSacks()
    {
        foreach (var slot in sackSlots)
        {
            if (slot.sackObject != null)
            {
                // Capture the starting rotation for the wobble effect
                slot.initialRotation = slot.sackObject.transform.localRotation;
                // Logic: If the sack is placed on the right side of the bee center (X > 0), 
                // flip its sprite scale initially so it faces the correct direction.
                if (slot.sackObject.transform.localPosition.x > 0)
                {
                    Vector3 scale = slot.sackObject.transform.localScale;
                    scale.x = -Mathf.Abs(scale.x);
                    slot.sackObject.transform.localScale = scale;
                }

                slot.sackObject.SetActive(false);
                slot.isFull = false;
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 1. Calculate horizontal target based on click side
            float screenCenterX = Screen.width / 2f;
            targetX = (Input.mousePosition.x < screenCenterX) ? -xLimit : xLimit;

            // 2. Add upward energy for the "Tap to Fly" mechanic
            currentTapBoost += tapBoostPower;

            // 3. Visual Feedback: Trigger the smoke burst
            if (boostParticles != null)
            {
                boostParticles.Stop(); // Reset the system so it can play again immediately
                boostParticles.Play(); // Emit the burst of particles
            }
        }

        // 4. Handle the decay of the boost force over time
        if (currentTapBoost > 0)
        {
            currentTapBoost -= tapDecay * Time.deltaTime; // Reduce boost gradually
            currentTapBoost = Mathf.Max(0, currentTapBoost); // Ensure it never goes below zero
        }
        UpdateAnimationState();
        ApplySackWobble();
    }

    void FixedUpdate()
    {
        float currentNetForce = baseRiseSpeed + currentTapBoost - (GetPollenCount() * weightPerPollen);
        float nextX = Mathf.MoveTowards(transform.position.x, targetX, sideSpeed * Time.fixedDeltaTime);
        float nextY = transform.position.y + (currentNetForce * Time.fixedDeltaTime);
        rb.MovePosition(new Vector2(nextX, nextY));
    }

    private void UpdateAnimationState()
    {
        float diff = targetX - transform.position.x;
        bool isMoving = Mathf.Abs(diff) > 0.1f;

        float targetZRotation = 0f;
        if (isMoving)
        {
            targetZRotation = (diff > 0) ? -tiltAngle : tiltAngle;

            // --- FIXED FLIP LOGIC ---
            // Instead of flipping the WHOLE bee (this transform), 
            // we only flip the visual child (beeVisualNode).
            // This ensures the sacks (siblings of the visual node) stay in place.
            if (beeVisualNode != null)
            {
                Vector3 s = beeVisualNode.localScale;
                s.x = (diff > 0) ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
                beeVisualNode.localScale = s;
            }
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, targetZRotation), Time.deltaTime * tiltSpeed);

        if (animator != null)
        {
            animator.SetBool(isFlyingParam, isMoving);
            animator.SetInteger(pollenCountParam, GetPollenCount());
        }
    }

    public bool AddPollen(string type)
    {
        foreach (var slot in sackSlots)
        {
            if (slot.colorID == type && !slot.isFull)
            {
                slot.isFull = true;
                slot.sackObject.SetActive(true);
                if (animator != null) animator.SetTrigger(collectTrigger);
                return true;
            }
        }
        return false;
    }

    public bool RemovePollen(string type)
    {
        foreach (var slot in sackSlots)
        {
            if (slot.colorID == type && slot.isFull)
            {
                slot.isFull = false;
                slot.sackObject.SetActive(false);
                if (animator != null) animator.SetTrigger(depositTrigger);
                return true;
            }
        }
        return false;
    }

    public int GetPollenCount()
    {
        int count = 0;
        foreach (var slot in sackSlots) if (slot.isFull) count++;
        return count;
    }
    private void ApplySackWobble()
    {
        // חישוב זווית הנידנוד לפי זמן: sin(זמן * מהירות) * עוצמה
        float wobbleZ = Mathf.Sin(Time.time * wobbleSpeed) * wobbleAngle;

        foreach (var slot in sackSlots)
        {
            // אנחנו מנדנדים רק שקים שהם "מלאים"
            if (slot.isFull && slot.sackObject != null)
            {
                // עדכון הרוטציה המקומית: הזווית המקורית כפול זווית הנידנוד החדשה
                slot.sackObject.transform.localRotation = slot.initialRotation * Quaternion.Euler(0, 0, wobbleZ);
            }
        }
    }
}