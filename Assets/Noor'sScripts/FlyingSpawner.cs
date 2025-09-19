using UnityEngine;
using System.Collections;

public class FlyingSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject flyingEnemyPrefab; // Drag your flying enemy prefab here in Inspector
    public float spawnInterval = 3f; // Time between spawns
    public int maxEnemies = 5; // Maximum flying enemies at once
    
    [Header("Spawn Configuration")]
    public float spawnHeight = 3f; // Height above ground to spawn flying enemies
    public bool spawnFromLeft = true; // If true, spawns from left edge, if false from right edge
    
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
        // Get camera to determine screen edges
        Camera cam = Camera.main;
        if (cam == null) return;
        
        // Calculate spawn position at screen edge
        Vector2 screenBounds = cam.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        Vector2 screenMin = cam.ScreenToWorldPoint(Vector2.zero);
        
        Vector3 spawnPosition;
        if (spawnFromLeft)
        {
            // Spawn from left edge of screen
            spawnPosition = new Vector3(screenMin.x - 1f, spawnHeight, 0f);
        }
        else
        {
            // Spawn from right edge of screen
            spawnPosition = new Vector3(screenBounds.x + 1f, spawnHeight, 0f);
        }

        // Instantiate flying enemy
        GameObject enemy = Instantiate(flyingEnemyPrefab, spawnPosition, Quaternion.identity);
        
        // Set enemy's parent to this spawner for organization
        enemy.transform.parent = this.transform;
        
        // Track enemy count
        currentEnemyCount++;
        
        // Setup monitoring for when enemy is destroyed
        StartCoroutine(MonitorEnemyDestruction(enemy));
    }

    IEnumerator MonitorEnemyDestruction(GameObject enemy)
    {
        yield return new WaitUntil(() => enemy == null);
        currentEnemyCount--;
    }

    // Visualize spawn points in Scene view
    void OnDrawGizmosSelected()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        
        Vector2 screenBounds = cam.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        Vector2 screenMin = cam.ScreenToWorldPoint(Vector2.zero);
        
        // Draw spawn points
        Gizmos.color = Color.blue;
        
        if (spawnFromLeft)
        {
            // Draw left spawn point
            Vector3 leftSpawn = new Vector3(screenMin.x - 1f, spawnHeight, 0f);
            Gizmos.DrawWireSphere(leftSpawn, 0.5f);
            Gizmos.DrawLine(leftSpawn, leftSpawn + Vector3.right * 2f);
        }
        else
        {
            // Draw right spawn point
            Vector3 rightSpawn = new Vector3(screenBounds.x + 1f, spawnHeight, 0f);
            Gizmos.DrawWireSphere(rightSpawn, 0.5f);
            Gizmos.DrawLine(rightSpawn, rightSpawn + Vector3.left * 2f);
        }
    }
}