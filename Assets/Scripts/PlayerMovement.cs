using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{

    [Header("Horizontal Movement")]
    public float sideSpeed = 12f; // How fast the bee moves left/right
    public float xLimit = 2.04f; // The horizontal limit (half the width of the screen in world units)
    [Header("Tilt Settings")]
    [SerializeField] private float tiltAngle = 15f; // The angle we want
    [SerializeField] private float tiltSpeed = 10f; // How fast to reach that angle
    [Header("Vertical Arcade Physics")]
    public float baseRiseSpeed = 4.0f;// The upward force when no pollen is carried
    public float weightPerPollen = 5.5f; // How much each pollen weighs down the bee
    public int maxPollen = 3; // Maximum pollen the bee can carry before it becomes too heavy to fly

    [Header("Animation Keys")]
    public string isFlyingParam = "bFlying";
    public string collectTrigger = "Collect";
    public string depositTrigger = "Deposit";
    public string pollenCountParam = "PollenCount";

    [Header("Debug Info (Read Only)")]
    [Tooltip("Final Vertical Force: Rise - (Pollen * Weight)")]
    public float currentNetForce;
    public string currentPollenType = "";

    private Rigidbody2D rb;
    private Animator animator;
    private float targetX;
    private int pollenCount = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.gravityScale = 0; // Manual control
        targetX = transform.position.x;
    }

    void Update()
    {
        // Simple Split-Screen Input (Left/Right)
        if (Input.GetMouseButtonDown(0))
        {
            float screenCenterX = Screen.width / 2f;
            targetX = (Input.mousePosition.x < screenCenterX) ? -xLimit : xLimit;
        }
        UpdateAnimationState();
    }

    void FixedUpdate()
    {
        // Physics Calculation: Upward Lift minus total weight
        currentNetForce = baseRiseSpeed - (pollenCount * weightPerPollen);

        // Movement Calculations
        float nextX = Mathf.MoveTowards(transform.position.x, targetX, sideSpeed * Time.fixedDeltaTime);
        float nextY = transform.position.y + (currentNetForce * Time.fixedDeltaTime);

        // Apply movement without clamping (Bee can leave frame)
        rb.MovePosition(new Vector2(nextX, nextY));
    }

    private void UpdateAnimationState()
    {
        float diff = targetX - transform.position.x;
        bool isMoving = Mathf.Abs(diff) > 0.1f;

        float targetZRotation = 0f; // Default straight
        // Visual Flip
        if (isMoving)
        {
            targetZRotation = (diff > 0) ? -tiltAngle : tiltAngle; // Tilt right or left
            Vector3 s = transform.localScale;
            s.x = (diff > 0) ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
            transform.localScale = s;
        }
        // Smoothly rotate towards target angle
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetZRotation);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * tiltSpeed);
       
        if (animator != null)
        {
            animator.SetBool(isFlyingParam, isMoving);
            animator.SetInteger(pollenCountParam, pollenCount);
        }
    }

    // --- Interaction Methods ---
    public void AddPollen(string type)
    {
        if (pollenCount < maxPollen)
        {
            pollenCount++;
            currentPollenType = type;
            if (animator != null) animator.SetTrigger(collectTrigger);
            Debug.Log("Player: Collected type " + type);
        }
    }

    public void ResetPollen()
    {
        pollenCount = 0;
        currentPollenType = "";
        if (animator != null) animator.SetTrigger(depositTrigger);
        Debug.Log("Player: Deposited and reset weight");
    }

    public int GetPollenCount() => pollenCount;
    public int GetMaxPollen() => maxPollen;

    // --- GIZMOS ---
    private void OnDrawGizmos()
    {
        // Center Line (Yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(0, -10, 0), new Vector3(0, 10, 0));

        // Horizontal Target Points (Cyan)
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(new Vector3(-xLimit, transform.position.y, 0), 0.2f);
        Gizmos.DrawWireSphere(new Vector3(xLimit, transform.position.y, 0), 0.2f);
    }
}