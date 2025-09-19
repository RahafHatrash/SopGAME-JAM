using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 10;
    
    private int currentHealth;
    private bool isDead = false;
    private PlayerController playerController;
    
    // Events for other systems to listen to
    public System.Action<int, int> OnHealthChanged; // currentHealth, maxHealth
    public System.Action OnCharacterDied;
    public System.Action<int> OnDamageTaken; // damage amount
    public System.Action<int> OnHealed; // heal amount
    
    void Start()
    {
        // Initialize health
        currentHealth = maxHealth;
        
        // Get components
        playerController = GetComponent<PlayerController>();
        
        // Notify systems of initial health
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        Debug.Log($"PlayerHealth initialized - Health: {currentHealth}/{maxHealth}"); // PlayerHealth class
    }
    
    void Update()
    {
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
        
        Debug.Log($"PLAYER DAMAGED! Took {damage} damage! Health: {currentHealth}/{maxHealth} | Remaining: {currentHealth} HP");
        
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
        
        Debug.Log($"Character collected bonus health item! Restored {bonusHealth} health! Health: {currentHealth}/{maxHealth}");
        
        // Notify systems
        OnHealed?.Invoke(bonusHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    
    
    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.Log(" PLAYER DIED! Health reached 0 - Game Over!");
        
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
        
        // Notify systems
        OnCharacterDied?.Invoke();
        
        // Player stays dead - no respawn
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
    
}
