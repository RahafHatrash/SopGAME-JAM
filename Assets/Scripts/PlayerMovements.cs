using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    [Header("Ground Check Settings")]
    public Transform groundCheckPoint;   // نقطة أسفل اللاعب
    public float groundCheckRadius = 0.1f;
    public LayerMask whatIsGround;       // لير الأرض العادية فقط
    public LayerMask whatIsGroundIncludingClouds; // لير الأرض + cloud platforms (للـ Jump)

    private bool isGrounded;
    private Rigidbody2D rb;
    private float moveInput;

    [Header("Cloud Platform Layer")]
    public LayerMask cloudLayer;         // لير منصة الكلاود
    private CloudPlatform currentCloudPlatform;
    private CloudPlatform[] allCloudPlatforms;  // جميع cloud platforms في السين
    private bool hasCloudPlatforms = false;     // هل يوجد cloud platforms في السين؟

    // Crouch
    private bool isCrouching = false;
    private Vector2 crouchScale = new Vector2(1f, 0.5f);
    private Vector2 originalScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
        
        // البحث عن جميع cloud platforms في السين
        allCloudPlatforms = FindObjectsByType<CloudPlatform>(FindObjectsSortMode.None);
        hasCloudPlatforms = allCloudPlatforms.Length > 0;
        Debug.Log("Found " + allCloudPlatforms.Length + " cloud platforms in scene");
    }

    void Update()
    {
        // حركة يمين/يسار
        moveInput = Input.GetAxisRaw("Horizontal");

        // Update AudioManager with movement state for footstep sounds
        if (AudioManager.Instance != null)
        {
            bool isMoving = Mathf.Abs(moveInput) > 0.1f && isGrounded;
            AudioManager.Instance.SetPlayerMoving(isMoving);
        }

        // Jump - يعمل طبيعي في جميع السينات
        if(Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            // Play jump sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayJumpSound();
            }

            // إذا كان اللاعب فوق cloud platform: ارفع باقي cloud platforms ماعدا اللي هو واقف فوقها
            // إذا كان فوق منصة عادية: Jump يعمل طبيعي بدون تأثير على cloud platforms
            if(currentCloudPlatform != null && hasCloudPlatforms)
            {
                foreach(CloudPlatform platform in allCloudPlatforms)
                {
                    if(platform != null && platform != currentCloudPlatform)
                        platform.LiftPlatform();
                }
                Debug.Log("Raised all other cloud platforms!");
            }
        }

        // Crouch - يعمل طبيعي في جميع السينات
        if(Input.GetKeyDown(KeyCode.C))
        {
            isCrouching = true;
            transform.localScale = crouchScale;
            Debug.Log("Crouch started");

            // إذا كان اللاعب فوق cloud platform: انزل باقي cloud platforms ماعدا اللي هو واقف فوقها
            // إذا كان فوق منصة عادية: Crouch يعمل طبيعي بدون تأثير على cloud platforms
            if(currentCloudPlatform != null && hasCloudPlatforms)
            {
                foreach(CloudPlatform platform in allCloudPlatforms)
                {
                    if(platform != null && platform != currentCloudPlatform)
                        platform.DropPlatform();
                }
                Debug.Log("Lowered all other cloud platforms!");
            }
        }
        if(Input.GetKeyUp(KeyCode.C))
        {
            isCrouching = false;
            transform.localScale = originalScale;
            Debug.Log("Crouch ended");
        }
    }

    void FixedUpdate()
    {
        // Ground Check - يفحص الأرض العادية + cloud platforms
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, whatIsGroundIncludingClouds);
        
        // Debug مفصل لتشخيص المشكلة
        Debug.Log("Ground Check Debug:");
        Debug.Log("- Ground Check Point Position: " + groundCheckPoint.position);
        Debug.Log("- Ground Check Radius: " + groundCheckRadius);
        Debug.Log("- What Is Ground Including Clouds: " + whatIsGroundIncludingClouds.value);
        Debug.Log("- IsGrounded: " + isGrounded);
        
        // فحص إضافي للأرض العادية فقط
        bool isOnNormalGround = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, whatIsGround);
        Debug.Log("- Is On Normal Ground: " + isOnNormalGround);

        // الحركة الأفقية
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    // Visual Debug - لرؤية Ground Check Point في Scene View
    void OnDrawGizmos()
    {
        if(groundCheckPoint != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & cloudLayer) != 0)
        {
            currentCloudPlatform = collision.gameObject.GetComponent<CloudPlatform>();
            Debug.Log("Player entered cloud platform: " + collision.gameObject.name);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & cloudLayer) != 0)
        {
            currentCloudPlatform = null;
            Debug.Log("Player exited cloud platform: " + collision.gameObject.name);
        }
    }
}
