using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 1인칭 플레이어 컨트롤러 — 1P 전용 (Half-Life 스타일)
///
/// [이동]   WASD — Walk / Run (속도 기반 자동 전환)
/// [시점]   마우스 — 좌우: Player 회전 / 상하: CameraRoot 클램핑
/// [점프]   Space  — 점프
/// [스프린트] Shift — 전력질주
///
/// Animator 파라미터 (GunAnimator, 1P):
///   walkSpeed (float) : 현재 이동 속도
///   maxSpeed  (int)   : 0=Idle  5=Walk  9=Sprint
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("입력")]
    [SerializeField] private InputActionReference _moveAction;
    [SerializeField] private InputActionReference _lookAction;
    [SerializeField] private InputActionReference _jumpAction;
    [SerializeField] private InputActionReference _sprintAction;

    [Header("이동 속도")]
    [SerializeField] private float _walkSpeed = 3.5f;
    [SerializeField] private float _runSpeed = 5.5f;
    [SerializeField] private float _sprintSpeed = 9f;

    [Header("점프 / 중력")]
    [SerializeField] private float _jumpHeight = 1.2f;
    [SerializeField] private float _gravity = -20f;

    [Header("시점")]
    [SerializeField] private Transform _cameraRoot;
    [SerializeField] private float _mouseSensitivity = 0.15f;
    [SerializeField] private float _pitchMin = -85f;
    [SerializeField] private float _pitchMax = 85f;

    [Header("1인칭 뷰모델")]
    [Tooltip("FPS_Character_prefab의 Animator (GunAnimator.controller)")]
    [SerializeField] private Animator _gunAnimator;

    private CharacterController _cc;
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private float _verticalVelocity;
    private float _currentPitch;
    private bool _isSprinting;

    /// <summary>외부 시스템(ShootingTest 등)에서 스프린트 상태 참조용</summary>
    public bool IsSprinting => _isSprinting;

    // LPSP AC_LPSP_PCH 파라미터
    private static readonly int HashMovement = Animator.StringToHash("Movement");
    private static readonly int HashRunning = Animator.StringToHash("Running");

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (Camera.main != null)
            Camera.main.nearClipPlane = 0.01f;
    }

    private void OnEnable()
    {
        _moveAction?.action.Enable();
        _lookAction?.action.Enable();
        _jumpAction?.action.Enable();
        _sprintAction?.action.Enable();
    }

    private void OnDisable()
    {
        _moveAction?.action.Disable();
        _lookAction?.action.Disable();
        _jumpAction?.action.Disable();
        _sprintAction?.action.Disable();
    }

    private void Update()
    {
        ReadInput();
        HandleMovement();
        HandleLook();
        UpdateGunAnimator();
    }

    private void ReadInput()
    {
        _moveInput = _moveAction?.action.ReadValue<Vector2>() ?? Vector2.zero;
        _lookInput = _lookAction?.action.ReadValue<Vector2>() ?? Vector2.zero;
        _isSprinting = _sprintAction != null && _sprintAction.action.ReadValue<float>() > 0.5f;
    }

    private void HandleMovement()
    {
        bool isGrounded = _cc.isGrounded;
        float moveLen = _moveInput.magnitude;

        float speed =
            moveLen < 0.1f ? 0f
            : _isSprinting ? _sprintSpeed
            : moveLen > 0.7f ? _runSpeed
            : _walkSpeed;

        Vector3 horizontal =
            (transform.right * _moveInput.x + transform.forward * _moveInput.y).normalized * speed;

        if (isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;

        if (_jumpAction != null && _jumpAction.action.WasPressedThisFrame() && isGrounded)
            _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);

        _verticalVelocity += _gravity * Time.deltaTime;
        _cc.Move((horizontal + Vector3.up * _verticalVelocity) * Time.deltaTime);
    }

    private void HandleLook()
    {
        float yaw = _lookInput.x * _mouseSensitivity;
        float pitch = _lookInput.y * _mouseSensitivity;

        transform.Rotate(Vector3.up, yaw);

        _currentPitch -= pitch;
        _currentPitch = Mathf.Clamp(_currentPitch, _pitchMin, _pitchMax);

        if (_cameraRoot != null)
            _cameraRoot.localRotation = Quaternion.Euler(_currentPitch, 0f, 0f);
    }

    private void UpdateGunAnimator()
    {
        if (_gunAnimator == null)
            return;

        float speed = _cc.velocity.magnitude;
        // Movement: 0=정지, 0~1=이동 (LPSP 규격)
        float movement = Mathf.InverseLerp(0f, _sprintSpeed, speed);
        _gunAnimator.SetFloat(HashMovement, movement, 0.1f, Time.deltaTime);
        _gunAnimator.SetBool(HashRunning, _isSprinting && speed > 0.1f);
    }
}
