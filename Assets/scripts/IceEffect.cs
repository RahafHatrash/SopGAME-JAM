using UnityEngine;
using System.Collections;

public class IceEffect : MonoBehaviour
{
    public float freezeDuration = 2f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // إذا صدم بأي كائن عنده Tag = Enemy
        if (collision.CompareTag("Enemy"))
        {
            // تحقق من نوع العدو - فقط Ping يمكن تجميده
            if (collision.gameObject.name.ToLower().Contains("ping"))
            {
                // جرب EnemyMov أولاً
                EnemyMov enemyMov = collision.GetComponent<EnemyMov>();
                if (enemyMov != null)
                {
                    StartCoroutine(FreezeEnemyMov(enemyMov));
                    Destroy(gameObject);
                    return;
                }
                
                // جرب FlyingEnemy
                FlyingEnemy flyingEnemy = collision.GetComponent<FlyingEnemy>();
                if (flyingEnemy != null)
                {
                    StartCoroutine(FreezeFlyingEnemy(flyingEnemy));
                    Destroy(gameObject);
                    return;
                }
                
                // جرب EnemyHealth كبديل
                EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    StartCoroutine(FreezeEnemyHealth(enemyHealth));
                }
            }
            else
            {
                Debug.Log($"[Freeze] Cannot freeze {collision.gameObject.name} - Only Ping enemies can be frozen");
            }

            Destroy(gameObject); // التلج يختفي بعد الاصطدام
        }
    }

    // تجميد EnemyMov
    private IEnumerator FreezeEnemyMov(EnemyMov enemy)
    {
        if (enemy == null) yield break;

        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();

        Color originalColor = Color.white;
        if (sr != null) originalColor = sr.color;

        Debug.Log($"[Freeze] Freezing EnemyMov for {freezeDuration} seconds");

        // تجميد الحركة + تغيير لون (أزرق مثلاً)
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (sr != null) sr.color = Color.cyan;

        // استدعاء دالة التجميد في EnemyMov
        enemy.Freeze(freezeDuration);

        yield return new WaitForSeconds(freezeDuration);

        // يرجع اللون طبيعي
        if (sr != null) sr.color = originalColor;
        Debug.Log("[Freeze] EnemyMov unfrozen!");
    }
    
    // تجميد FlyingEnemy
    private IEnumerator FreezeFlyingEnemy(FlyingEnemy enemy)
    {
        if (enemy == null) yield break;

        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();

        Color originalColor = Color.white;
        if (sr != null) originalColor = sr.color;

        Debug.Log($"[Freeze] Freezing FlyingEnemy for {freezeDuration} seconds");

        // تجميد الحركة + تغيير لون (أزرق مثلاً)
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (sr != null) sr.color = Color.cyan;

        // استدعاء دالة التجميد في FlyingEnemy
        enemy.Freeze(freezeDuration);

        yield return new WaitForSeconds(freezeDuration);

        // يرجع اللون طبيعي
        if (sr != null) sr.color = originalColor;
        Debug.Log("[Freeze] FlyingEnemy unfrozen!");
    }
    
    // تجميد EnemyHealth (النظام القديم)
    private IEnumerator FreezeEnemyHealth(EnemyHealth enemy)
    {
        if (enemy == null) yield break;

        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();

        Color originalColor = Color.white;
        if (sr != null) originalColor = sr.color;

        Debug.Log($"[Freeze] Freezing EnemyHealth for {freezeDuration} seconds");

        // تجميد الحركة + تغيير لون (أزرق مثلاً)
        if (rb != null) rb.linearVelocity = Vector2.zero;
        enemy.enabled = false;
        if (sr != null) sr.color = Color.cyan;

        yield return new WaitForSeconds(freezeDuration);

        // يرجع طبيعي
        enemy.enabled = true;
        if (sr != null) sr.color = originalColor;
        Debug.Log("[Freeze] EnemyHealth unfrozen!");
    }
}
