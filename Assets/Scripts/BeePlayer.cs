using UnityEngine;

public class BeePlayer : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("How fast the bee follows the finger/mouse")]
    [SerializeField] private float smoothSpeed = 15f;

    [Header("Boundaries (Inspector Controlled)")]
    [Tooltip("The limit for left/right movement (X axis)")]
    [SerializeField] private float xLimit = 2.5f;

    [Tooltip("The lowest point the bee can reach (Y axis)")]
    [SerializeField] private float minY = -3.0f;

    [Tooltip("The highest point the bee can reach (Y axis)")]
    [SerializeField] private float maxY = 3.0f;

    [Header("Game State")]
    public bool hasPollen = false;

    private Rigidbody2D rb;
    private Vector2 targetPosition;
    private Camera mainCamera;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        // Ensure gravity is zero as requested
        rb.gravityScale = 0;

        // Initialize target position to current position
        targetPosition = rb.position;
    }

    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        MoveBee();
    }

    void HandleInput()
    {
        // Input.GetMouseButton(0) works for both Mouse Click (PC) and Single Touch (Mobile)
        if (Input.GetMouseButton(0))
        {
            // Convert screen pixel coordinates to world coordinates
            Vector3 touchPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

            // Clamp the X position to keep the bee within screen width
            float clampedX = Mathf.Clamp(touchPosition.x, -xLimit, xLimit);

            // Clamp the Y position to limit vertical movement based on Inspector settings
            float clampedY = Mathf.Clamp(touchPosition.y, minY, maxY);

            // Set the new target position
            targetPosition = new Vector2(clampedX, clampedY);
        }
        else
        {
            // Optional: If no input is detected, you can choose to stop the bee 
            // or let it drift. Currently, it stays at the last targetPosition.
        }
    }

    void MoveBee()
    {
        // Smoothly move the Rigidbody towards the target position
        Vector2 smoothedPosition = Vector2.Lerp(rb.position, targetPosition, smoothSpeed * Time.fixedDeltaTime);
        rb.MovePosition(smoothedPosition);
    }

    // Collision detection logic
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Pollen"))
        {
            CollectPollen(other.gameObject);
        }
        else if (other.CompareTag("Flower"))
        {
            DepositPollen(other.gameObject);
        }
    }

    void CollectPollen(GameObject pollenObj)
    {
        if (!hasPollen)
        {
            hasPollen = true;
            // Add visual feedback or sound here
            Debug.Log("Pollen Collected"); 
            Destroy(pollenObj);
        }
    }

    void DepositPollen(GameObject flowerObj)
    {
        if (hasPollen)
        {
            hasPollen = false;
            // Add score or visual feedback here
            Debug.Log("Pollen Deposited");
        }
    }
}