using UnityEngine;

public class BottomDestroyer : MonoBehaviour
{
    // Ensure the GameObject with this script has a Collider2D set to "Is Trigger"
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object hitting the collider is a Flower or Pollen
        // (Optional: You can check tags if you want to be specific)
        if (other.CompareTag("Flower") || other.CompareTag("Pollen"))
        {
            Destroy(other.gameObject);
        }
        // If you don't use tags, you can just use: Destroy(other.gameObject);
    }
}