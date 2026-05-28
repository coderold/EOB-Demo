using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    public Animator anim;
    private Vector3 _velocity;
    private Vector2 _moveInput;

    [Header("Required Components")] 
    private CharacterController _controller;
    private MeleeAttack _meleeAttack;
    [SerializeField] private PlayerInput playerInput;
    private CinemachineCamera _localVCam;
    
    [Header("Attack Settings")]
    private int _comboStep = 0;
    private float _lastAttackTime;
    public float comboResetTime = 1.5f;

    [Header("Movement Settings")]
    private bool _isWalking;
    private const float _walkSpeed = 2.5f;
    private const float _sprintSpeed = 4f;
    private float _currentSpeed;
    private Vector3 _moveDirection;

    [Header("Grounded Check")] public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;
    private bool _isGrounded;
    private float _groundedTimer;

    [Header("Jump Settings")] public float jumpHeight = 2f;
    private bool _isJumping;
    private bool _canJump = true; 
    
    
    [Header("Dash Settings")] 
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool _isDashing;
    private bool _canDash = true;
    private float _dashTimeLeft;
    private float _dashCooldownLeft;

    [Header("Sprint Settings")] 
    private bool _isSprinting;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _meleeAttack = GetComponent<MeleeAttack>();
        
        if (_controller == null)
        {
            _controller = gameObject.AddComponent<CharacterController>();
            _controller.height = 2f;
            _controller.radius = 0.5f;
        }
        
    }
    
    public override void OnNetworkSpawn()
    {
        _localVCam = GetComponentInChildren<CinemachineCamera>();
        
        if (IsOwner)
        {
            if (_localVCam != null)
            {
                _localVCam.Target.TrackingTarget = transform;
                _localVCam.Follow = transform;
                _localVCam.LookAt = transform;

          
                _localVCam.Priority = 10; 
            }
        }
        else
        {
          
            if (_localVCam != null)
            {
                _localVCam.gameObject.SetActive(false);
            }
            
            enabled = false;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (_isDashing)
        {
            HandleDash();
            return;
        }

        CheckGrounded();

        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
            _isJumping = false;
            _groundedTimer = 0.1f;
        }

        HandleMovement();
        HandleJump();
        HandleSprint();
        ApplyGravity();
        MovePlayer();

        if (!_canDash)
        {
            _dashCooldownLeft -= Time.deltaTime;
            if (_dashCooldownLeft <= 0) _canDash = true;
        }
    }

    private void CheckGrounded()
    {
        _isGrounded = _controller.isGrounded;

        if (!_isGrounded && groundCheck)
            _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (!_isGrounded)
        {
            RaycastHit hit;
            var raycastDistance = _controller.height / 2 + 0.1f;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance, groundMask))
                _isGrounded = true;
        }

        if (_groundedTimer > 0)
        {
            _groundedTimer -= Time.deltaTime;
            if (_groundedTimer > 0) _isGrounded = true;
        }
    }
    

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

  
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed && _isGrounded && !_isDashing)
        {
            if (Time.time - _lastAttackTime > comboResetTime)
            {
                _comboStep = 0;
            }
            
            anim.SetInteger("AttackIndex", _comboStep);
            anim.SetTrigger("Attack");

      
            _lastAttackTime = Time.time;
            
        }
    }


    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && _isGrounded && !_isDashing && _canJump)
        {
            _isJumping = true;
            _velocity.y = Mathf.Sqrt(jumpHeight * -3f * Physics.gravity.y);
            anim.SetTrigger("Jump");
            _groundedTimer = 0;
        }
    }


    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed && _isGrounded && _canDash && !_isDashing)
        {
            _isDashing = true;
            _canDash = false;
            _dashTimeLeft = dashDuration;
            _dashCooldownLeft = dashCooldown;
            anim.SetTrigger("Dash");
        }
    }


    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed) _isSprinting = true;
        else if (context.canceled) _isSprinting = false;
    }



    public void OnLand()
    {
        _canJump = true;
    }
    

    public void OnAttackHit()
    {
    
        if (_meleeAttack != null)
        {
            _meleeAttack.PerformHitDetection();
        }
    }

    public void OnAttackEnd()
    {
        _comboStep++;
        
        if (_comboStep > 1)
        {
            _comboStep = 0;
        }
        
    }
    

    private void HandleMovement()
    {
        _moveDirection = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;

        bool isMovingOnGround = _moveInput != Vector2.zero && _isGrounded && !_isDashing;
        bool isActuallySprinting = _isSprinting && isMovingOnGround;

        _currentSpeed = isActuallySprinting ? _sprintSpeed : _walkSpeed;


        float animationSpeedParam = isMovingOnGround ? _currentSpeed : 0f;
    
        anim.SetFloat("Speed", animationSpeedParam);
    }

    private void HandleJump()
    {
        if (_isJumping && _velocity.y <= 0) _isJumping = false;
    }

    private void HandleSprint()
    {
        if (_moveInput == Vector2.zero) 
        {
            _isSprinting = false;
        }
    }

    private void HandleDash()
    {
        if (_dashTimeLeft > 0)
        {
            Vector3 dashDirection = transform.forward;
            
            _controller.Move(dashDirection * (dashSpeed * Time.deltaTime));
            _dashTimeLeft -= Time.deltaTime;
        }
        else
        {
            _isDashing = false;
        }
    }

    private void ApplyGravity()
    {
        if (!_isDashing && !_controller.isGrounded)
            _velocity.y += Physics.gravity.y * Time.deltaTime;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void MovePlayer()
    {
        if (!_isDashing)
        {
            var move = _moveDirection * _currentSpeed;
            move.y = _velocity.y;
            RotateTowardsMouse();
            _controller.Move(move * Time.deltaTime);
        }
    }
    

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }

        if (_controller != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, Vector3.down * (_controller.height / 2 + 0.1f));
        }
    }

    private void RotateTowardsMouse()
    {
        if (!IsOwner) return;

if (Camera.main != null)
{
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        

    Plane groundPlane = new Plane(Vector3.up, transform.position);


    if (groundPlane.Raycast(ray, out float rayDistance))
    {
        Vector3 targetPoint = ray.GetPoint(rayDistance);

    
        Vector3 lookDirection = new Vector3(targetPoint.x, transform.position.y, targetPoint.z);
        
        transform.LookAt(lookDirection);
    }
}
    }
    }
