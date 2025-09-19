using UnityEngine;

public class SnowPlatform : MonoBehaviour
{
    [Header("Snow Platform Settings")]
    public float forceMultiplier = 1.5f;
    public float dragMultiplier = 0.3f;
    
    [Header("Layer Detection")]
    public LayerMask playerLayer = 256; // Player layer (layer 8)
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsInLayerMask(collision.gameObject, playerLayer))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                // Enable force-based physics
                player.useForcePhysics = true;
                
                // Store original values
                player.originalMoveForce = player.moveForce;
                player.originalDrag = player.drag;
                
                // Apply snow effects - reduce force and drag
                player.moveForce *= forceMultiplier;
                player.drag *= dragMultiplier;
            }
        }
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (IsInLayerMask(collision.gameObject, playerLayer))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                // Disable force-based physics
                player.useForcePhysics = false;
                
                // Restore original values
                player.moveForce = player.originalMoveForce;
                player.drag = player.originalDrag;
            }
        }
    }
    
    private bool IsInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return (layerMask.value & (1 << obj.layer)) != 0;
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
