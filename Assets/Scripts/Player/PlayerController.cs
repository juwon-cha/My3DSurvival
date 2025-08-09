using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float MoveSpeed = 3.0f;
    public float SprintSpeed = 6.0f;
    public float JumpForce;
    private Vector2 _curMovementInput;
    public LayerMask GroundLayerMask;

    [Header("Look")]
    public Transform CameraContainer;
    public float MinXLook;
    public float MaxXLook;
    private float _camCurXRot;
    public float LookSensitivity;
    private Vector2 _mouseDelta;
    public bool CanLook = true;

    [Header("Animation Settings")]
    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;
    [Tooltip("Time required to pass before being able to jump again.")]
    public float JumpTimeout = 0.50f;
    [Tooltip("Time required to pass before entering the fall state.")]
    public float FallTimeout = 0.15f;

    public Action Inventory;
    private Rigidbody _rigidbody;
    private Animator _animator;

    // Player state
    private bool _isSprinting = false; // 스프린트 상태
    private bool _isGrounded = true;   // 지면 상태

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    // Animation values
    private float _speed;
    private float _animationBlend;

    // Timers
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        AssignAnimationIDs();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // 마우스 커서 안 보이게 처리

        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
    }

    private void Update()
    {
        HandleGroundedCheck();
        HandleAnimations();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void LateUpdate()
    {
        if(CanLook)
        {
            CameraLook();
        }
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    // 지면 상태를 체크하고 애니메이터에 반영
    private void HandleGroundedCheck()
    {
        // IsGrounded()의 결과를 변수에 저장
        _isGrounded = CheckIfGrounded();
        _animator.SetBool(_animIDGrounded, _isGrounded);
    }

    // 점프, 낙하, 움직임 애니메이션 처리
    private void HandleAnimations()
    {
        // 속도 계산 및 블렌딩
        float targetSpeed = _isSprinting ? SprintSpeed : MoveSpeed;
        if (_curMovementInput == Vector2.zero) targetSpeed = 0.0f;

        float currentHorizontalSpeed = new Vector3(_rigidbody.velocity.x, 0.0f, _rigidbody.velocity.z).magnitude;
        float speedOffset = 0.1f;
        float inputMagnitude = _curMovementInput.magnitude;

        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        _animator.SetFloat(_animIDSpeed, _animationBlend);
        _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);

        // 점프 및 낙하 상태 처리
        if (_isGrounded)
        {
            _fallTimeoutDelta = FallTimeout;

            _animator.SetBool(_animIDJump, false);
            _animator.SetBool(_animIDFreeFall, false);

            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            _jumpTimeoutDelta = JumpTimeout;

            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                _animator.SetBool(_animIDFreeFall, true);
            }
        }
    }

    private void Move()
    {
        float actualMoveSpeed = _isSprinting ? SprintSpeed : MoveSpeed;
        Vector3 dir = transform.forward * _curMovementInput.y + transform.right * _curMovementInput.x;
        dir *= actualMoveSpeed;
        dir.y = _rigidbody.velocity.y;

        _rigidbody.velocity = dir;
    }

    private void CameraLook()
    {
        _camCurXRot += _mouseDelta.y * LookSensitivity;
        _camCurXRot = Mathf.Clamp( _camCurXRot,MinXLook, MaxXLook);
        CameraContainer.localEulerAngles = new Vector3(-_camCurXRot, 0, 0);

        transform.eulerAngles += new Vector3(0, _mouseDelta.x * LookSensitivity, 0);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed)
        {
            _curMovementInput = context.ReadValue<Vector2>();
        }
        else if(context.phase == InputActionPhase.Canceled)
        {
            _curMovementInput = Vector2.zero;
        }
    }

    // 스프린트 입력 처리
    public void OnSprint(InputAction.CallbackContext context)
    {
        _isSprinting = context.ReadValueAsButton();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        _mouseDelta = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started && _isGrounded && _jumpTimeoutDelta <= 0.0f)
        {
            _rigidbody.AddForce(Vector2.up * JumpForce, ForceMode.Impulse);
            _animator.SetBool(_animIDJump, true);
        }
    }

    private bool CheckIfGrounded()
    {
        Ray[] rays = new Ray[4]
        {
            new Ray(transform.position + (transform.forward * 0.2f) + (Vector3.up * 0.01f), Vector3.down),
            new Ray(transform.position + (-transform.forward * 0.2f) + (Vector3.up * 0.01f), Vector3.down),
            new Ray(transform.position + (transform.right * 0.2f) + (Vector3.up * 0.01f), Vector3.down),
            new Ray(transform.position + (-transform.right * 0.2f) + (Vector3.up * 0.01f), Vector3.down),
        };

        for (int i = 0; i < rays.Length; i++)
        {
            Debug.DrawRay(rays[i].origin, rays[i].direction * 0.1f, Color.red);
            if (Physics.Raycast(rays[i], 0.1f, GroundLayerMask))
            {
                return true;
            }
        }

        return false;
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Started)
        {
            Inventory?.Invoke();
            ToggleCursor();
        }
    }

    private void ToggleCursor()
    {
        bool toggle = Cursor.lockState == CursorLockMode.Locked;
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
        CanLook = !toggle;
    }
}
