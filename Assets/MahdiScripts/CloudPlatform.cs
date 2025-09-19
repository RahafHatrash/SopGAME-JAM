using UnityEngine;

public class CloudPlatform : MonoBehaviour
{
    [Header("Cloud Platform Settings")]
    public float liftAmount = 5f;
    public float dropAmount = 5f;
    public float moveSpeed = 2f;
    
    
    private Rigidbody2D platformRb;
    private Vector3 currentPosition;
    private bool playerOnPlatform = false;
    private PlayerController playerController;
    
    void Start()
    {
        platformRb = GetComponent<Rigidbody2D>();
        if (platformRb == null)
        {
            platformRb = gameObject.AddComponent<Rigidbody2D>();
            platformRb.bodyType = RigidbodyType2D.Kinematic;
        }
        currentPosition = transform.position;
    }
    
    void Update()
    {
        if (playerOnPlatform && playerController != null)
        {
            // Jump button lifts the platform
            if (Input.GetKeyDown(KeyCode.Space))
            {
                LiftAllCloudPlatforms();
            }
            
            // Crouch button drops the platform
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                DropAllCloudPlatforms();
            }
        }
        
    }
    
    void LiftAllCloudPlatforms()
    {
        // Find all cloud platforms in the scene
        CloudPlatform[] allCloudPlatforms = FindObjectsByType<CloudPlatform>(FindObjectsSortMode.None);
        
        foreach (CloudPlatform cloudPlatform in allCloudPlatforms)
        {
            // Skip the platform the player is standing on
            if (cloudPlatform != this)
            {
                cloudPlatform.LiftPlatform();
            }
        }
        
    }
    
    void DropAllCloudPlatforms()
    {
        // Find all cloud platforms in the scene
        CloudPlatform[] allCloudPlatforms = FindObjectsByType<CloudPlatform>(FindObjectsSortMode.None);
        
        foreach (CloudPlatform cloudPlatform in allCloudPlatforms)
        {
            // Skip the platform the player is standing on
            if (cloudPlatform != this)
            {
                cloudPlatform.DropPlatform();
            }
        }
        
    }
    
    void LiftPlatform()
    {
        // Stop any current movement
        StopAllCoroutines();
        platformRb.linearVelocity = Vector2.zero;
        
        currentPosition = transform.position;
        Vector3 targetPosition = currentPosition + Vector3.up * liftAmount;
        StartCoroutine(MoveToPosition(targetPosition));
    }
    
    void DropPlatform()
    {
        // Stop any current movement
        StopAllCoroutines();
        platformRb.linearVelocity = Vector2.zero;
        
        currentPosition = transform.position;
        Vector3 targetPosition = currentPosition - Vector3.up * dropAmount;
        StartCoroutine(MoveToPosition(targetPosition));
    }
    
    System.Collections.IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            platformRb.linearVelocity = direction * moveSpeed;
            yield return null;
        }
        
        // Reached target position
        platformRb.linearVelocity = Vector2.zero;
        transform.position = targetPosition;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            playerOnPlatform = true;
            playerController = player;
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            playerOnPlatform = false;
            playerController = null;
        }
    }
    
    
    [ContextMenu("Create Cloud Platform")]
    public void CreateCloudPlatform()
    {
        // Create cloud platform GameObject
        GameObject platform = new GameObject("CloudPlatform");
        
        // Add SpriteRenderer
        SpriteRenderer spriteRenderer = platform.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateCloudSprite();
        spriteRenderer.color = Color.cyan;
        
        // Add solid Collider2D for collision
        BoxCollider2D solidCollider = platform.AddComponent<BoxCollider2D>();
        solidCollider.isTrigger = false; // Solid platform for collision
        
        // Add trigger Collider2D for detection
        BoxCollider2D triggerCollider = platform.AddComponent<BoxCollider2D>();
        triggerCollider.isTrigger = true; // Trigger for detection
        triggerCollider.size = new Vector2(3.2f, 0.7f); // Slightly larger than solid collider
        
        // Add CloudPlatform script
        platform.AddComponent<CloudPlatform>();
        
        // Set platform layer
        platform.layer = LayerMask.NameToLayer("CloudPlatform");
        
        // Position platform
        platform.transform.position = new Vector3(0, 0, 0);
        platform.transform.localScale = new Vector3(3, 0.5f, 1);
        
    }
    
    private Sprite CreateCloudSprite()
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
