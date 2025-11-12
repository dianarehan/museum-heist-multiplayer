using UnityEngine;

namespace prefabs.SecurityGuard.scripts
{

/// <summary>
/// First Person Movement Controller
/// Handles player movement, sprinting, crouching, and animator parameter updates
/// Requires CharacterController component
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FirstPersonMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;
    
    [Header("Jump Settings")]
    [SerializeField] private bool canJump = true;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Crouch Settings")]
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchTransitionSpeed = 10f;
    
    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;
    
    [Header("Animation")]
    [Tooltip("The animator component for the character")]
    [SerializeField] private Animator animator;
    
    [Tooltip("Use legacy parameter names (Speed, IsGrounded, etc)")]
    [SerializeField] private bool useLegacyParameters = true;
    
    [Header("Animation Parameters")]
    [SerializeField] private string moveSpeedParam = "MoveSpeed";
    [SerializeField] private string moveXParam = "MoveX";
    [SerializeField] private string moveZParam = "MoveZ";
    [SerializeField] private string isGroundedParam = "IsGrounded";
    [SerializeField] private string isWalkingParam = "IsWalking";
    [SerializeField] private string isSprintingParam = "IsSprinting";
    [SerializeField] private string isCrouchingParam = "IsCrouching";
    [SerializeField] private string jumpTriggerParam = "Jump";
    
    // Components
    private CharacterController characterController;
    
    // Movement state
    private Vector3 velocity;
    private Vector3 currentVelocity;
    private bool isGrounded;
    private bool isCrouching;
    private bool isSprinting;
    
    // Input
    private Vector2 moveInput;
    private bool jumpInput;
    private bool sprintInput;
    private bool crouchInput;
    
    private void Start()
    {
        InitializeController();
    }
    
    private void InitializeController()
    {
        // Get required components
        characterController = GetComponent<CharacterController>();
        
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("FirstPersonMovement: No Animator found. Animation triggers won't work.");
            }
        }
        
        // Set initial height
        standingHeight = characterController.height;
        
        // Validate ground mask
        if (groundMask == 0)
        {
            Debug.LogWarning("FirstPersonMovement: Ground mask not set. Using default layer.");
            groundMask = ~0; // All layers
        }
    }
    
    private void Update()
    {
        HandleInput();
        CheckGrounded();
        HandleCrouch();
        HandleMovement();
        HandleJump();
        ApplyGravity();
        UpdateAnimator();
    }
    
    private void HandleInput()
    {
        // Movement input
        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );
        
        // Jump input
        jumpInput = Input.GetButtonDown("Jump");
        
        // Sprint input (hold Shift)
        sprintInput = Input.GetKey(KeyCode.LeftShift);
        
        // Crouch input (hold Ctrl or C)
        crouchInput = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
    }
    
    private void CheckGrounded()
    {
        // Raycast down to check if grounded
        Vector3 spherePosition = transform.position - new Vector3(0, characterController.height / 2 - characterController.radius, 0);
        isGrounded = Physics.CheckSphere(
            spherePosition,
            characterController.radius + groundCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
        
        // Reset vertical velocity when grounded
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small negative value to keep grounded
        }
    }
    
    private void HandleCrouch()
    {
        if (!canCrouch) return;
        
        bool wantsToCrouch = crouchInput;
        
        // Check if can stand up (raycast above)
        if (isCrouching && !wantsToCrouch)
        {
            // Check if there's space to stand
            float heightDifference = standingHeight - crouchHeight;
            if (Physics.Raycast(transform.position, Vector3.up, heightDifference, groundMask))
            {
                // Can't stand up, something above
                wantsToCrouch = true;
            }
        }
        
        isCrouching = wantsToCrouch;
        
        // Smoothly adjust height
        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        characterController.height = Mathf.Lerp(
            characterController.height,
            targetHeight,
            Time.deltaTime * crouchTransitionSpeed
        );
        
        // Adjust center to keep feet on ground
        characterController.center = Vector3.up * (characterController.height / 2);
    }
    
    private void HandleMovement()
    {
        // Get movement direction relative to camera
        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        moveDirection.Normalize();
        
        // Determine target speed
        float targetSpeed = walkSpeed;
        
        if (isCrouching)
        {
            targetSpeed = crouchSpeed;
            isSprinting = false;
        }
        else if (sprintInput && moveInput.y > 0.1f) // Only sprint when moving forward
        {
            targetSpeed = sprintSpeed;
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }
        
        // Apply acceleration/deceleration
        Vector3 targetVelocity = moveDirection * targetSpeed;
        float speedChangeFactor = moveInput.magnitude > 0.1f ? acceleration : deceleration;
        
        currentVelocity = Vector3.Lerp(
            currentVelocity,
            targetVelocity,
            Time.deltaTime * speedChangeFactor
        );
        
        // Apply horizontal movement
        characterController.Move(currentVelocity * Time.deltaTime);
    }
    
    private void HandleJump()
    {
        if (canJump && jumpInput && isGrounded && !isCrouching)
        {
            // Calculate jump velocity
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            
            // Trigger jump animation
            if (animator != null)
            {
                animator.SetTrigger(jumpTriggerParam);
            }
        }
    }
    
    private void ApplyGravity()
    {
        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        
        // Apply vertical movement
        characterController.Move(velocity * Time.deltaTime);
    }
    
    private void UpdateAnimator()
    {
        if (animator == null) return;
        
        // Calculate movement speed (0-1 normalized)
        float speed = currentVelocity.magnitude;
        float normalizedSpeed = 0f;
        
        if (isCrouching)
        {
            normalizedSpeed = speed / crouchSpeed;
        }
        else if (isSprinting)
        {
            normalizedSpeed = speed / sprintSpeed;
        }
        else
        {
            normalizedSpeed = speed / walkSpeed;
        }
        
        // Update animator parameters
        if (HasParameter(animator, moveSpeedParam))
        {
            animator.SetFloat(moveSpeedParam, normalizedSpeed);
        }
        
        // Local movement direction for blend tree
        Vector3 localVelocity = transform.InverseTransformDirection(currentVelocity);
        
        if (HasParameter(animator, moveXParam))
        {
            animator.SetFloat(moveXParam, localVelocity.x / walkSpeed);
        }
        
        if (HasParameter(animator, moveZParam))
        {
            animator.SetFloat(moveZParam, localVelocity.z / walkSpeed);
        }
        
        // Boolean states
        if (HasParameter(animator, isGroundedParam))
        {
            animator.SetBool(isGroundedParam, isGrounded);
        }
        
        if (HasParameter(animator, isWalkingParam))
        {
            animator.SetBool(isWalkingParam, speed > 0.1f);
        }
        
        if (HasParameter(animator, isSprintingParam))
        {
            animator.SetBool(isSprintingParam, isSprinting);
        }
        
        if (HasParameter(animator, isCrouchingParam))
        {
            animator.SetBool(isCrouchingParam, isCrouching);
        }
    }
    
    /// <summary>
    /// Check if animator has a parameter
    /// </summary>
    private bool HasParameter(Animator anim, string paramName)
    {
        if (string.IsNullOrEmpty(paramName)) return false;
        
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }
    
    #region Public Methods
    
    /// <summary>
    /// Get current movement speed
    /// </summary>
    public float GetCurrentSpeed()
    {
        return currentVelocity.magnitude;
    }
    
    /// <summary>
    /// Check if player is currently moving
    /// </summary>
    public bool IsMoving()
    {
        return currentVelocity.magnitude > 0.1f;
    }
    
    /// <summary>
    /// Check if player is sprinting
    /// </summary>
    public bool IsSprinting()
    {
        return isSprinting;
    }
    
    /// <summary>
    /// Check if player is crouching
    /// </summary>
    public bool IsCrouching()
    {
        return isCrouching;
    }
    
    /// <summary>
    /// Check if player is grounded
    /// </summary>
    public bool IsGrounded()
    {
        return isGrounded;
    }
    
    /// <summary>
    /// Set walk speed
    /// </summary>
    public void SetWalkSpeed(float speed)
    {
        walkSpeed = Mathf.Max(0, speed);
    }
    
    /// <summary>
    /// Set sprint speed
    /// </summary>
    public void SetSprintSpeed(float speed)
    {
        sprintSpeed = Mathf.Max(0, speed);
    }
    
    /// <summary>
    /// Enable or disable jumping
    /// </summary>
    public void SetCanJump(bool canJump)
    {
        this.canJump = canJump;
    }
    
    /// <summary>
    /// Enable or disable crouching
    /// </summary>
    public void SetCanCrouch(bool canCrouch)
    {
        this.canCrouch = canCrouch;
    }
    
    /// <summary>
    /// Get the move input vector
    /// </summary>
    public Vector2 GetMoveInput()
    {
        return moveInput;
    }
    
    #endregion
    
    #region Debug
    
    private void OnDrawGizmosSelected()
    {
        if (characterController == null) return;
        
        // Draw ground check sphere
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 spherePosition = transform.position - new Vector3(0, characterController.height / 2 - characterController.radius, 0);
        Gizmos.DrawWireSphere(spherePosition, characterController.radius + groundCheckDistance);
        
        // Draw crouch height check
        if (isCrouching && canCrouch)
        {
            Gizmos.color = Color.yellow;
            float heightDifference = standingHeight - crouchHeight;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * heightDifference);
        }
    }
    
    #endregion
}
}