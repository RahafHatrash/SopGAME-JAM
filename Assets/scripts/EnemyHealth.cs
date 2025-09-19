using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Hit System")]
    public int maxHits = 3;      // عدد ضربات TakeHit
    private int currentHits = 0;

    [Header("Health System")]
    public int maxHealth = 3;    // Health كامل
    private int currentHealth;

    private bool isDead = false;

    public Animator animator;    // Optional: الأنيميشن جاهز
    public Rigidbody2D rb;       // Optional: لو فيه Rigidbody
     
    void Start()
    {
        currentHealth = maxHealth;

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    // --- TakeHit: لكل ضربة بسيطة
    public void TakeHit()
    {
        if (isDead) return;

        currentHits++;
        Debug.Log("[Enemy] Hit " + currentHits + "/" + maxHits);

        if (currentHits >= maxHits)
        {
            Die();
        }
    }

    // --- TakeDamage: حسب نظام الفريق
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"Enemy took {damage} damage. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // --- Die: يدمج كل شيء
    private void Die()
    {
        if (isDead) return; // Prevent multiple calls
        isDead = true;

        Debug.Log("Enemy died!");

        // Stop movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Optional: Drop item
        DropRandomItem();

        // Optional: Death animation
        if (animator != null)
            animator.SetTrigger("Die");
        else
            Destroy(gameObject);

        // إذا فيه Coroutine
        StartCoroutine(DeathAnimation());
    }

    // --- مثال Coroutine لو فيه أنيميشن للموت
    private IEnumerator DeathAnimation()
    {
        // مدة الانيميشن
        yield return new WaitForSeconds(1f);

        Destroy(gameObject);
    }

    // --- مثال Drop item
    private void DropRandomItem()
    {
        // ضع هنا أي كود لإسقاط عنصر
        Debug.Log("Dropped an item!");
    }
}
