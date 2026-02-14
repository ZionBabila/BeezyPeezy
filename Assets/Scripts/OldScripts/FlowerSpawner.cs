using UnityEngine;

public class FlowerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Drag your 4 flower prefabs here")]
    [SerializeField] private GameObject[] flowerPrefabs;

    [Tooltip("Time in seconds between each spawn")]
    [SerializeField] private float spawnInterval = 1.5f;

    [Header("Position Settings")]
    [Tooltip("The Y position where flowers spawn (Top of screen)")]
    [SerializeField] private float spawnY = 6.0f;

    [Tooltip("The X distance from center. Creates the Left/Right lanes")]
    [SerializeField] private float xOffset = 2.0f;

    [Header("Debug / Gizmos")]
    [Tooltip("Size of the gizmo sphere in the scene view")]
    [SerializeField] private float gizmoRadius = 0.5f;
    [Tooltip("Color of the spawn points")]
    [SerializeField] private Color gizmoColor = Color.green;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnFlower();
            timer = 0;
        }
    }

    void SpawnFlower()
    {
        // 1. Validation: Ensure we have prefabs assigned
        if (flowerPrefabs.Length == 0) return;

        // 2. Pick a random flower type (0 to 3)
        int randomIndex = Random.Range(0, flowerPrefabs.Length);
        GameObject selectedPrefab = flowerPrefabs[randomIndex];

        // 3. Pick a random side (Left or Right)
        float spawnX = (Random.value > 0.5f) ? xOffset : -xOffset;

        // 4. Create the spawn position vector
        Vector3 spawnPos = new Vector3(spawnX, spawnY, 0);

        // 5. Instantiate (Create) the flower
        Instantiate(selectedPrefab, spawnPos, Quaternion.identity);
    }

    // This function draws visuals in the Scene view to help you setup positions
    private void OnDrawGizmos()
    {
        // Calculate the two exact spawn positions
        Vector3 leftSpawnPos = new Vector3(-xOffset, spawnY, 0);
        Vector3 rightSpawnPos = new Vector3(xOffset, spawnY, 0);
        Vector3 centerPos = new Vector3(0, spawnY, 0);

        // Draw the Spawn Points (Left and Right)
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(leftSpawnPos, gizmoRadius);
        Gizmos.DrawWireSphere(rightSpawnPos, gizmoRadius);
        
        // Label the spheres (Optional - works in newer Unity versions or with handles, 
        // but spheres are usually enough)

        // Draw the Center and connection line
        Gizmos.color = Color.red;
        // Draw a line connecting left and right to visualize the "Lane" width
        Gizmos.DrawLine(leftSpawnPos, rightSpawnPos);
        // Draw a small sphere at the center (0 on X axis)
        Gizmos.DrawWireSphere(centerPos, gizmoRadius / 2);
    }
}