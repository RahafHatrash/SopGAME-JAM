using UnityEngine;
using System.Collections;

public class IceEffect : MonoBehaviour
{
    public float freezeDuration = 2f;
    public LayerMask freezeTargetLayer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((freezeTargetLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                StartCoroutine(FreezeEnemy(enemy));
            }

            Destroy(gameObject);
        }
    }

    private IEnumerator FreezeEnemy(EnemyHealth enemy)
    {
        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();

        Color originalColor = Color.white;
        if (sr != null) originalColor = sr.color;

        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (sr != null) sr.color = Color.cyan;

        enemy.enabled = false;

        yield return new WaitForSeconds(freezeDuration);

        enemy.enabled = true;
        if (sr != null) sr.color = originalColor;
    }
}
