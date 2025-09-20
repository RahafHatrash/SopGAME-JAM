using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float flightHeight = 2f; // Height above ground to fly at
    public float screenEdgeBuffer = 1f; // Distance from screen edge to destroy enemy
    
    [Header("Health Settings")]
    public int maxHealth = 3;  // يحتاج 3 طلقات للموت
    private int currentHealth;
    
    [Header("Death Animation")]
    public float deathAnimationDuration = 1f;
    public AnimationCurve deathScaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public AnimationCurve deathRotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 360);
    
    [Header("Damage Settings")]
    public int damageToPlayer = 1;
    
    [Header("Drop Settings")]
    public int baseXP = 5;                // XP always dropped when killed
    
    [Header("Components")]
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private ItemDropper itemDropper;
    private bool movingRight = true;
    private bool isDead = false;
    public bool isFrozen = false;
    private Color originalColor; // Store original color
    private Vector3 startPosition;
    
    void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        itemDropper = GetComponentInChildren<ItemDropper>();
        currentHealth = maxHealth;
        startPosition = transform.position;
        
        // Set up flying physics
        if (rb != null)
        {
            rb.gravityScale = 0f; // No gravity for flying enemy
            rb.freezeRotation = true; // Prevent rotation from physics
        }
        
        // Set up collider as trigger so it doesn't collide with platforms
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true; // Make it a trigger so it passes through platforms
        }
        
        // Configure collision layers to ignore other enemies
        ConfigureEnemyCollisions();
    }
    
    void ConfigureEnemyCollisions()
    {
        // Find all enemies and ignore collisions between them
        FlyingEnemy[] allFlyingEnemies = FindObjectsByType<FlyingEnemy>(FindObjectsSortMode.None);
        EnemyMov[] allGroundEnemies = FindObjectsByType<EnemyMov>(FindObjectsSortMode.None);
        Collider2D thisCollider = GetComponent<Collider2D>();
        
        // Ignore collisions with other flying enemies
        foreach (FlyingEnemy flyingEnemy in allFlyingEnemies)
        {
            if (flyingEnemy != this && flyingEnemy.gameObject != this.gameObject)
            {
                Collider2D otherCollider = flyingEnemy.GetComponent<Collider2D>();
                if (otherCollider != null && thisCollider != null)
                {
                    Physics2D.IgnoreCollision(thisCollider, otherCollider, true);
                }
            }
        }
        
        // Ignore collisions with ground enemies
        foreach (EnemyMov groundEnemy in allGroundEnemies)
        {
            Collider2D otherCollider = groundEnemy.GetComponent<Collider2D>();
            if (otherCollider != null && thisCollider != null)
            {
                Physics2D.IgnoreCollision(thisCollider, otherCollider, true);
            }
        }
    }
    
    void Update()
    {
        // Don't update if dead or frozen
        if (isDead || isFrozen) return;
        
        // Check if enemy reached screen edge
        CheckScreenEdges();
        
        // Move the flying enemy
        Move();
        
        // Flip sprite based on direction
        FlipSprite();
    }
    
    void Move()
    {
        // Apply horizontal movement only (flying enemy)
        float direction = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * moveSpeed, 0f);
        Debug.Log($"[FlyingEnemy] Moving: direction={direction}, speed={moveSpeed}, velocity={rb.linearVelocity}");
    }
    
    void CheckScreenEdges()
    {
        // Get screen bounds
        Camera cam = Camera.main;
        if (cam == null) return;
        
        Vector2 screenBounds = cam.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        Vector2 screenMin = cam.ScreenToWorldPoint(Vector2.zero);
        
        // Check if enemy is outside screen bounds (with buffer)
        if (transform.position.x < screenMin.x - screenEdgeBuffer || 
            transform.position.x > screenBounds.x + screenEdgeBuffer)
        {
            // Destroy enemy when it reaches screen edge
            Debug.Log("Flying enemy reached screen edge and was destroyed!");
            Destroy(gameObject);
        }
    }
    
    
    void FlipSprite()
    {
        // Flip sprite based on movement direction
        spriteRenderer.flipX = !movingRight;
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead) return; // Don't take damage if already dead
        
        Debug.Log($"[DEBUG] FlyingEnemy {gameObject.name} - Before damage: {currentHealth}/{maxHealth}");
        currentHealth -= damage;
        Debug.Log($"[DEBUG] FlyingEnemy {gameObject.name} - After {damage} damage: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Debug.Log($"[DEBUG] FlyingEnemy {gameObject.name} - DIED! Health: {currentHealth}");
            Die();
        }
    }
    
    void Die()
    {
        if (isDead) return; // Prevent multiple death calls
        
        isDead = true;
        Debug.Log("Enemy died!");
        
        // Stop movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        // Drop random item
        DropRandomItem();
        
        // Start death animation
        StartCoroutine(DeathAnimation());
    }
    
    System.Collections.IEnumerator DeathAnimation()
    {
        float elapsedTime = 0f;
        Vector3 originalScale = transform.localScale;
        float originalRotation = transform.rotation.eulerAngles.z;
        Color originalColor = spriteRenderer.color;
        
        while (elapsedTime < deathAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / deathAnimationDuration;
            
            // Apply scale animation
            float scaleMultiplier = deathScaleCurve.Evaluate(progress);
            transform.localScale = originalScale * scaleMultiplier;
            
            // Apply rotation animation
            float rotationAmount = deathRotationCurve.Evaluate(progress);
            transform.rotation = Quaternion.Euler(0, 0, originalRotation + rotationAmount);
            
            // Apply red flash effect (أحمر خفيف يختفي تدريجياً)
            float redFlashIntensity = Mathf.Lerp(1f, 0f, progress); // من 1 إلى 0
            Color flashColor = Color.Lerp(originalColor, Color.red, redFlashIntensity * 0.3f); // 30% أحمر
            spriteRenderer.color = flashColor;
            
            yield return null;
        }
        
        // Destroy the enemy after animation
        Destroy(gameObject);
    }
    
    void DropRandomItem()
    {
        // Always give base XP
        GiveXP(baseXP);
        Debug.Log($"Enemy dropped {baseXP} XP!");
        
        // Drop bonus item using child component
        if (itemDropper != null)
        {
            itemDropper.DropBonusItem();
        }
    }
    
    
    
    void GiveXP(int xpAmount)
    {
        // Add XP to score system
        ScoreUIManager scoreUI = FindFirstObjectByType<ScoreUIManager>();
        if (scoreUI != null)
        {
            scoreUI.AddScore(xpAmount);
        }
        else
        {
            Debug.LogWarning("[FlyingEnemy] ScoreUIManager not found!");
        }
    }
    
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignore other enemies
        if (collision.gameObject.CompareTag("Enemy") || 
            collision.gameObject.GetComponent<EnemyMov>() != null ||
            collision.gameObject.GetComponent<FlyingEnemy>() != null)
        {
            return; // Don't interact with other enemies
        }
        
        // Only interact with player if not frozen
        if (collision.gameObject.CompareTag("Player") && !isFrozen)
        {
            // Deal damage to player
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageToPlayer);
                Debug.Log("Flying enemy hit player for " + damageToPlayer + " damage!");
            }
            
            // Switch direction when hitting player
            movingRight = !movingRight;
            Debug.Log("Flying enemy switched direction after hitting player!");
        }
        else if (collision.gameObject.CompareTag("Player") && isFrozen)
        {
            Debug.Log("Frozen flying enemy cannot damage player!");
        }
    }
    
    // Visualize flight path in editor
    void OnDrawGizmosSelected()
    {
        // Draw flight path
        Gizmos.color = Color.blue;
        Vector3 start = new Vector3(transform.position.x - 5f, transform.position.y, 0);
        Vector3 end = new Vector3(transform.position.x + 5f, transform.position.y, 0);
        Gizmos.DrawLine(start, end);
        
        // Draw current position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
    
    // Freeze function for ice ability
    public void Freeze(float duration)
    {
        if (isDead) return;
        
        // FlyingEnemy doesn't freeze - just ignore freeze hits
        Debug.Log("FlyingEnemy ignores freeze hits - only ground enemies freeze!");
    }
    
    private System.Collections.IEnumerator UnfreezeAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        isFrozen = false;
        
        // Restore original color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
            Debug.Log("FlyingEnemy unfrozen! Color restored to: " + originalColor);
        }
        else
        {
            Debug.Log("FlyingEnemy unfrozen!");
        }
    }
}

