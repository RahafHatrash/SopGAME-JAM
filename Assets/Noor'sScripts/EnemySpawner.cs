using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab; // Drag your enemy prefab here in Inspector
    public float spawnInterval = 2f; // Time between spawns
    public int maxEnemiesPerSpawnPoint = 3; // Maximum enemies per spawn point
    
    [Header("Spawn Points")]
    public Transform[] spawnPoints; // Array of spawn point transforms
    public bool useRandomSpawnPoints = true; // If true, randomly select from spawn points
    
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
            currentEnemyCountPerSpawnPoint = new int[1]; // For spawner position fallback
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
            yield return new WaitForSeconds(spawnInterval);
            
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

        // Instantiate enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        
        // Optional: Set enemy's parent to this spawner for organization
        enemy.transform.parent = this.transform;
        
        // Track enemy count for this spawn point
        currentEnemyCountPerSpawnPoint[spawnPointIndex]++;
        
        // Setup event for when enemy is destroyed
        EnemyMov enemyScript = enemy.GetComponent<EnemyMov>();
        if (enemyScript != null)
        {
            // Since EnemyMov doesn't have an event system, we'll use the monitoring approach
            StartCoroutine(MonitorEnemyDestruction(enemy, spawnPointIndex));
        }
        else
        {
            // Fallback: use GameObject destruction event
            StartCoroutine(MonitorEnemyDestruction(enemy, spawnPointIndex));
        }
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
        
        // If no spawn points are defined, spawn at spawner position
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points defined! Spawning at spawner position.");
            return transform.position;
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
            return spawnPoints[0] != null ? spawnPoints[0].position : transform.position;
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

    // Visualize spawn points in Scene view
    void OnDrawGizmosSelected()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            // Draw spawner position if no spawn points
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            return;
        }

        // Draw all spawn points with different colors based on availability
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
            {
                // Green if available, red if at max capacity
                bool isAvailable = currentEnemyCountPerSpawnPoint != null && 
                                 currentEnemyCountPerSpawnPoint[i] < maxEnemiesPerSpawnPoint;
                Gizmos.color = isAvailable ? Color.green : Color.red;
                
                Gizmos.DrawWireSphere(spawnPoints[i].position, 0.5f);
                
                // Draw a line from spawner to spawn point
                Gizmos.color = Color.white;
                Gizmos.DrawLine(transform.position, spawnPoints[i].position);
                
                // Draw enemy count above spawn point
                if (currentEnemyCountPerSpawnPoint != null)
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

