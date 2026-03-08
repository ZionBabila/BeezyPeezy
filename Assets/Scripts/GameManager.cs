using UnityEngine;
using System.Collections.Generic;
using TMPro;

// 1. Simple Enum for Color IDs
public enum FlowerID { Pink, Yellow, Blue, Purple, White, Red }

// 2. Data structure for a single flower in a wave
[System.Serializable]
public struct FlowerSpawnData
{
    public FlowerID flowerID; 
    public bool isFull;       
}

// 3. Data structure for the flower templates (The "Makhsan")
[System.Serializable]
public class FlowerDefinition
{
    public FlowerID flowerID;      
    public bool isFullWithPollen;  
    public GameObject prefab;      
    
    [Header("Statistics")]
    [Range(0, 100)]
    public float spawnWeight = 50f; 
}

// 4. Sequence structure for the waves - Now includes Spacing
[System.Serializable]
public struct FlowerWave
{
    public string waveName; 
    public float verticalSpacing;        // The vertical gap between flowers in this specific wave
    public FlowerSpawnData[] flowerSequence; 
}

public class GameManager : MonoBehaviour
{
    [Header("---UI Settings---")]
    [SerializeField] private TextMeshProUGUI scoreText;
    private int score = 0;
    public static GameManager Instance;

    [Header("--- Spawning Logic ---")]
    [SerializeField] private FlowerDefinition[] availableFlowers;
    [SerializeField] private float spawnY = 8.0f; // Base height for the first flower in a wave
    [SerializeField] private float spawnXOffset = 2.1f;
    [SerializeField] private float initialSpawnRate = 3.5f; // Time between whole waves
    [SerializeField] private float difficultyMultiplier = 0.99f;

    [Header("--- Sequence Settings ---")]
    public bool useSequencing = true; 
    public FlowerWave[] waves;        
    private int currentWaveIndex = 0;

    [Header("--- Movement Engine ---")]
    [SerializeField] private float objectFallSpeed = 4f;
    [SerializeField] private float destroyY = -7f;

    private float spawnTimer;
    private float currentSpawnRate;
    private Dictionary<GameObject, FlowerDefinition> activeObjectsMap = new Dictionary<GameObject, FlowerDefinition>();
    private List<GameObject> movingObjects = new List<GameObject>();

    void Awake() 
    { 
        Instance = this; 
        currentSpawnRate = initialSpawnRate;
    }

    void Start() { UpdateScoreUI(); }

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
            SpawnFlowerWave(); // Trigger the entire wave at once
            spawnTimer = 0;
            currentSpawnRate = Mathf.Max(1.5f, currentSpawnRate * difficultyMultiplier);
        }
    }

    private void SpawnFlowerWave()
    {
        if (availableFlowers.Length == 0) return;

        if (useSequencing && waves.Length > 0)
        {
            FlowerWave wave = waves[currentWaveIndex];
            
            // Loop through the entire sequence of the current wave
            for (int i = 0; i < wave.flowerSequence.Length; i++)
            {
                FlowerSpawnData data = wave.flowerSequence[i];
                FlowerDefinition def = FindDefinition(data.flowerID, data.isFull);

                if (def != null)
                {
                    // Calculate Y position: Base spawnY + (index * spacing)
                    // This creates the vertical "train" effect
                    float staggeredY = spawnY + (i * wave.verticalSpacing);
                    
                    // Decide side (Left/Right) randomly for each flower in the wave
                    float xPos = (Random.value > 0.5f) ? spawnXOffset : -spawnXOffset;
                    
                    CreateFlower(def, new Vector3(xPos, staggeredY, 0));
                }
            }

            // Move to the next wave for the next spawn cycle
            currentWaveIndex = (currentWaveIndex + 1) % waves.Length;
        }
        else
        {
            // Fallback: spawn a single weighted random flower if sequencing is off
            FlowerDefinition def = GetWeightedFlower();
            if (def != null) 
            {
                float xPos = (Random.value > 0.5f) ? spawnXOffset : -spawnXOffset;
                CreateFlower(def, new Vector3(xPos, spawnY, 0));
            }
        }
    }

    // Helper: Finds the matching prefab from availableFlowers
    private FlowerDefinition FindDefinition(FlowerID id, bool full)
    {
        foreach (var f in availableFlowers)
        {
            if (f.flowerID == id && f.isFullWithPollen == full) return f;
        }
        return null;
    }

    // Helper: Handles instantiation and registration
    private void CreateFlower(FlowerDefinition def, Vector3 pos)
    {
        GameObject newObj = Instantiate(def.prefab, pos, Quaternion.identity);
        
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

        string pollenID = def.flowerID.ToString();

        if (flowerState.isFull) 
        {
            if (player.AddPollen(pollenID)) 
            {
                flowerState.SetVisualState(false);
            }
        }
        else 
        {
            if (player.RemovePollen(pollenID))
            {
                flowerState.SetVisualState(true);
                AddScore(1);
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
        if (scoreText != null) scoreText.text = "Score: " + score;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(new Vector3(spawnXOffset, spawnY, 0), 0.3f);
        Gizmos.DrawWireSphere(new Vector3(-spawnXOffset, spawnY, 0), 0.3f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-10, destroyY, 0), new Vector3(10, destroyY, 0));
    }
}