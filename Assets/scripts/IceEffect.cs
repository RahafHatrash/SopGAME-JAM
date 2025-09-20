using UnityEngine;
using System.Collections;

public class IceEffect : MonoBehaviour
{
    public float freezeDuration = 3f; // Increased duration for better gameplay

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") && collision.gameObject.name.ToLower().Contains("ping"))
        {
            EnemyMov enemyMov = collision.GetComponent<EnemyMov>();
            if (enemyMov != null)
            {
                StartCoroutine(FreezeEnemyMov(enemyMov));
                Destroy(gameObject);
                return;
            }
            
            FlyingEnemy flyingEnemy = collision.GetComponent<FlyingEnemy>();
            if (flyingEnemy != null)
            {
                StartCoroutine(FreezeFlyingEnemy(flyingEnemy));
                Destroy(gameObject);
                return;
            }
            
            EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                StartCoroutine(FreezeEnemyHealth(enemyHealth));
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator FreezeEnemyMov(EnemyMov enemy)
    {
        if (enemy == null) yield break;

        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
        Color originalColor = sr != null ? sr.color : Color.white;

        // Apply ice color with more visible effect
        if (sr != null) 
        {
            sr.color = new Color(0.3f, 0.7f, 1f, 1f); // Brighter ice blue
        }
        
        Debug.Log($"[IceEffect] Freezing EnemyMov: {enemy.name}");

        // Use the enemy's own Freeze function (this handles movement stopping)
        enemy.Freeze(freezeDuration);
        
        
        // Wait for freeze duration
        yield return new WaitForSeconds(freezeDuration);

        // Restore original color
        if (sr != null) sr.color = originalColor;
        Debug.Log($"[IceEffect] Unfrozen EnemyMov: {enemy.name} - Color restored to: {originalColor}");
    }
    
    private IEnumerator FreezeFlyingEnemy(FlyingEnemy enemy)
    {
        if (enemy == null) yield break;

        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
        Color originalColor = sr != null ? sr.color : Color.white;

        // Apply ice color with more visible effect
        if (sr != null) 
        {
            sr.color = new Color(0.3f, 0.7f, 1f, 1f); // Brighter ice blue
        }
        
        Debug.Log($"[IceEffect] Freezing FlyingEnemy: {enemy.name}");

        // Use the enemy's own Freeze function (this handles movement stopping)
        enemy.Freeze(freezeDuration);
        
        
        // Wait for freeze duration
        yield return new WaitForSeconds(freezeDuration);

        // Restore original color
        if (sr != null) sr.color = originalColor;
        Debug.Log($"[IceEffect] Unfrozen FlyingEnemy: {enemy.name} - Color restored to: {originalColor}");
    }
    
    private IEnumerator FreezeEnemyHealth(EnemyHealth enemy)
    {
        if (enemy == null) yield break;

        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
        Color originalColor = sr != null ? sr.color : Color.white;

        // Stop movement and apply ice color
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (sr != null) sr.color = new Color(0.3f, 0.7f, 1f, 1f); // Brighter ice blue
        
        // Mark enemy as frozen (disable script)
        enemy.enabled = false;
        Debug.Log($"[IceEffect] Frozen EnemyHealth: {enemy.name}");

        yield return new WaitForSeconds(freezeDuration);

        // Restore original state
        enemy.enabled = true;
        if (sr != null) sr.color = originalColor;
        Debug.Log($"[IceEffect] Unfrozen EnemyHealth: {enemy.name}");
    }
}
