using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    
    [Header("Momentum Movement")]
    public bool useMomentumMovement = false;
    [Range(0.5f, 5f)]
    public float autoMovementSpeed = 1.5f;
    [Range(1f, 10f)]
    public float manualMovementSpeed = 5f;
    [Range(0.1f, 2f)]
    public float movementTransitionSpeed = 1f;
    
    
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.5f; // Bigger radius, more like player size
    public LayerMask groundLayerMask = -1; // Check all layers
    
    private Rigidbody2D rb;
    private bool isGrounded;
    private float horizontalInput;
    private float lastMovementDirection = 1f; // 1 for right, -1 for left
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Ensure we start with normal movement
        useMomentumMovement = false;
        
        // Create ground check if it doesn't exist
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.3f, 0); // Higher position, at bottom of player
            groundCheck = groundCheckObj.transform;
        }
    }
    
    void Update()
    {
        // Get horizontal input
        horizontalInput = Input.GetAxis("Horizontal");
        
        // Jump input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                Jump();
                Debug.Log("JUMPED! Player was grounded.");
            }
            else
            {
                Debug.Log("Cannot jump - not grounded!");
            }
        }
        
        // Emergency reset key (R key)
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetToNormalPhysics();
        }
        
        // Toggle momentum movement (M key)
        if (Input.GetKeyDown(KeyCode.M))
        {
            useMomentumMovement = !useMomentumMovement;
        }
    }
    
    void FixedUpdate()
    {
        // Check if grounded using tag detection
        bool wasGrounded = isGrounded;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius);
        isGrounded = false;
        
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Ground"))
            {
                isGrounded = true;
                break;
            }
        }
        
        // Debug ground detection
        if (wasGrounded != isGrounded)
        {
            Debug.Log($"Ground state changed: {isGrounded} | Position: {groundCheck.position} | Radius: {groundCheckRadius}");
        }
        
        if (useMomentumMovement)
        {
            // Momentum-based movement - auto movement in last direction with player override
            float currentVelocityX = rb.linearVelocity.x;
            float currentVelocityY = rb.linearVelocity.y;
            
            float targetVelocityX;
            
            // Check if player is actively controlling movement
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                // Player input overrides auto movement completely
                targetVelocityX = horizontalInput * manualMovementSpeed;
                
                // Update last movement direction
                lastMovementDirection = Mathf.Sign(horizontalInput);
            }
            else
            {
                // No input - use auto movement in last direction
                targetVelocityX = lastMovementDirection * autoMovementSpeed;
            }
            
            // Smoothly transition to target velocity
            float newVelocityX = Mathf.Lerp(currentVelocityX, targetVelocityX, Time.fixedDeltaTime * (5f * movementTransitionSpeed));
            
            rb.linearVelocity = new Vector2(newVelocityX, currentVelocityY);
        }
        else
        {
            // Direct velocity movement (default)
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }
    }
    
    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }
    
    // Public method to reset physics to normal state
    public void ResetToNormalPhysics()
    {
        useMomentumMovement = false;
        Debug.Log("PlayerController: Reset to normal physics");
    }
    
    // Method to help debug ground detection
    [ContextMenu("Test Ground Detection")]
    public void TestGroundDetection()
    {
        if (groundCheck != null)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius);
            Debug.Log($"Ground check at {groundCheck.position} with radius {groundCheckRadius} found {colliders.Length} colliders:");
            foreach (var col in colliders)
            {
                bool isGround = col.CompareTag("Ground");
                Debug.Log($"- {col.name} (tag: {col.tag}) - Is Ground: {isGround}");
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            // Draw ground check circle - green if grounded, red if not
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            
            // Draw a solid circle to make it more visible
            Gizmos.color = isGrounded ? new Color(0, 1, 0, 0.3f) : new Color(1, 0, 0, 0.3f);
            Gizmos.DrawSphere(groundCheck.position, groundCheckRadius);
        }
    }
    
    void OnDrawGizmos()
    {
        // Always show ground check (not just when selected)
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
