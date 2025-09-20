using UnityEngine;
using System.Collections;

public class FlyingSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject flyingEnemyPrefab; // Drag your flying enemy prefab here in Inspector
    public float spawnInterval = 3f; // Base time between spawns
    public float spawnIntervalRandomness = 1f; // Random variation added to spawn interval
    public int maxEnemiesPerSpawnPoint = 2; // Maximum flying enemies per spawn point
    
    [Header("Spawn Points")]
    public Transform[] spawnPoints; // Array of spawn point transforms
    public bool useRandomSpawnPoints = true; // If true, randomly select from spawn points
    
    [Header("Fallback Spawn Configuration")]
    public float spawnHeight = 3f; // Height above ground to spawn flying enemies (fallback)
    public bool spawnFromLeft = true; // If true, spawns from left edge, if false from right edge (fallback)
    
    private int[] currentEnemyCountPerSpawnPoint; // Track enemies per spawn point
    private Coroutine spawnCoroutine;

    void Start()
    {
        // Initialize enemy count tracking array
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            currentEnemyCountPerSpawnPoint = new int[spawnPoints.Length];
        }
        else
        {
            currentEnemyCountPerSpawnPoint = new int[1]; // For fallback spawn
        }
        
        StartSpawning();
    }

    public void StartSpawning()
    {
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
        
        spawnCoroutine = StartCoroutine(SpawnEnemies());
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
    }

    IEnumerator SpawnEnemies()
    {
        while (true)
        {
            // Calculate random spawn interval
            float randomInterval = spawnInterval + Random.Range(-spawnIntervalRandomness, spawnIntervalRandomness);
            // Ensure minimum interval of 0.1 seconds
            randomInterval = Mathf.Max(0.1f, randomInterval);
            
            yield return new WaitForSeconds(randomInterval);
            
            // Check if any spawn point has room for more enemies
            if (HasAvailableSpawnPoint())
            {
                SpawnEnemy();
            }
        }
    }

    void SpawnEnemy()
    {
        // Get spawn position and spawn point index
        Vector3 spawnPosition = GetSpawnPosition(out int spawnPointIndex);

        // Instantiate flying enemy
        GameObject enemy = Instantiate(flyingEnemyPrefab, spawnPosition, Quaternion.identity);
        
        // Set enemy's parent to this spawner for organization
        enemy.transform.parent = this.transform;
        
        // Track enemy count for this spawn point
        currentEnemyCountPerSpawnPoint[spawnPointIndex]++;
        
        // Setup monitoring for when enemy is destroyed
        StartCoroutine(MonitorEnemyDestruction(enemy, spawnPointIndex));
    }

    IEnumerator MonitorEnemyDestruction(GameObject enemy, int spawnPointIndex)
    {
        yield return new WaitUntil(() => enemy == null);
        currentEnemyCountPerSpawnPoint[spawnPointIndex]--;
    }

    bool HasAvailableSpawnPoint()
    {
        for (int i = 0; i < currentEnemyCountPerSpawnPoint.Length; i++)
        {
            if (currentEnemyCountPerSpawnPoint[i] < maxEnemiesPerSpawnPoint)
            {
                return true;
            }
        }
        return false;
    }

    Vector3 GetSpawnPosition(out int spawnPointIndex)
    {
        spawnPointIndex = 0;
        
        // If no spawn points are defined, use fallback screen edge spawning
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points defined! Using fallback screen edge spawning.");
            return GetFallbackSpawnPosition();
        }

        // Find available spawn points (those under the limit)
        System.Collections.Generic.List<int> availableSpawnPoints = new System.Collections.Generic.List<int>();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null && currentEnemyCountPerSpawnPoint[i] < maxEnemiesPerSpawnPoint)
            {
                availableSpawnPoints.Add(i);
            }
        }

        // If no spawn points are available, use the first one (fallback)
        if (availableSpawnPoints.Count == 0)
        {
            spawnPointIndex = 0;
            return spawnPoints[0] != null ? spawnPoints[0].position : GetFallbackSpawnPosition();
        }

        // Select from available spawn points
        if (useRandomSpawnPoints)
        {
            spawnPointIndex = availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count)];
        }
        else
        {
            // Use the first available spawn point
            spawnPointIndex = availableSpawnPoints[0];
        }

        return spawnPoints[spawnPointIndex].position;
    }

    Vector3 GetFallbackSpawnPosition()
    {
        // Get camera to determine screen edges (fallback)
        Camera cam = Camera.main;
        if (cam == null) return transform.position;
        
        Vector2 screenBounds = cam.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        Vector2 screenMin = cam.ScreenToWorldPoint(Vector2.zero);
        
        if (spawnFromLeft)
        {
            return new Vector3(screenMin.x - 1f, spawnHeight, 0f);
        }
        else
        {
            return new Vector3(screenBounds.x + 1f, spawnHeight, 0f);
        }
    }

    // Visualize spawn points in Scene view
    void OnDrawGizmosSelected()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            // Draw fallback spawn point
            Vector3 fallbackSpawn = GetFallbackSpawnPosition();
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(fallbackSpawn, 0.5f);
            Gizmos.DrawLine(transform.position, fallbackSpawn);
            return;
        }

        // Draw all spawn points with different colors based on availability
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
            {
                // Green if available, red if at max capacity
                bool isAvailable = true; // Default to available in editor
                if (currentEnemyCountPerSpawnPoint != null && i < currentEnemyCountPerSpawnPoint.Length)
                {
                    isAvailable = currentEnemyCountPerSpawnPoint[i] < maxEnemiesPerSpawnPoint;
                }
                
                Gizmos.color = isAvailable ? Color.green : Color.red;
                Gizmos.DrawWireSphere(spawnPoints[i].position, 0.5f);
                
                // Draw a line from spawner to spawn point
                Gizmos.color = Color.white;
                Gizmos.DrawLine(transform.position, spawnPoints[i].position);
                
                // Draw enemy count above spawn point
                if (currentEnemyCountPerSpawnPoint != null && i < currentEnemyCountPerSpawnPoint.Length)
                {
                    Vector3 labelPos = spawnPoints[i].position + Vector3.up * 1f;
                    #if UNITY_EDITOR
                    UnityEditor.Handles.Label(labelPos, $"{currentEnemyCountPerSpawnPoint[i]}/{maxEnemiesPerSpawnPoint}");
                    #endif
                }
            }
        }
    }
}