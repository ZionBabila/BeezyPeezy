using UnityEngine;

public class PlayerDetect : MonoBehaviour
{
    private PlayerMovement playerMovement;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Debugging to see if collision happens at all
        Debug.Log("Collision Detected with: " + other.name);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ProcessCollision(other.gameObject, playerMovement);
        }
    }
}