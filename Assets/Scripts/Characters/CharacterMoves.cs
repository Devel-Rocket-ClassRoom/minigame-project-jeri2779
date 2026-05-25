
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

public class CharacterMoves : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private Camera playerCamera;

    private bool isSprinting = false;
    private bool isJumping = false;
    private bool canMove = true;
    private float currentStamina;
    private CharacterController controller;
    private CharacterHealth health;
    private CharacterStats stats;
    private Vector2 lookInput;
    private Vector2 moveInput;
    private Vector3 velocity;
    private float xRotation;

    public float CurrentStamina => currentStamina;
    public bool CanMove => canMove;
    public bool IsMoving => canMove && moveInput.sqrMagnitude > 0.01f;
    public bool IsSprinting => isSprinting;
    public float RecoilPitch { get; set; }

    public void ResetStamina()
    {
        currentStamina = stats.MaxStamina;
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
        canMove = movable;
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
        if (context.started)
            isSprinting = true;
        else if (context.canceled)
            isSprinting = false;
    }

    private void HandleMove()
    {
        if (canMove)
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

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation - RecoilPitch, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleSprint()
    {
        bool sprinting = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

        if (sprinting && currentStamina > 0f && moveInput != Vector2.zero && canMove)
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
        if (controller.isGrounded && isJumping)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
    }
}
