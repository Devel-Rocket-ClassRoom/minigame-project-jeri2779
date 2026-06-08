
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

public class CharacterMoves : MonoBehaviour, IImpactReceiver
{
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private bool invertMouseX = false;
    [SerializeField] private bool invertMouseY = false;
    [SerializeField] private float impactKickAmount = 8f; // 적 투사체 피격 시 시점 강제 이동량(도)
    [SerializeField] private float kickSmoothing = 12f;   // 피격 킥이 목표치까지 가는 속도(클수록 빠름/즉각에 가까움)
    private float pendingKickPitch; // 아직 시점에 반영 안 된 킥 잔량(상하)
    private float pendingKickYaw;   // 아직 시점에 반영 안 된 킥 잔량(좌우)
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private WeaponInventory weaponInventory;

    private bool isSprinting = false;
    private bool sprintHeld = false;
    private bool sprintToggle = false; // false=홀드, true=토글
    private bool isJumping = false;
    private bool prevJumping = false;
    private int airJumpsRemaining = 0;
    private float currentStamina;
    private CharacterController controller;
    private CharacterHealth health;
    private CharacterStats stats;
    private PlayerControl playerControl;
    private Vector2 lookInput;
    private Vector2 moveInput;
    private Vector3 velocity;
    private float xRotation;

    public float CurrentStamina => currentStamina;
    // 설정창 마우스 감도 
    public void SetMouseSensitivity(float value) => mouseSensitivity = value;
    // 설정창 마우스 X축 반전
    public void SetInvertX(bool value) => invertMouseX = value;
    // 설정창 마우스 Y축 반전
    public void SetInvertY(bool value) => invertMouseY = value;
    // 설정창 달리기 토글 방식
    public void SetSprintToggle(bool value) => sprintToggle = value;

   
    // 적 투사체 피격 — 시점을 무작위로 틀어 조준을 흐트러뜨린다(잔량 누적, HandleLook이 부드럽게 흘려보냄).
    public void ApplyImpact()
    {
        pendingKickPitch += Random.Range(impactKickAmount * 0.5f, impactKickAmount) * (Random.value < 0.5f ? -1f : 1f);
        pendingKickYaw += Random.Range(impactKickAmount * 0.5f, impactKickAmount) * (Random.value < 0.5f ? -1f : 1f);
    }

    public bool CanMove => playerControl.ControlState == PlayerControlState.Active;
    public bool IsMoving => CanMove && moveInput.sqrMagnitude > 0.01f;
    public bool IsSprinting => isSprinting;
    public float RecoilPitch { get; set; }

    public void ResetStamina()
    {
        currentStamina = stats.MaxStamina;
    }

    public void AddStamina(float amount)
    {
        currentStamina = Mathf.Min(currentStamina + amount, stats.MaxStamina);
    }

    public IEnumerator DeathCameraRotate(float duration)
    {
        float elapsed = 0f;
        float startY = playerCamera.transform.position.y;
        float targetY = transform.position.y - 0.5f;
        float startZ = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float curved = t * t;

            float zRotation = Mathf.Lerp(startZ, 90f, curved);
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, zRotation);

            float newY = Mathf.Lerp(startY, targetY, curved);
            playerCamera.transform.position = new Vector3(
                playerCamera.transform.position.x,
                newY,
                playerCamera.transform.position.z
            );
            yield return null;
        }
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        health = GetComponent<CharacterHealth>();
        stats = GetComponent<CharacterStats>();
        playerControl = GetComponent<PlayerControl>();
        currentStamina = stats.MaxStamina;
    }

    private void Update()
    {
        if (health.State == CharacterHealth.CharacterState.Dead) return;
        HandleLook();
        HandleSprint();
        HandleMove();
        HandleJump();
    }

    public void SetMovable(bool movable)
    {
        if (movable)
            playerControl.Unfreeze();
        else
            playerControl.Freeze();
    }

    public void Teleport(Vector3 position)
    {
        velocity.y = 0f;
        controller.enabled = false;
        transform.position = position;
        controller.enabled = true;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (sprintToggle)
        {
            if (context.started) sprintHeld = !sprintHeld;
            return;
        }

        if (context.started)
            sprintHeld = true;
        else if (context.canceled)
            sprintHeld = false;
    }

    private void HandleMove()
    {
        if (CanMove)
        {
            Vector3 moveDir = transform.right * moveInput.x + transform.forward * moveInput.y;
            float currentSpeed;
            if(isSprinting)
            {
                currentSpeed= stats.MoveSpeed * 2f;
            }
            else
            {
                currentSpeed = stats.MoveSpeed;
            }

            controller.Move(moveDir * currentSpeed * Time.deltaTime);
        }

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += Physics.gravity.y * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
    }

    private void HandleLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;
        if (invertMouseX) mouseX = -mouseX;
        if (invertMouseY) mouseY = -mouseY;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // 잔량을 목표치까지 부드럽게 흘려보냄(프레임 독립 지수 감속, 복귀 없음)
        float kickStep = 1f - Mathf.Exp(-kickSmoothing * Time.deltaTime);
        float stepPitch = pendingKickPitch * kickStep;
        float stepYaw = pendingKickYaw * kickStep;
        xRotation = Mathf.Clamp(xRotation + stepPitch, -90f, 90f);
        pendingKickPitch -= stepPitch;
        pendingKickYaw -= stepYaw;

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation - RecoilPitch, 0f, 0f);
        transform.Rotate(Vector3.up * (mouseX + stepYaw));
    }

    private void HandleSprint()
    {
        bool sprinting = sprintHeld;

        bool isReloading = weaponInventory.CurrentWeapon?.IsReloading ?? false;
        if (sprinting && !isReloading && currentStamina > 0f && moveInput != Vector2.zero && CanMove)
        {
            isSprinting = true;
            currentStamina -= stats.StaminaDrainRate * Time.deltaTime;
            if (currentStamina < 0f) currentStamina = 0f;
        }
        else
        {
            isSprinting = false;
            if (!sprinting)
            {
                currentStamina += stats.StaminaRegenRate * Time.deltaTime;
                if (currentStamina > stats.MaxStamina)
                    currentStamina = stats.MaxStamina;
            }
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
            isJumping = true;
        else if (context.canceled)
            isJumping = false;
    }

    private void HandleJump()
    {
        bool justPressed = isJumping && !prevJumping;
        prevJumping = isJumping;

        if (controller.isGrounded)
        {
            airJumpsRemaining = stats.ExtraJumpCount;
            if (isJumping)
                velocity.y = Mathf.Sqrt((jumpHeight + stats.JumpHeightBonus) * -2f * Physics.gravity.y);
        }
        else if (justPressed && airJumpsRemaining > 0)
        {
            velocity.y = Mathf.Sqrt((jumpHeight + stats.JumpHeightBonus) * -2f * Physics.gravity.y);
            airJumpsRemaining--;
        }
    }
}
