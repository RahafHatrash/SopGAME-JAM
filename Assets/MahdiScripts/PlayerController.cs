using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float normalJumpForce = 4f;
    public float slimeJumpForce = 2f;
    
    [Header("Animation")]
    public Animator animator;
    
    [Header("Sprite Flipping")]
    public SpriteRenderer spriteRenderer;
    
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
    private float lastMovementDirection = 0f; // 0 means no movement until player input
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Debug.Log($"[PlayerController] Starting with lastMovementDirection: {lastMovementDirection}, useMomentumMovement: {useMomentumMovement}");
        Debug.Log("[PlayerController] Death trigger detection enabled - looking for 'Die' tag");
        
        // Get Animator component if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Get SpriteRenderer component if not assigned
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
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
        
        // Set Walk animation - true when moving, false when not
        if (animator != null)
        {
            animator.SetBool("Walk", Mathf.Abs(horizontalInput) > 0.1f);
        }

        // Update AudioManager with movement state for footstep sounds
        if (AudioManager.Instance != null)
        {
            bool isMoving = Mathf.Abs(horizontalInput) > 0.1f && isGrounded;
            AudioManager.Instance.SetPlayerMoving(isMoving);
        }
        
        // Flip sprite based on movement direction
        if (spriteRenderer != null)
        {
            if (horizontalInput > 0.1f) // Moving right
            {
                spriteRenderer.flipX = true;
            }
            else if (horizontalInput < -0.1f) // Moving left
            {
                spriteRenderer.flipX = false;
            }
        }
        
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
        
        // Hit input (Left Mouse Button or X key)
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.X))
        {
            Hit();
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
        
        // No need to reset jump state - using triggers
        
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
                // Player input uses autoMovementSpeed to maintain slow speed in sewer scene
                targetVelocityX = horizontalInput * autoMovementSpeed;
                
                // Update last movement direction
                lastMovementDirection = Mathf.Sign(horizontalInput);
            }
            else
            {
                // No input - stop moving completely
                targetVelocityX = 0f;
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
        // Determine jump force based on platform type
        float currentJumpForce = normalJumpForce; // Default jump force
        
        // Check if standing on slime platform
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius);
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("SlimePlatform"))
            {
                currentJumpForce = slimeJumpForce;
                break;
            }
        }
        
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, currentJumpForce);
        
        // Trigger Jump animation when jumping
        if (animator != null)
        {
            animator.SetTrigger("Jump");
            Debug.Log($"Jump animation triggered with force: {currentJumpForce}");
        }

        // Play jump sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayJumpSound();
        }
    }
    
    void Hit()
    {
        // Trigger Hit animation
        if (animator != null)
        {
            animator.SetTrigger("isHitting");
            Debug.Log("Hit animation triggered");
        }

        // Play hit sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayHitSound();
        }
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
    
    // Public method to check if player is grounded
    public bool IsGrounded()
    {
        return isGrounded;
    }
    
    // Check for death triggers
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[PlayerController] OnTriggerEnter2D with: {other.name}, Tag: {other.tag}");
        
        // Check if player touched death trigger
        if (other.CompareTag("Die"))
        {
            Debug.Log("[PlayerController] Player touched Die trigger - going to death scene!");
            
            // Stop movement sounds before scene transition
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopMovementSounds();
            }
            
            // Destroy the player GameObject
            Debug.Log("[PlayerController] Destroying player GameObject");
            Destroy(gameObject);
            
            SceneManager.LoadScene("DieScene");
        }
    }
    
    // Also check OnCollisionEnter2D in case it's not a trigger
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"[PlayerController] OnCollisionEnter2D with: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");
        
        // Check if player touched death trigger
        if (collision.gameObject.CompareTag("Die"))
        {
            Debug.Log("[PlayerController] Player collided with Die trigger - going to death scene!");
            
            // Stop movement sounds before scene transition
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopMovementSounds();
            }
            
            // Destroy the player GameObject
            Debug.Log("[PlayerController] Destroying player GameObject");
            Destroy(gameObject);
            
            SceneManager.LoadScene("DieScene");
        }
    }
}
