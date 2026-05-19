using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Animator anim;
    private Controls controls;
    private Vector3 velocity;
    private Vector2 moveInput;

    [Header("Required Components")]
    private CharacterController controller;

    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    private float currentSpeed;
    private Vector3 moveDirection;

    [Header("Grounded Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;
    private bool isGrounded;
    private float groundedTimer;

    [Header("Jump Settings")]
    public float jumpHeight = 2f;
    private bool isJumping;
    private bool canJump = true; // For jump cooldown

    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool isDashing;
    private bool canDash = true;
    private float dashTimeLeft;
    private float dashCooldownLeft;

    [Header("Sprint Settings")]
    private bool isSprinting;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.5f;
            controller.center = new Vector3(0, 1f, 0);
        }

        controls = new Controls();
        controls.Player.Attack.performed += OnAttack;
        controls.Player.Jump.performed += OnJump;
        controls.Player.Dash.performed += OnDash;
        controls.Player.Sprint.performed += OnSprintStart;
        controls.Player.Sprint.canceled += OnSprintEnd;
    }

    void OnEnable()
    {
        controls.Player.Enable();
    }

    void OnDisable()
    {
        controls.Player.Disable();
    }

    void Update()
    {
        moveInput = controls.Player.Move.ReadValue<Vector2>();
        
        if (isDashing)
        {
            HandleDash();
            return;
        }

        CheckGrounded();

        if (isGrounded && velocity.y < 0)
        {
            anim.SetBool("Grounded", true);
            anim.SetBool("FreeFall", false);
            velocity.y = -2f;
            isJumping = false;
            groundedTimer = 0.1f;
        }
        else if (!isGrounded)
        {
            anim.SetBool("Grounded", false);
            if (velocity.y < 0 && !isJumping)
            {
                anim.SetBool("FreeFall", true);
            }
        }

        HandleMovement();
        HandleJump();
        HandleSprint();
        ApplyGravity();
        MovePlayer();
        UpdateAnimations();

        if (!canDash)
        {
            dashCooldownLeft -= Time.deltaTime;
            if (dashCooldownLeft <= 0) canDash = true;
        }
    }

    private void CheckGrounded()
    {
        isGrounded = controller.isGrounded;
        
        if (!isGrounded && groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }
        
        if (!isGrounded)
        {
            RaycastHit hit;
            float raycastDistance = controller.height / 2 + 0.1f;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance, groundMask))
            {
                isGrounded = true;
            }
        }
        
        if (groundedTimer > 0)
        {
            groundedTimer -= Time.deltaTime;
            if (groundedTimer > 0)
            {
                isGrounded = true;
            }
        }
    }

    // ========== ANIMATION EVENT HANDLERS ==========
    
    // Called by jump landing animation
    public void OnLand()
    {
        Debug.Log("Player landed on ground");
        anim.SetBool("Grounded", true);
        anim.SetBool("FreeFall", false);
        canJump = true;
        
        // Optional: Add landing effects
        // Play footstep sound
        // Create dust particle effect
    }
    
    // Called by jump takeoff animation
    public void OnJumpStart()
    {
        Debug.Log("Player jumped");
        canJump = false;
    }
    
    // Called by attack animation when weapon should hit
    public void OnAttackHit()
    {
        Debug.Log("Attack hit frame!");
        // Add damage detection here
    }
    
    // Called by attack animation when it's finished
    public void OnAttackEnd()
    {
        Debug.Log("Attack finished");
        // Reset attack state if needed
    }
    
    // Called by dash animation
    public void OnDashStart()
    {
        Debug.Log("Dash started");
        // Add dash effects (trail, particles, sound)
    }
    
    // Called by dash animation when finished
    public void OnDashEnd()
    {
        Debug.Log("Dash ended");
    }
    
    // Called by footstep animations
    public void OnFootstep()
    {
        // Play footstep sound
        Debug.Log("Footstep");
    }

    // ========== INPUT HANDLERS ==========

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (isGrounded && !isDashing)
        {
            anim.SetTrigger("Attack");
            Debug.Log("Attack triggered");
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (isGrounded && !isDashing && canJump)
        {
            isJumping = true;
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            anim.SetBool("Grounded", false);
            anim.SetBool("FreeFall", false);
            anim.SetTrigger("Jump");
            groundedTimer = 0;
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (isGrounded && canDash && !isDashing && moveInput != Vector2.zero)
        {
            isDashing = true;
            canDash = false;
            dashTimeLeft = dashDuration;
            dashCooldownLeft = dashCooldown;
            anim.SetTrigger("Dash");
        }
    }

    public void OnSprintStart(InputAction.CallbackContext context)
    {
        if (isGrounded && !isDashing && moveInput != Vector2.zero)
            isSprinting = true;
    }

    public void OnSprintEnd(InputAction.CallbackContext context)
    {
        isSprinting = false;
    }

    // ========== MOVEMENT METHODS ==========

    private void HandleMovement()
    {
        moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        currentSpeed = (isSprinting && !isDashing && moveInput != Vector2.zero && isGrounded) ? sprintSpeed : walkSpeed;
    }

    private void HandleJump()
    {
        if (isJumping && velocity.y <= 0)
        {
            isJumping = false;
        }
    }

    private void HandleSprint()
    {
        if (moveInput == Vector2.zero || !isGrounded) isSprinting = false;
        anim.SetBool("isSprinting", isSprinting && isGrounded && !isDashing && moveInput != Vector2.zero);
    }

    private void HandleDash()
    {
        if (dashTimeLeft > 0)
        {
            Vector3 dashDirection = moveDirection != Vector3.zero ? moveDirection : transform.forward;
            controller.Move(dashDirection * dashSpeed * Time.deltaTime);
            dashTimeLeft -= Time.deltaTime;
        }
        else
        {
            isDashing = false;
        }
    }

    private void ApplyGravity()
    {
        if (!isDashing && !controller.isGrounded)
            velocity.y += Physics.gravity.y * Time.deltaTime;
    }

    private void MovePlayer()
    {
        if (!isDashing)
        {
            Vector3 move = moveDirection * currentSpeed;
            move.y = velocity.y;
            controller.Move(move * Time.deltaTime);
        }
        
        if (moveDirection != Vector3.zero && !isDashing)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    private void UpdateAnimations()
    {
        float speedPercent = 0;
        if (!isDashing && moveInput != Vector2.zero)
            speedPercent = (isSprinting && isGrounded) ? 1f : 0.5f;
        
        anim.SetFloat("Speed", speedPercent);
        anim.SetFloat("MotionSpeed", isSprinting ? 1.5f : 1f);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
        
        if (controller != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, Vector3.down * (controller.height/2 + 0.1f));
        }
    }
}