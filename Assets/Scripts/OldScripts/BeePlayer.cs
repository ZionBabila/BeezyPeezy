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

        // Ensure gravity is zero
        rb.gravityScale = 0;

        // Initialize target position
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
        // Input logic (Mouse or Touch)
        if (Input.GetMouseButton(0))
        {
            Vector3 touchPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            
            // Clamp movement within screen boundaries
            float clampedX = Mathf.Clamp(touchPosition.x, -xLimit, xLimit);
            float clampedY = Mathf.Clamp(touchPosition.y, minY, maxY);

            targetPosition = new Vector2(clampedX, clampedY);
        }
    }

    void MoveBee()
    {
        Vector2 smoothedPosition = Vector2.Lerp(rb.position, targetPosition, smoothSpeed * Time.fixedDeltaTime);
        rb.MovePosition(smoothedPosition);
    }

    // --- Collision Logic ---

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Case 1: Hit floating pollen -> Collect it
        if (other.CompareTag("Pollen"))
        {
            CollectPollen(other.gameObject);
        }
        // Case 2: Hit an empty flower -> Try to deposit pollen
        else if (other.CompareTag("Flower"))
        {
            DepositPollen(other.gameObject);
        }
        // Case 3: Hit a flower that already has pollen -> Do nothing (or feedback)
        else if (other.CompareTag("FullFlower"))
        {
            Debug.Log("Interaction ignored: Flower is already full.");
        }
    }

    void CollectPollen(GameObject pollenObj)
    {
        if (!hasPollen)
        {
            hasPollen = true;
            Debug.Log("Pollen Collected!");
            
            // Destroy the pollen object to simulate picking it up
            Destroy(pollenObj);
        }
        else
        {
            Debug.Log("Cannot collect: Player already has pollen.");
        }
    }

    void DepositPollen(GameObject flowerObj)
    {
        if (hasPollen)
        {
            hasPollen = false;
            Debug.Log("Success: Pollen Deposited on Empty Flower!");

            // Visual feedback: Destroy the empty flower 
            // (In a real game, you might swap the sprite to a 'Full Flower' instead)
            Destroy(flowerObj);
        }
        else
        {
            Debug.Log("Failed: Hit Empty Flower, but player has no pollen.");
        }
    }
}