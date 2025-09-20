using UnityEngine;
using System.Collections;

public class IceEffect : MonoBehaviour
{
    public float freezeDuration = 2f;

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

        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
        Color originalColor = sr != null ? sr.color : Color.white;

        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (sr != null) sr.color = Color.cyan;

        enemy.Freeze(freezeDuration);
        yield return new WaitForSeconds(freezeDuration);

        if (sr != null) sr.color = originalColor;
    }
    
    private IEnumerator FreezeFlyingEnemy(FlyingEnemy enemy)
    {
        if (enemy == null) yield break;

        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
        Color originalColor = sr != null ? sr.color : Color.white;

        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (sr != null) sr.color = Color.cyan;

        enemy.Freeze(freezeDuration);
        yield return new WaitForSeconds(freezeDuration);

        if (sr != null) sr.color = originalColor;
    }
    
    private IEnumerator FreezeEnemyHealth(EnemyHealth enemy)
    {
        if (enemy == null) yield break;

        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
        Color originalColor = sr != null ? sr.color : Color.white;

        if (rb != null) rb.linearVelocity = Vector2.zero;
        enemy.enabled = false;
        if (sr != null) sr.color = Color.cyan;

        yield return new WaitForSeconds(freezeDuration);

        enemy.enabled = true;
        if (sr != null) sr.color = originalColor;
    }
}
