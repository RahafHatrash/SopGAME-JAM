using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Hit System")]
    public int maxHits = 2;      // عدد ضربات TakeHit (طلقتين ليموت)
    private int currentHits = 0;

    [Header("Health System")]
    public int maxHealth = 3;    // Health كامل
    private int currentHealth;

    [Header("Death Animation")]
    public float deathAnimationDuration = 1f;
    public AnimationCurve deathScaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); // يقلص من 1 إلى 0
    public AnimationCurve deathRotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 360); // يدور 360 درجة

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

        // Start death animation (يقلص ويدور)
        StartCoroutine(DeathAnimation());
    }

    // --- أنيميشن الموت (يقلص ويدور)
    private IEnumerator DeathAnimation()
    {
        float elapsedTime = 0f;
        Vector3 originalScale = transform.localScale;
        float originalRotation = transform.rotation.eulerAngles.z;
        
        while (elapsedTime < deathAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / deathAnimationDuration;
            
            // Apply scale animation (يقلص)
            float scaleMultiplier = deathScaleCurve.Evaluate(progress);
            transform.localScale = originalScale * scaleMultiplier;
            
            // Apply rotation animation (يدور)
            float rotationAmount = deathRotationCurve.Evaluate(progress);
            transform.rotation = Quaternion.Euler(0, 0, originalRotation + rotationAmount);
            
            yield return null;
        }
        
        // Destroy the enemy after animation
        Destroy(gameObject);
    }

    // --- مثال Drop item
    private void DropRandomItem()
    {
        // ضع هنا أي كود لإسقاط عنصر
        Debug.Log("Dropped an item!");
    }
}
