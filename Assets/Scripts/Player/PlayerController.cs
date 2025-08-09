using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

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

    // 카메라 회전 값
    private float _cameraTargetYaw;     // 좌우 회전
    private float _cameraTargetPitch;   // 상하 회전

    public float LookSensitivity;
    private Vector2 _mouseDelta;
    public bool CanLook = true;

    [Header("Rotation")]
    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

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
    private Camera _mainCamera;

    // Player state
    private bool _isSprinting = false; // 스프린트 상태
    private bool _isGrounded = true;   // 지면 상태

    // Rotation
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;

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
        _mainCamera = Camera.main;

        AssignAnimationIDs();
    }

    private void Start()
    {
        // 시작할 때 카메라의 Yaw 값을 캐릭터의 현재 방향으로 초기화
        _cameraTargetYaw = transform.eulerAngles.y;

        UnityEngine.Cursor.lockState = CursorLockMode.Locked; // 마우스 커서 안 보이게 처리

        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
    }

    private void Update()
    {
        HandleGroundedCheck();
        HandleRotation();
        HandleSpeedAndAnimation();
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

    private void HandleRotation()
    {
        // 입력이 있을 때만 회전
        if (_curMovementInput != Vector2.zero)
        {
            // 2D 입력(x, y)을 3D 방향 벡터로 변환 (카메라 기준)
            Vector3 inputDir = new Vector3(_curMovementInput.x, 0.0f, _curMovementInput.y).normalized;

            // 카메라의 Y축 회전값을 더해 목표 회전 각도 계산
            // Mathf.Atan2(...)는 입력 방향의 절대 각도를 계산(예: D키 = 90도)
            // 여기에 _cameraTargetYaw를 더함
            // 즉, "카메라가 현재 바라보는 방향을 기준으로 90도만큼 돌아라" 라는 의미
            _targetRotation = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + _cameraTargetYaw;

            // SmoothDampAngle을 사용하여 캐릭터를 부드럽게 회전
            // 현재 캐릭터의 각도에서 목표 각도(_targetRotation)까지 부드럽게 회전하는 값을 계산
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

            // 계산된 회전 값을 캐릭터의 transform에 최종 적용
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }
    }

    // 점프, 낙하, 움직임 애니메이션 처리
    private void HandleSpeedAndAnimation()
    {
        // 목표 속도 설정
        float targetSpeed = _isSprinting ? SprintSpeed : MoveSpeed;
        if (_curMovementInput == Vector2.zero)
        {
            targetSpeed = 0.0f;
        }

        // 현재 속도와 목표 속도를 기반으로 가/감속
        float currentHorizontalSpeed = new Vector3(_rigidbody.velocity.x, 0.0f, _rigidbody.velocity.z).magnitude;
        float speedOffset = 0.1f;

        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // Lerp를 사용하여 부드러운 속도 변화 생성
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * SpeedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f)
        {
            _animationBlend = 0f;
        }

        // 애니메이터에 값 전달
        _animator.SetFloat(_animIDSpeed, _animationBlend);
        _animator.SetFloat(_animIDMotionSpeed, _curMovementInput.magnitude); // 입력 크기에 따라 모션 속도 조절

        // 점프 및 낙하 상태 처리
        if (_isGrounded)
        {
            _fallTimeoutDelta = FallTimeout;
            _animator.SetBool(_animIDJump, false);
            _animator.SetBool(_animIDFreeFall, false);
            if (_jumpTimeoutDelta >= 0.0f) _jumpTimeoutDelta -= Time.deltaTime;
        }
        else
        {
            _jumpTimeoutDelta = JumpTimeout;
            if (_fallTimeoutDelta >= 0.0f) _fallTimeoutDelta -= Time.deltaTime;
            else _animator.SetBool(_animIDFreeFall, true);
        }
    }

    private void Move()
    {
        // 입력이 있을 때만 캐릭터의 앞 방향으로 이동
        Vector3 moveDirection = transform.forward * (_curMovementInput == Vector2.zero ? 0f : 1f);

        // Rigidbody의 속도 설정
        Vector3 targetVelocity = moveDirection.normalized * _speed;
        targetVelocity.y = _rigidbody.velocity.y; // Y축 속도는 유지
        _rigidbody.velocity = targetVelocity;
    }

    private void CameraLook()
    {
        // 마우스 입력으로 카메라의 Yaw와 Pitch 값 누적
        // 마우스 좌우 움직임(_mouseDelta.x)으로 _cameraTargetYaw 값을 계속 누적
        // 카메라를 수평으로 공전시키는 역할. 캐릭터는 전혀 움직이지 않는다.
        _cameraTargetYaw += _mouseDelta.x * LookSensitivity;

        // 마우스 상하 움직임(_mouseDelta.y)으로 _cameraTargetPitch 값을 누적
        // 카메라의 수직 각도(올려다보기/내려다보기)를 결정
        _cameraTargetPitch -= _mouseDelta.y * LookSensitivity; // 위아래 반전이 필요하면 +로 변경
        _cameraTargetPitch = Mathf.Clamp(_cameraTargetPitch, MinXLook, MaxXLook);

        // 카메라 컨테이너의 회전을 직접 설정
        // Y축 회전은 _cameraTargetYaw 값을, X축 회전은 _cameraTargetPitch 값을 사용
        CameraContainer.rotation = Quaternion.Euler(_cameraTargetPitch, _cameraTargetYaw, 0.0f);
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
        bool toggle = UnityEngine.Cursor.lockState == CursorLockMode.Locked;
        UnityEngine.Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
        CanLook = !toggle;
    }
}
