using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    
    [Header("Snow Platform Physics")]
    public float moveForce = 10f;
    public float maxSpeed = 5f;
    public float drag = 2f;
    public bool useForcePhysics = false;
    
    [HideInInspector]
    public float originalMoveForce = 10f;
    [HideInInspector]
    public float originalDrag = 2f;
    
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayerMask = 1;
    
    private Rigidbody2D rb;
    private bool isGrounded;
    private float horizontalInput;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Create ground check if it doesn't exist
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
        }
    }
    
    void Update()
    {
        // Get horizontal input
        horizontalInput = Input.GetAxis("Horizontal");
        
        // Jump input
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
    }
    
    void FixedUpdate()
    {
        // Check if grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);
        
        if (useForcePhysics)
        {
            // Force-based physics (for snow platforms)
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                // Apply force in the direction of input
                Vector2 force = new Vector2(horizontalInput * moveForce * Time.fixedDeltaTime, 0f);
                rb.AddForce(force, ForceMode2D.Force);
            }
            
            // Apply drag to slow down when not inputting
            if (Mathf.Abs(horizontalInput) < 0.1f)
            {
                Vector2 dragForce = -rb.linearVelocity * drag * Time.fixedDeltaTime;
                rb.AddForce(dragForce, ForceMode2D.Force);
            }
            
            // Limit maximum speed
            if (Mathf.Abs(rb.linearVelocity.x) > maxSpeed)
            {
                rb.linearVelocity = new Vector2(Mathf.Sign(rb.linearVelocity.x) * maxSpeed, rb.linearVelocity.y);
            }
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
    
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
