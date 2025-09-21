using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("UI References")]
    public Text healthText;
    public Slider healthBar;
    public Image healthBarFill;
    
    [Header("Health Display Settings")]
    public bool showHealthText = true;
    public bool showHealthBar = true;
    public string healthTextFormat = "Health: {0}/{1}";
    
    [Header("Health Bar Colors")]
    public Color healthyColor = Color.green;
    public Color warningColor = Color.yellow;
    public Color dangerColor = Color.red;
    public float warningThreshold = 0.5f; // 50% health
    public float dangerThreshold = 0.25f;  // 25% health
    
    private PlayerHealth playerHealth;
    
    void Start()
    {
        // Find the player health component
        FindPlayerHealth();
    }
    
    void Update()
    {
        // Keep trying to find PlayerHealth if not found
        if (playerHealth == null)
        {
            FindPlayerHealth();
        }
    }
    
    void FindPlayerHealth()
    {
        // Find the player health component
        playerHealth = FindObjectOfType<PlayerHealth>();
        
        if (playerHealth == null)
        {
            return; // Will retry in Update
        }
        
        // Subscribe to health events
        playerHealth.OnHealthChanged += UpdateHealthDisplay;
        playerHealth.OnCharacterDied += OnPlayerDied;
        
        // Initialize UI
        SetupUI();
        UpdateHealthDisplay(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());
        
        Debug.Log("HealthUI: PlayerHealth found and connected!");
    }
    
    void SetupUI()
    {
        // Setup health bar if it exists
        if (healthBar != null)
        {
            healthBar.minValue = 0;
            healthBar.maxValue = playerHealth.GetMaxHealth();
            healthBar.value = playerHealth.GetCurrentHealth();
        }
        
        // Hide UI elements if not needed
        if (healthText != null)
        {
            healthText.gameObject.SetActive(showHealthText);
        }
        
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(showHealthBar);
        }
    }
    
    void UpdateHealthDisplay(int currentHealth, int maxHealth)
    {
        // Update health text
        if (healthText != null && showHealthText)
        {
            healthText.text = string.Format(healthTextFormat, currentHealth, maxHealth);
        }
        
        // Update health bar
        if (healthBar != null && showHealthBar)
        {
            healthBar.value = currentHealth;
            healthBar.maxValue = maxHealth;
            
            // Update health bar color based on health percentage
            if (healthBarFill != null)
            {
                float healthPercentage = (float)currentHealth / maxHealth;
                
                if (healthPercentage <= dangerThreshold)
                {
                    healthBarFill.color = dangerColor;
                }
                else if (healthPercentage <= warningThreshold)
                {
                    healthBarFill.color = warningColor;
                }
                else
                {
                    healthBarFill.color = healthyColor;
                }
            }
        }
    }
    
    void OnPlayerDied()
    {
        // Update display when player dies
        if (healthText != null)
        {
            healthText.text = "DEAD";
            healthText.color = Color.red;
        }
        
        if (healthBar != null)
        {
            healthBar.value = 0;
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthDisplay;
            playerHealth.OnCharacterDied -= OnPlayerDied;
        }
    }
    
    // Method to manually update health display (useful for testing)
    [ContextMenu("Test Health Update")]
    public void TestHealthUpdate()
    {
        if (playerHealth != null)
        {
            UpdateHealthDisplay(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());
        }
    }
}
