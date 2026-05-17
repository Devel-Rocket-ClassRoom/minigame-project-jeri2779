using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 발사 기능 테스트용 스크립트.
/// 연사 + 머즐 플래시 + 히트스캔 + 스프린트 차단 포함.
/// 나중에 WeaponController로 대체 예정.
/// </summary>
public class ShootingTest : MonoBehaviour
{
    [Header("입력")]
    [SerializeField] private InputActionReference _fireAction;

    [Header("참조")]
    [Tooltip("SK_FP_CH_Default_Root 의 Animator")]
    [SerializeField] private Animator _gunAnimator;

    [Tooltip("발사 기준 카메라 (Main Camera)")]
    [SerializeField] private Camera _camera;

    [Tooltip("총구 소켓 Transform (SOCKET_Muzzle)")]
    [SerializeField] private Transform _muzzleSocket;

    [Tooltip("머즐 플래시 파티클 시스템 — SOCKET_Muzzle 하위에 미리 배치된 오브젝트")]
    [SerializeField] private ParticleSystem _muzzleFlashPS;

    [Header("발사 설정")]
    [Tooltip("분당 발사 수 (RPM). 600 = AR 기본값")]
    [SerializeField] private float _roundsPerMinute = 600f;

    [SerializeField] private float _maxDistance = 500f;

    [Header("머즐 플래시")]
    [Tooltip("머즐 플래시 크기. 값이 클수록 이펙트가 커짐")]
    [SerializeField] private float _muzzleFlashSize = 0.5f;

    [Tooltip("한 번 발사 시 방출할 파티클 수")]
    [SerializeField] private int _muzzleFlashCount = 5;

    // Layer Overlay = 인덱스 2 (Fire 스테이트가 있는 레이어)
    private const int LayerOverlay = 2;
    private const int LayerFirstPersonView = 9; // FP_Camera가 렌더링하는 레이어

    private float _fireInterval;
    private float _nextFireTime;
    private FPSController _fpsController;

    private void Awake()
    {
        if (_camera == null)
            _camera = Camera.main;

        _fireInterval = 60f / _roundsPerMinute;
        _fpsController = GetComponent<FPSController>();

        // 머즐 플래시 크기 적용
        if (_muzzleFlashPS != null)
        {
            var main = _muzzleFlashPS.main;
            main.startSize = _muzzleFlashSize;
        }
    }

    private void OnEnable() => _fireAction?.action.Enable();

    private void OnDisable() => _fireAction?.action.Disable();

    private void Update()
    {
        if (_fireAction == null)
            return;

        // 스프린트 중에는 발사 불가
        if (_fpsController != null && _fpsController.IsSprinting)
            return;

        if (_fireAction.action.IsPressed() && Time.time >= _nextFireTime)
        {
            Fire();
            _nextFireTime = Time.time + _fireInterval;
        }
    }

    private void Fire()
    {
        // 1. 발사 애니메이션
        if (_gunAnimator != null)
            _gunAnimator.Play("Fire", LayerOverlay, 0.0f);

        // 2. 머즐 플래시 — Emit()으로 파티클 즉시 burst 방출 (LPSP 원본 방식)
        if (_muzzleFlashPS != null)
            _muzzleFlashPS.Emit(_muzzleFlashCount);

        // 3. 히트스캔 레이캐스트
        Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, _maxDistance))
        {
            // Debug.Log(
            //     $"[ShootingTest] Hit: {hit.collider.gameObject.name} | 거리: {hit.distance:F1}m"
            // );
        }
    }

    private static void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursive(child.gameObject, layer);
    }
}
