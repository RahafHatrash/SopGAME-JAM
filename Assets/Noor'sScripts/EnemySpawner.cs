using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab; // Drag your enemy prefab here in Inspector
    public float spawnInterval = 2f; // Time between spawns
    public int maxEnemies = 10; // Maximum enemies at once
    
    [Header("Spawn Area")]
    public Vector2 spawnAreaMin = new Vector2(-2f, -2f);
    public Vector2 spawnAreaMax = new Vector2(2f, 2f);
    
    private int currentEnemyCount = 0;
    private Coroutine spawnCoroutine;

    void Start()
    {
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
            
            if (currentEnemyCount < maxEnemies)
            {
                SpawnEnemy();
            }
        }
    }

    void SpawnEnemy()
    {
        // Calculate random position within spawn area
        Vector3 spawnPosition = new Vector3(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            Random.Range(spawnAreaMin.y, spawnAreaMax.y),
            0f
        );

        // Instantiate enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        
        // Optional: Set enemy's parent to this spawner for organization
        enemy.transform.parent = this.transform;
        
        // Track enemy count
        currentEnemyCount++;
        
        // Setup event for when enemy is destroyed
        EnemyMov enemyScript = enemy.GetComponent<EnemyMov>();
        if (enemyScript != null)
        {
            // Since EnemyMov doesn't have an event system, we'll use the monitoring approach
            StartCoroutine(MonitorEnemyDestruction(enemy));
        }
        else
        {
            // Fallback: use GameObject destruction event
            StartCoroutine(MonitorEnemyDestruction(enemy));
        }
    }

    IEnumerator MonitorEnemyDestruction(GameObject enemy)
    {
        yield return new WaitUntil(() => enemy == null);
        currentEnemyCount--;
    }

    // Visualize spawn area in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = new Vector3(
            (spawnAreaMin.x + spawnAreaMax.x) / 2,
            (spawnAreaMin.y + spawnAreaMax.y) / 2,
            0
        );
        Vector3 size = new Vector3(
            spawnAreaMax.x - spawnAreaMin.x,
            spawnAreaMax.y - spawnAreaMin.y,
            0.1f
        );
        Gizmos.DrawWireCube(center, size);
    }
}

