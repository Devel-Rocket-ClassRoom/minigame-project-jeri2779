using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 1인칭 플레이어 컨트롤러
/// - Move  액션 : WASD 이동 + 중력
/// - Look  액션 : 마우스 시점 회전
///   - 좌우(Yaw)  → Player 오브젝트 회전
///   - 상하(Pitch) → CameraRoot 회전 (클램핑 적용)
///
/// InputActionReference 방식 사용:
///   Inspector에서 InputSystem_Actions 파일의 각 액션을 직접 연결
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("입력")]
    [SerializeField] private InputActionReference _moveAction;
    [SerializeField] private InputActionReference _lookAction;

    [Header("이동")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _gravity = -20f;

    [Header("시점")]
    [SerializeField] private Transform _cameraRoot; // 카메라를 담은 자식 오브젝트
    [SerializeField] private float _mouseSensitivity = 0.15f;
    [SerializeField] private float _pitchMin = -85f;
    [SerializeField] private float _pitchMax = 85f;

    // 컴포넌트
    private CharacterController _cc;

    // 상태
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private float _verticalVelocity;
    private float _currentPitch; // 카메라 상하 누적 각도

    // -------------------------------------------------------
    // 생명주기
    // -------------------------------------------------------
    private void Awake()
    {
        _cc = GetComponent<CharacterController>();

        // 마우스 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        _moveAction.action.Enable();
        _lookAction.action.Enable();
    }

    private void OnDisable()
    {
        _moveAction.action.Disable();
        _lookAction.action.Disable();
    }

    private void Update()
    {
        ReadInput();
        HandleMovement();
        HandleLook();
    }

    // -------------------------------------------------------
    // 입력 읽기
    // -------------------------------------------------------
    private void ReadInput()
    {
        _moveInput = _moveAction.action.ReadValue<Vector2>();
        _lookInput = _lookAction.action.ReadValue<Vector2>();
    }

    // -------------------------------------------------------
    // 이동 + 중력
    // -------------------------------------------------------
    private void HandleMovement()
    {
        // 수평 이동 (카메라 방향 기준)
        Vector3 move =
            transform.right * _moveInput.x + transform.forward * _moveInput.y;
        move *= _moveSpeed;

        // 중력
        if (_cc.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f; // 바닥에 붙어있게 살짝 눌러줌
        _verticalVelocity += _gravity * Time.deltaTime;
        move.y = _verticalVelocity;

        _cc.Move(move * Time.deltaTime);
    }

    // -------------------------------------------------------
    // 시점 회전
    // -------------------------------------------------------
    private void HandleLook()
    {
        float yaw = _lookInput.x * _mouseSensitivity;
        float pitch = _lookInput.y * _mouseSensitivity;

        // 좌우: Player 오브젝트 자체를 회전
        transform.Rotate(Vector3.up, yaw);

        // 상하: CameraRoot만 회전 (각도 범위 제한)
        _currentPitch -= pitch; // 마우스를 위로 올리면 음수 → 카메라가 위를 보도록
        _currentPitch = Mathf.Clamp(_currentPitch, _pitchMin, _pitchMax);
        _cameraRoot.localRotation = Quaternion.Euler(_currentPitch, 0f, 0f);
    }
}
