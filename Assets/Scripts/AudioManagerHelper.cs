using UnityEngine;

/// <summary>
/// Helper script to easily integrate AudioManager with player controllers
/// Add this component to your player GameObject to automatically play audio for player actions
/// </summary>
public class AudioManagerHelper : MonoBehaviour
{
    [Header("Audio Integration")]
    [SerializeField] private bool enableMovementAudio = true;
    [SerializeField] private bool enableActionAudio = true;
    [SerializeField] private bool enableFootstepAudio = true;

    [Header("Movement Detection")]
    [SerializeField] private float movementThreshold = 0.1f;
    [SerializeField] private float footstepCooldown = 0.3f;

    // Player components
    private PlayerController playerController;
    private PlayerMovement playerMovement;
    private Rigidbody2D playerRb;
    private Animator playerAnimator;

    // Movement tracking
    private Vector3 lastPosition;
    private float lastFootstepTime;
    private bool wasGrounded = false;
    private bool wasMoving = false;

    // Animation tracking
    private bool lastJumpTrigger = false;
    private bool lastHitTrigger = false;

    void Start()
    {
        // Get player components
        playerController = GetComponent<PlayerController>();
        playerMovement = GetComponent<PlayerMovement>();
        playerRb = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<Animator>();

        // Initialize position tracking
        lastPosition = transform.position;

        // Ensure AudioManager exists
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager not found! Please add AudioManager to your scene.");
        }
    }

    void Update()
    {
        if (AudioManager.Instance == null) return;

        // Handle movement audio
        if (enableMovementAudio)
        {
            HandleMovementAudio();
        }

        // Handle action audio based on animations
        if (enableActionAudio && playerAnimator != null)
        {
            HandleActionAudio();
        }
    }

    private void HandleMovementAudio()
    {
        // Check if player is moving
        Vector3 currentPosition = transform.position;
        float movementDistance = Vector3.Distance(currentPosition, lastPosition);
        bool isMoving = movementDistance > movementThreshold;

        // Update AudioManager with movement state
        AudioManager.Instance.SetPlayerMoving(isMoving);

        // Handle footstep audio
        if (enableFootstepAudio && isMoving)
        {
            HandleFootstepAudio();
        }

        lastPosition = currentPosition;
        wasMoving = isMoving;
    }

    private void HandleFootstepAudio()
    {
        // Only play footsteps when grounded and enough time has passed
        if (Time.time - lastFootstepTime >= footstepCooldown)
        {
            bool isGrounded = IsPlayerGrounded();
            
            if (isGrounded)
            {
                AudioManager.Instance.PlayFootstepSound();
                lastFootstepTime = Time.time;
            }
        }
    }

    private void HandleActionAudio()
    {
        if (playerAnimator == null) return;

        // Check for jump animation
        bool jumpTrigger = playerAnimator.GetBool("Jump") || 
                          playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Jump");
        
        if (jumpTrigger && !lastJumpTrigger)
        {
            AudioManager.Instance.PlayJumpSound();
        }
        lastJumpTrigger = jumpTrigger;

        // Check for hit animation
        bool hitTrigger = playerAnimator.GetBool("isHitting") || 
                         playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hit");
        
        if (hitTrigger && !lastHitTrigger)
        {
            AudioManager.Instance.PlayHitSound();
        }
        lastHitTrigger = hitTrigger;
    }

    private bool IsPlayerGrounded()
    {
        // Try to get grounded state from player controllers
        if (playerController != null)
        {
            // Use reflection to access IsGrounded method if it exists
            var isGroundedMethod = playerController.GetType().GetMethod("IsGrounded");
            if (isGroundedMethod != null)
            {
                return (bool)isGroundedMethod.Invoke(playerController, null);
            }
        }

        // Fallback: check if player has velocity and is near ground
        if (playerRb != null)
        {
            return Mathf.Abs(playerRb.linearVelocity.y) < 0.1f;
        }

        return false;
    }

    #region Public Methods for Manual Audio Triggering

    /// <summary>
    /// Call this method when the player jumps
    /// </summary>
    public void OnPlayerJump()
    {
        if (AudioManager.Instance != null && enableActionAudio)
        {
            AudioManager.Instance.PlayJumpSound();
        }
    }

    /// <summary>
    /// Call this method when the player hits/attacks
    /// </summary>
    public void OnPlayerHit()
    {
        if (AudioManager.Instance != null && enableActionAudio)
        {
            AudioManager.Instance.PlayHitSound();
        }
    }

    /// <summary>
    /// Call this method when the player shoots
    /// </summary>
    public void OnPlayerShoot()
    {
        if (AudioManager.Instance != null && enableActionAudio)
        {
            AudioManager.Instance.PlayShootSound();
        }
    }

    /// <summary>
    /// Call this method when the player dies
    /// </summary>
    public void OnPlayerDeath()
    {
        if (AudioManager.Instance != null && enableActionAudio)
        {
            AudioManager.Instance.PlayDeathSound();
        }
    }

    /// <summary>
    /// Call this method when a UI button is clicked
    /// </summary>
    public void OnButtonClick()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }
    }

    #endregion

    #region Configuration Methods

    public void SetMovementAudioEnabled(bool enabled)
    {
        enableMovementAudio = enabled;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetPlayerMoving(false);
        }
    }

    public void SetActionAudioEnabled(bool enabled)
    {
        enableActionAudio = enabled;
    }

    public void SetFootstepAudioEnabled(bool enabled)
    {
        enableFootstepAudio = enabled;
    }

    public void SetMovementThreshold(float threshold)
    {
        movementThreshold = threshold;
    }

    public void SetFootstepCooldown(float cooldown)
    {
        footstepCooldown = cooldown;
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Test Jump Audio")]
    public void TestJumpAudio()
    {
        OnPlayerJump();
    }

    [ContextMenu("Test Hit Audio")]
    public void TestHitAudio()
    {
        OnPlayerHit();
    }

    [ContextMenu("Test Shoot Audio")]
    public void TestShootAudio()
    {
        OnPlayerShoot();
    }

    [ContextMenu("Test Death Audio")]
    public void TestDeathAudio()
    {
        OnPlayerDeath();
    }

    [ContextMenu("Test Button Click Audio")]
    public void TestButtonClickAudio()
    {
        OnButtonClick();
    }

    #endregion
}
