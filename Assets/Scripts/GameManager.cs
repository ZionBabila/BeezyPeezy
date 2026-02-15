using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;

[System.Serializable]
public class FlowerDefinition
{
    public string idName;         // Used for matching (e.g., "Yellow")
    public GameObject prefab;     
    
    [Header("Settings")]
    public string attributes;     
    public bool isFullWithPollen; // True = Pickup, False = Deposit
    
    [Header("Statistics")]
    [Range(0, 100)]
    public float spawnWeight = 50f; 
}

public class GameManager : MonoBehaviour
{
    [Header("---UI Settings---")]
    [SerializeField] private TextMeshProUGUI scoreText;
    private int score = 0;
    public static GameManager Instance;

    [Header("--- Spawning Logic ---")]
    [SerializeField] private FlowerDefinition[] availableFlowers;
    [SerializeField] private float spawnY = 7.0f;
    [SerializeField] private float spawnXOffset = 2.1f;
    [SerializeField] private float initialSpawnRate = 2.0f;
    [SerializeField] private float difficultyMultiplier = 0.99f;

    [Header("--- Movement Engine ---")]
    [SerializeField] private float objectFallSpeed = 4f;
    [SerializeField] private float destroyY = -6f;

    private float spawnTimer;
    private float currentSpawnRate;
    private Dictionary<GameObject, FlowerDefinition> activeObjectsMap = new Dictionary<GameObject, FlowerDefinition>();
    private List<GameObject> movingObjects = new List<GameObject>();

    void Awake() 
    { 
        Instance = this; 
        currentSpawnRate = initialSpawnRate;
    }
    void Start()
    {
        UpdateScoreUI();
    }

    void Update()
    {
        HandleSpawning();
        HandleObjectMovement();
    }

    private void HandleSpawning()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= currentSpawnRate)
        {
            SpawnFlower();
            spawnTimer = 0;
            currentSpawnRate = Mathf.Max(0.5f, currentSpawnRate * difficultyMultiplier);
        }
    }

    private void SpawnFlower()
    {
        if (availableFlowers.Length == 0) return;

        float xPos = (Random.value > 0.5f) ? spawnXOffset : -spawnXOffset;
        Vector3 spawnPos = new Vector3(xPos, spawnY, 0);

        FlowerDefinition def = GetWeightedFlower();
        if (def == null || def.prefab == null) return;

        GameObject newObj = Instantiate(def.prefab, spawnPos, Quaternion.identity);

        FlowerState state = newObj.GetComponent<FlowerState>();
        if (state != null) state.SetVisualState(def.isFullWithPollen);

        activeObjectsMap.Add(newObj, def);
        movingObjects.Add(newObj);
    }

    private FlowerDefinition GetWeightedFlower()
    {
        float total = 0;
        foreach (var f in availableFlowers) total += f.spawnWeight;
        float rnd = Random.Range(0, total);
        float sum = 0;
        foreach (var f in availableFlowers)
        {
            sum += f.spawnWeight;
            if (rnd <= sum) return f;
        }
        return availableFlowers[0];
    }

    private void HandleObjectMovement()
    {
        for (int i = movingObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = movingObjects[i];
            if (obj == null) continue;

            obj.transform.Translate(Vector3.down * objectFallSpeed * Time.deltaTime);

            if (obj.transform.position.y < destroyY)
            {
                activeObjectsMap.Remove(obj);
                movingObjects.RemoveAt(i);
                Destroy(obj);
            }
        }
    }

    public void ProcessCollision(GameObject hitObject, PlayerMovement player)
    {
        if (!activeObjectsMap.ContainsKey(hitObject)) return;

        FlowerDefinition def = activeObjectsMap[hitObject];
        FlowerState flowerState = hitObject.GetComponent<FlowerState>();
        if (flowerState == null) return;

        // Interaction Logic
        if (flowerState.isFull) // Try Pick up
        {
            bool canCollect = player.GetPollenCount() == 0 || player.currentPollenType == def.idName;
            if (player.GetPollenCount() < player.GetMaxPollen() && canCollect)
            {
                player.AddPollen(def.idName);
                flowerState.SetVisualState(false);
            }
        }
        else // Try Deposit
        {
            if (player.GetPollenCount() > 0 && player.currentPollenType == def.idName)
            {
                player.ResetPollen();
                flowerState.SetVisualState(true);
                AddScore(1); // Example scoring
            }
        }
    }
    private void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }   
private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(new Vector3(spawnXOffset, spawnY, 0), 0.3f);
        Gizmos.DrawWireSphere(new Vector3(-spawnXOffset, spawnY, 0), 0.3f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-5, destroyY, 0), new Vector3(5, destroyY, 0));
    }
}