using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float MoveSpeed = 3.0f;
    public float SprintSpeed = 6.0f;
    public float SprintStamina = 5f; // 질주 시 사용되는 스태미나
    public float JumpForce;
    public float AirControlForce = 10f; // 공중에서 조작하는 힘
    private Vector2 _curMovementInput;
    public LayerMask GroundLayerMask;
    public LayerMask WallLayerMask;

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

    // Player state
    private bool _isSprinting = false;  // 스프린트 상태
    private bool _isGrounded = true;    // 지면 상태
    private bool _isNearWall = false;   // 벽 근처
    private bool _isClimbing = false;   // 벽타기 상태

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

    // PlatformLauncher
    private bool _isMovementLocked = false; // PlatformLauncher으로 날아갈 때 이동 제어 잠깐 잠금

    // Climbing
    private Vector3 _rockNormal;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

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
        HandleSprintingStamina();
    }

    private void FixedUpdate()
    {
        if (!_isMovementLocked)
        {
            HandleClimbing(); // 벽타기 로직

            // 지상에 있을 때와 공중에 있을 때 움직임 분리
            if (_isGrounded)
            {
                Move(); // 지상 이동 로직
            }
            else
            {
                AirMove(); // 공중 이동 로직
            }
        }
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

    private void HandleSprintingStamina()
    {
        if(_isSprinting && _isGrounded)
        {
            // SprintStamina 초당 소모량으로 사용
            bool hasEnoughStamina = CharacterManager.Instance.Player.PlayerCondition.UseStamina(SprintStamina * Time.deltaTime);

            if (!hasEnoughStamina)
            {
                _isSprinting = false;
            }
        }
    }

    private void HandleClimbing()
    {
        _isClimbing = CheckWall();

        if(_isClimbing)
        {
            if (Input.GetKey(KeyCode.W))
            {
                // 벽타기 애니메이션
                //_animator.SetBool("Climbing", true);

                _rigidbody.useGravity = false;
            }

            // 벽 쪽으로 밀어주는 힘 추가 (벽에서 떨어지지 않도록)
            //_rigidbody.AddForce(-_rockNormal * 10f, ForceMode.Force);
        }
        else // 벽에서 멀어질때, 벽타다가 내림
        {
            // 애니메이션 처리
            //_animator.SetBool("Climbing", false);

            _rigidbody.useGravity = true;
        }
    }

    // 지면 상태를 체크하고 애니메이터에 반영
    private void HandleGroundedCheck()
    {
        // IsGrounded()의 결과를 변수에 저장
        _isGrounded = CheckIfGrounded();
        _animator.SetBool(_animIDGrounded, _isGrounded);

        // 땅에 있지 않다면
        if (!_isGrounded)
        {
            // 질주 상태 해제
            _isSprinting = false;
        }
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
        // 지상에 있을 때만 실행 -> 공중에 있을 때 점프 직전의 _speed 값이 그대로 유지
        if (_isGrounded)
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
        // 플레이어가 움직이는 플랫폼 위에 있지 않을 때
        if (transform.parent == null)
        {
            // 입력이 있을 때만 캐릭터의 앞 방향으로 이동
            Vector3 moveDirection = transform.forward * (_curMovementInput == Vector2.zero ? 0f : 1f);

            // Rigidbody의 속도 설정
            Vector3 targetVelocity = moveDirection.normalized * _speed;
            targetVelocity.y = _rigidbody.velocity.y; // Y축 속도는 유지
            _rigidbody.velocity = targetVelocity;
        }
        else
        {
            // 현재 내가 올라탄 플랫폼의 Rigidbody를 가져옴
            Rigidbody platformRigidbody = transform.parent.GetComponent<Rigidbody>();

            // 플레이어의 키보드 입력에 따른 목표 속도를 계산
            Vector3 moveDirection = transform.forward * (_curMovementInput.magnitude > 0.1f ? 1f : 0f);
            Vector3 playerInputVelocity = moveDirection.normalized * _speed;

            // 최종 속도 = (플랫폼의 현재 속도) + (플레이어의 입력 속도)
            // 이렇게 하면 입력이 없을 때 playerInputVelocity가 0이 되어 플랫폼의 속도만 남게 된다
            Vector3 finalVelocity = platformRigidbody.velocity + playerInputVelocity;

            // 점프나 중력에 의한 Y축 속도는 플레이어의 것을 그대로 사용
            finalVelocity.y = _rigidbody.velocity.y;

            // 계산된 최종 속도를 플레이어의 Rigidbody에 적용
            _rigidbody.velocity = finalVelocity;
        }
    }

    private void AirMove()
    {
        // 카메라 기준 방향 계산
        Vector3 inputDir = new Vector3(_curMovementInput.x, 0.0f, _curMovementInput.y).normalized;
        float targetRotation = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + _cameraTargetYaw;
        Vector3 moveDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;

        // 현재 속도에 플레이어의 입력 방향으로 힘을 더해줌
        _rigidbody.AddForce(moveDirection.normalized * AirControlForce * 10f * Time.fixedDeltaTime, ForceMode.Force);
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

    // PlatformLauncher에서 호출할 메서드
    public void Launch(Vector3 direction, float force)
    {
        if(_isMovementLocked)
        {
            return;
        }

        StartCoroutine(LockMovementAndLaunch(direction, force));
    }

    private IEnumerator LockMovementAndLaunch(Vector3 direction, float force)
    {
        // 이동 제어 잠금
        _isMovementLocked = true;

        // 부모-자식 관계 해제
        transform.SetParent(null);

        // 기존 속도 초기화 -> 발사 힘이 정확히 적용되도록 함
        _rigidbody.velocity = Vector3.zero;

        // 주어진 방향과 힘으로 발사
        _rigidbody.AddForce(direction * force, ForceMode.Impulse);

        // 0.5초 동안 기다린 후 이동 제어 잠금 해제
        // 이 시간 동안 Move() 메서드 호출되지 않아 속도 유지
        yield return new WaitForSeconds(0.5f);
        _isMovementLocked = false;
    }

    private bool CheckWall()
    {
        Ray ray = new Ray(transform.position + (transform.up * 1.0f), transform.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * 0.5f, Color.red);

        _isNearWall = Physics.Raycast(ray, out hit, 0.5f, WallLayerMask);

        if(_isNearWall)
        {
            Debug.Log("Detected a wall");

            // Raycast로 얻은 법선 벡터 사용
            _rockNormal = hit.normal;
        }

        return _isNearWall;
    }
}
