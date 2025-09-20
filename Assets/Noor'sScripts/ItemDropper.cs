using UnityEngine;

public class ItemDropper : MonoBehaviour
{
    [Header("Bonus Item Sprites")]
    public Sprite doubleXPSprite;         // Sprite for double XP drop
    public Sprite healthPackSprite;       // Sprite for health pack drop
    
    [Header("Bonus Drop Settings")]
    [Range(0f, 1f)]
    public float bonusDropChance = 0.3f;  // 30% chance to drop bonus item
    [Range(0f, 1f)]
    public float xpDropChance = 0.6f;     // 60% chance for double XP, 40% for health pack
    public int bonusXPAmount = 10;        // Amount of XP double XP pickup gives
    public int healAmount = 1;            // Amount of health health pack restores
    
    public void DropBonusItem()
    {
        // Check if we should drop bonus item
        if (Random.Range(0f, 1f) <= bonusDropChance)
        {
            // Determine what bonus to drop
            if (Random.Range(0f, 1f) <= xpDropChance)
            {
                // Drop double XP
                CreateBonusItem("DoubleXP", doubleXPSprite, true);
                Debug.Log("Enemy also dropped double XP!");
            }
            else
            {
                // Drop health pack
                CreateBonusItem("HealthPack", healthPackSprite, false);
                Debug.Log("Enemy also dropped health pack!");
            }
        }
    }
    
    void CreateBonusItem(string itemType, Sprite itemSprite, bool isXP)
    {
        // Create a new GameObject
        GameObject bonusItem = new GameObject(itemType);
        bonusItem.transform.position = transform.position;
        
        // Add SpriteRenderer
        SpriteRenderer itemRenderer = bonusItem.AddComponent<SpriteRenderer>();
        itemRenderer.sprite = itemSprite;
        itemRenderer.sortingOrder = 1; // Make sure it appears above the enemy
        
        // Add Collider2D as trigger
        BoxCollider2D itemCollider = bonusItem.AddComponent<BoxCollider2D>();
        itemCollider.isTrigger = true;
        itemCollider.size = new Vector2(0.5f, 0.5f);
        
        // Add pickup behavior
        ItemPickup pickup = bonusItem.AddComponent<ItemPickup>();
        pickup.isXP = isXP;
        pickup.xpAmount = bonusXPAmount;
        pickup.healAmount = healAmount;
    }
}

// Pickup behavior for bonus items
public class ItemPickup : MonoBehaviour
{
    public bool isXP = true;
    public int xpAmount = 10;
    public int healAmount = 1;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Play pickup sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPickupSound();
            }

            if (isXP)
            {
                // Add XP to score system
                Debug.Log($"Player picked up double XP! Gained {xpAmount} XP!");
                
                ScoreUIManager scoreUI = FindFirstObjectByType<ScoreUIManager>();
                if (scoreUI != null)
                {
                    scoreUI.AddScore(xpAmount);
                }
                else
                {
                    Debug.LogWarning("[ItemPickup] ScoreUIManager not found!");
                }
            }
            else
            {
                // Heal the player with bonus health
                PlayerHealth player = other.GetComponent<PlayerHealth>();
                if (player != null)
                {
                    // Use bonus health method for 2 health restoration
                    player.CollectBonusHealth();
                }
            }
            
            // Destroy the pickup
            Destroy(gameObject);
        }
    }
}
