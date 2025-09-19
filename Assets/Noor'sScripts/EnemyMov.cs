using UnityEngine;

public class EnemyMov : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float platformCheckDistance = 0.5f;
    
    [Header("Health Settings")]
    public int maxHealth = 2;
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
    
    // Ground check visualization variables
    private bool isGroundedAhead = false;
    private Vector2 groundCheckPosition;
    
    [Header("Ground Detection")]
    public string groundTag = "Ground"; // Tag to check for ground objects
    
    [Header("Collision Settings")]
    public string playerTag = "Player"; // Tag for player objects
    public string enemyTag = "Enemy"; // Tag for enemy objects
    
    void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        itemDropper = GetComponentInChildren<ItemDropper>();
        currentHealth = maxHealth;
        
        // Configure collision layers to ignore other enemies
        ConfigureCollisionLayers();
    }
    
    void ConfigureCollisionLayers()
    {
        // Find all enemies and ignore collisions between them
        EnemyMov[] allEnemies = FindObjectsByType<EnemyMov>(FindObjectsSortMode.None);
        Collider2D thisCollider = GetComponent<Collider2D>();
        
        foreach (EnemyMov enemy in allEnemies)
        {
            if (enemy != this && enemy.gameObject != this.gameObject)
            {
                Collider2D otherCollider = enemy.GetComponent<Collider2D>();
                if (otherCollider != null && thisCollider != null)
                {
                    Physics2D.IgnoreCollision(thisCollider, otherCollider, true);
                }
            }
        }
        
        // Also ignore collisions with flying enemies
        FlyingEnemy[] allFlyingEnemies = FindObjectsByType<FlyingEnemy>(FindObjectsSortMode.None);
        foreach (FlyingEnemy flyingEnemy in allFlyingEnemies)
        {
            Collider2D otherCollider = flyingEnemy.GetComponent<Collider2D>();
            if (otherCollider != null && thisCollider != null)
            {
                Physics2D.IgnoreCollision(thisCollider, otherCollider, true);
            }
        }
    }
    
    void Update()
    {
        // Don't update if dead
        if (isDead) return;
        
        // Check if we should turn around
        CheckForTurnAround();
        
        // Move the enemy
        Move();
        
        // Flip sprite based on direction
        FlipSprite();
    }
    
    void Move()
    {
        // Apply horizontal movement
        float direction = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }
    
    void CheckForTurnAround()
    {
        // Get the enemy's collider to find the bottom
        Collider2D enemyCollider = GetComponent<Collider2D>();
        if (enemyCollider == null) return;
        
        // Calculate the bottom of the enemy's collider
        float bottomOfCollider = enemyCollider.bounds.min.y;
        
        // Check if there's ground ahead using the bottom of the collider
        Vector2 groundCheckPos = new Vector2(
            transform.position.x + (movingRight ? platformCheckDistance : -platformCheckDistance),
            bottomOfCollider - 0.1f // Check slightly below the bottom of the collider
        );
        
        // Check for ground using raycast
        RaycastHit2D hit = Physics2D.Raycast(groundCheckPos, Vector2.down, 0.5f);
        bool hasGroundAhead = hit.collider != null && hit.collider.CompareTag(groundTag);
        
        // Store ground check result for gizmo visualization
        isGroundedAhead = hasGroundAhead;
        groundCheckPosition = groundCheckPos;
        
        // Turn around if no ground ahead
        if (!hasGroundAhead)
        {
            movingRight = !movingRight;
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
        
        currentHealth -= damage;
        Debug.Log($"Enemy took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
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
        // Automatically add XP to score system
        // You can integrate with your score system here
        Debug.Log($"Added {xpAmount} XP to score system!");
        
        // Example integration with a score manager:
        // ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        // if (scoreManager != null)
        // {
        //     scoreManager.AddXP(xpAmount);
        // }
        
        // Or if you have a static score system:
        // ScoreSystem.AddXP(xpAmount);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if colliding with another enemy (multiple ways to detect)
        if (collision.gameObject.CompareTag(enemyTag) || 
            collision.gameObject.GetComponent<EnemyMov>() != null ||
            collision.gameObject.GetComponent<FlyingEnemy>() != null)
        {
            // Ignore collision with other enemies - make them pass through each other
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.collider, true);
            Debug.Log($"Enemy {gameObject.name} ignoring collision with {collision.gameObject.name}");
            return;
        }
        
        // Only interact with player
        if (collision.gameObject.CompareTag(playerTag))
        {
            // Deal damage to player
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageToPlayer);
                Debug.Log("Enemy hit player for " + damageToPlayer + " damage!");
            }
            
            // Switch direction when hitting player
            movingRight = !movingRight;
            Debug.Log("Enemy switched direction after hitting player!");
        }
    }
    
    // Visualize detection areas in editor
    void OnDrawGizmosSelected()
    {
        // Get the enemy's collider to find the bottom
        Collider2D enemyCollider = GetComponent<Collider2D>();
        if (enemyCollider == null) return;
        
        // Calculate the bottom of the enemy's collider
        float bottomOfCollider = enemyCollider.bounds.min.y;
        
        // Ground check position using bottom of collider
        Vector2 groundCheckPos = new Vector2(
            transform.position.x + (movingRight ? platformCheckDistance : -platformCheckDistance),
            bottomOfCollider - 0.1f
        );
        
        // Use real-time ground detection result if available, otherwise use default color
        bool isGrounded = Application.isPlaying ? isGroundedAhead : false;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheckPos, 0.1f);
        
        // Draw raycast line
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(groundCheckPos, groundCheckPos + Vector2.down * 0.5f);
        
        // Draw line to show ground check position
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, groundCheckPos);
        
        // Draw the bottom of the collider for reference
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(new Vector2(transform.position.x - 0.5f, bottomOfCollider), 
                       new Vector2(transform.position.x + 0.5f, bottomOfCollider));
    }
    
    // Always show ground check (not just when selected)
    void OnDrawGizmos()
    {
        // Get the enemy's collider to find the bottom
        Collider2D enemyCollider = GetComponent<Collider2D>();
        if (enemyCollider == null) return;
        
        // Calculate the bottom of the enemy's collider
        float bottomOfCollider = enemyCollider.bounds.min.y;
        
        Vector2 groundCheckPos = new Vector2(
            transform.position.x + (movingRight ? platformCheckDistance : -platformCheckDistance),
            bottomOfCollider - 0.1f
        );
        
        // Use real-time ground detection result if available, otherwise use default color
        bool isGrounded = Application.isPlaying ? isGroundedAhead : false;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheckPos, 0.1f);
    }
}
