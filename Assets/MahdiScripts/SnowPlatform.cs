using UnityEngine;


public class SnowPlatform : MonoBehaviour
{
    // Snow platform enables momentum movement for sliding effect
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Try to get PlayerController first (more reliable than layer checking)
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            // Enable momentum movement for snow platforms but keep normal speed
            player.useMomentumMovement = true;
            player.autoMovementSpeed = 5f; // Same speed as sky scene
            player.manualMovementSpeed = 5f; // Same speed as sky scene
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            // Disable momentum movement
            player.useMomentumMovement = false;
        }
    }
    
    [ContextMenu("Create Snow Platform")]
    public void CreateSnowPlatform()
    {
        // Create snow platform GameObject
        GameObject platform = new GameObject("SnowPlatform");
        
        // Add SpriteRenderer
        SpriteRenderer spriteRenderer = platform.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateSnowSprite();
        spriteRenderer.color = Color.white;
        
        // Add Collider2D
        BoxCollider2D collider = platform.AddComponent<BoxCollider2D>();
        collider.isTrigger = false; // Solid platform for collision
        
        // Add SnowPlatform script
        platform.AddComponent<SnowPlatform>();
        
        // Set platform layer
        platform.layer = LayerMask.NameToLayer("SnowPlatform");
        
        // Position platform
        platform.transform.position = new Vector3(0, 0, 0);
        platform.transform.localScale = new Vector3(3, 0.5f, 1);
        
        Debug.Log("Snow platform created!");
    }
    
    private Sprite CreateSnowSprite()
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
