using UnityEngine;

public class SlimePlatform : MonoBehaviour
{
    [Header("Slime Platform Settings")]
    public float speedReduction = 0.3f;
    public float jumpReduction = 0.7f;
    
    [Header("Layer Detection")]
    public LayerMask playerLayer = 256; // Player layer (layer 8)
    
    private PlayerController playerController;
    private float originalMoveSpeed;
    private float originalJumpForce;
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsInLayerMask(collision.gameObject, playerLayer))
        {
            playerController = collision.gameObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // Store original values
                originalMoveSpeed = playerController.moveSpeed;
                originalJumpForce = playerController.jumpForce;
                
                // Apply slime effects - reduce speed and jump
                playerController.moveSpeed *= speedReduction;
                playerController.jumpForce *= jumpReduction;
            }
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (IsInLayerMask(collision.gameObject, playerLayer) && playerController != null)
        {
            // Restore original values
            playerController.moveSpeed = originalMoveSpeed;
            playerController.jumpForce = originalJumpForce;
        }
    }
    
    private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return (layerMask.value & (1 << obj.layer)) != 0;
    }
    
    [ContextMenu("Create Slime Platform")]
    public void CreateSlimePlatform()
    {
        // Create slime platform GameObject
        GameObject platform = new GameObject("SlimePlatform");
        
        // Add SpriteRenderer
        SpriteRenderer spriteRenderer = platform.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateSlimeSprite();
        spriteRenderer.color = Color.green;
        
        // Add Collider2D
        BoxCollider2D collider = platform.AddComponent<BoxCollider2D>();
        collider.isTrigger = false; // Solid platform for collision
        
        // Add SlimePlatform script
        platform.AddComponent<SlimePlatform>();
        
        // Set platform layer
        platform.layer = LayerMask.NameToLayer("SlimePlatform");
        
        // Position platform
        platform.transform.position = new Vector3(0, 0, 0);
        platform.transform.localScale = new Vector3(3, 0.5f, 1);
        
        Debug.Log("Slime platform created!");
    }
    
    private Sprite CreateSlimeSprite()
    {
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }
}
