using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 10;
    
    [Header("Death Settings")]
    public string deathSceneName = "DieScene";
    public float deathDelay = 0f; // No delay - go to death scene immediately
    
    [Header("Fall Death Settings")]
    public bool enableGroundedFallDeath = true; // Die when not grounded
    public float groundedFallDelay = 1f; // Delay before death when not grounded
    
    [Header("Damage Visual Effects")]
    public float damageFlashDuration = 0.3f; // How long the red flash lasts
    public Color damageFlashColor = Color.red; // Color to flash when damaged
    
    [Header("Damage Sound")]
    // Hit sound is now managed by AudioManager
    
    private int currentHealth;
    private bool isDead = false;
    private PlayerController playerController;
    private bool wasGrounded = true;
    private float notGroundedTimer = 0f;
    
    // Visual effects
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isFlashing = false;
    
    // Events for other systems to listen to
    public System.Action<int, int> OnHealthChanged; // currentHealth, maxHealth
    public System.Action OnCharacterDied;
    public System.Action<int> OnDamageTaken; // damage amount
    public System.Action<int> OnHealed; // heal amount
    
    void Start()
    {
        // Always load health from PlayerPrefs (it will be set by MainMenu or DieScene)
        currentHealth = PlayerPrefs.GetInt("PlayerHealth", maxHealth);
        currentHealth = Mathf.Min(currentHealth, maxHealth); // Don't exceed max health
        Debug.Log($"PlayerHealth initialized - Health loaded: {currentHealth}/{maxHealth}");
        
        // Get components
        playerController = GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Store original color for damage flash effect
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // Notify systems of initial health
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    void Update()
    {
        
        // Check for grounded fall death
        if (enableGroundedFallDeath && !isDead && playerController != null)
        {
            bool isGrounded = playerController.IsGrounded();
            
            if (!isGrounded && wasGrounded)
            {
                // Player just left ground, start timer
                notGroundedTimer = 0f;
                Debug.Log("[PlayerHealth] Player left ground - starting fall timer!");
            }
            else if (!isGrounded)
            {
                // Player is still not grounded, increment timer
                notGroundedTimer += Time.deltaTime;
                
                if (notGroundedTimer >= groundedFallDelay)
                {
                    Debug.Log($"[PlayerHealth] Player fell off ground for {notGroundedTimer:F1} seconds - DEATH BY FALLING!");
                    Die();
                    return;
                }
            }
            else if (isGrounded)
            {
                // Player is back on ground, reset timer
                notGroundedTimer = 0f;
            }
            
            wasGrounded = isGrounded;
        }
        
        // Debug grounded fall death every 2 seconds
        if (Time.time % 2f < 0.1f && !isDead && playerController != null)
        {
            Debug.Log($"[PlayerHealth] Player grounded: {playerController.IsGrounded()}, NotGroundedTimer: {notGroundedTimer:F1}s");
        }
        
        // Debug input to test damage (H key)
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(1);
        }
        
        // Debug input to test heal (J key)
        if (Input.GetKeyDown(KeyCode.J))
        {
            Heal(maxHealth);
        }
        
        // Debug input to test death (K key)
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(currentHealth);
        }
        
        // Debug input to test bonus health (L key)
        if (Input.GetKeyDown(KeyCode.L))
        {
            CollectBonusHealth();
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        // Apply damage
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        // Save health to PlayerPrefs
        PlayerPrefs.SetInt("PlayerHealth", currentHealth);
        PlayerPrefs.Save();
        
        Debug.Log($"PLAYER DAMAGED! Took {damage} damage! Health: {currentHealth}/{maxHealth} | Remaining: {currentHealth} HP");
        
        // Play damage sound through AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDamageSound();
        }
        
        // Start damage flash effect
        StartDamageFlash();
        
        // Notify systems
        OnDamageTaken?.Invoke(damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int healAmount)
    {
        if (isDead) return;
        
        currentHealth += healAmount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        
        // Save health to PlayerPrefs
        PlayerPrefs.SetInt("PlayerHealth", currentHealth);
        PlayerPrefs.Save();
        
        Debug.Log($"Character healed for {healAmount}! Health: {currentHealth}/{maxHealth}");
        
        // Notify systems
        OnHealed?.Invoke(healAmount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    // Method specifically for bonus items that restore 2 health
    public void CollectBonusHealth()
    {
        if (isDead) return;
        
        int bonusHealth = 2;
        currentHealth += bonusHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        
        // Save health to PlayerPrefs
        PlayerPrefs.SetInt("PlayerHealth", currentHealth);
        PlayerPrefs.Save();
        
        Debug.Log($"Character collected bonus health item! Restored {bonusHealth} health! Health: {currentHealth}/{maxHealth}");
        
        // Notify systems
        OnHealed?.Invoke(bonusHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    
    
    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.Log("💀 PLAYER DIED! Health reached 0 - Game Over! 💀");
        
        // Stop movement sounds immediately
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMovementSounds();
        }
        
        // Stop character movement
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        // Disable player controller if it exists
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Score is already saved by ScoreUIManager
        Debug.Log("[PlayerHealth] Player died - going to death scene");
        
        // Notify systems
        OnCharacterDied?.Invoke();
        
        // Destroy the player GameObject
        Debug.Log("[PlayerHealth] Destroying player GameObject");
        Destroy(gameObject);
        
        // Go to death scene immediately
        Debug.Log($"[PlayerHealth] Loading death scene immediately: {deathSceneName}");
        SceneManager.LoadScene(deathSceneName);
    }
    
    
    
    // Public getters for other systems
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;
    public float GetHealthPercentage() => (float)currentHealth / maxHealth;
    
    // Method to set max health (useful for upgrades)
    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    // Method to reset health to max (useful for checkpoints)
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log("Health reset to maximum");
    }
    
    // Method to check if character can take damage
    public bool CanTakeDamage() => !isDead;
    
    // Method to set grounded fall death settings
    public void SetGroundedFallDeathSettings(bool enabled, float delay)
    {
        enableGroundedFallDeath = enabled;
        groundedFallDelay = delay;
        Debug.Log($"[PlayerHealth] Grounded fall death settings updated - Enabled: {enabled}, Delay: {delay}s");
    }
    
    // Start the damage flash effect
    void StartDamageFlash()
    {
        if (spriteRenderer != null && !isFlashing)
        {
            StartCoroutine(DamageFlashCoroutine());
        }
    }
    
    // Coroutine for damage flash effect
    System.Collections.IEnumerator DamageFlashCoroutine()
    {
        if (spriteRenderer == null) yield break;
        
        isFlashing = true;
        
        // Flash red
        spriteRenderer.color = damageFlashColor;
        
        // Wait for flash duration
        yield return new WaitForSeconds(damageFlashDuration);
        
        // Return to original color
        spriteRenderer.color = originalColor;
        
        isFlashing = false;
    }
    
}
