using UnityEngine;

public class SlimePlatform : MonoBehaviour
{
    [Header("Slime Platform Settings")]
    public float speedReduction = 0.3f;
    public float jumpReduction = 0.7f;
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            // Apply slime effects - reduce speed only
            // Jump force is now handled automatically in PlayerController based on platform type
            player.moveSpeed *= speedReduction;
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            // Restore original values
            player.moveSpeed = 5f; // Restore default moveSpeed
            // Jump force is now handled automatically in PlayerController based on platform type
        }
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
