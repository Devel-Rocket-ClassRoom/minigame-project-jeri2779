using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private WeaponInventoryNew inventory;
    [SerializeField] private Camera characterCamera;
    [SerializeField] private LayerMask shootableLayer;
    [SerializeField] private float recoilResetSpeed = 5f;
    [SerializeField] private float wpBlockDist = 0.8f;
    [SerializeField] private Vector3 blockedWpOffset = new Vector3(0f, -0.2f, -0.25f);
    [SerializeField] private float wpSwaySpeed = 12f;

    private CharacterHealth health;
    private CharacterMoves characterMoves;
    private float currentRecoil;
    private bool isFiring;
    private bool firedThisPress;
    private IWeapon cachedWeapon;
    private Vector3 originWpLocalPosition;

    private void Awake()
    {
        health = GetComponent<CharacterHealth>();
        characterMoves = GetComponent<CharacterMoves>();
    }

    private void Update()
    {
        if (health.State == CharacterHealth.CharacterState.Dead) return;

        HandleFire();
        HandleWeaponCollision();
        HandleSlotInput();
        currentRecoil = Mathf.Lerp(currentRecoil, 0f, Time.deltaTime * recoilResetSpeed);
        characterMoves.RecoilPitch = currentRecoil;
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isFiring = true;
            firedThisPress = false;
        }
        else if (context.canceled)
        {
            isFiring = false;
            firedThisPress = false;
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed && !inventory.IsDrawing)
            inventory.CurrentWeapon?.TryReload();
    }

    public void OnSwitchSlot(int slotIndex)
    {
        inventory.SwitchSlot(slotIndex);
    }

    private void HandleSlotInput()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.digit1Key.wasPressedThisFrame) inventory.SwitchSlot(0);
        else if (kb.digit2Key.wasPressedThisFrame) inventory.SwitchSlot(1);
        else if (kb.digit3Key.wasPressedThisFrame) inventory.SwitchSlot(2);
        else if (kb.digit4Key.wasPressedThisFrame) inventory.SwitchSlot(3);
    }

    private void HandleFire()
    {
        if (!characterMoves.CanMove) return;
        if (!isFiring) return;
        if (inventory.IsDrawing) return;

        var weapon = inventory.CurrentWeapon;
        if (weapon == null) return;

        if (!weapon.Data.isAutomatic && firedThisPress) return;

        var ctx = new FireContext
        {
            ray = BuildAimRay(),
            layer = shootableLayer
        };

        bool fired = weapon.Use(ctx);

        if (fired && weapon.CurrentAmmo != -1)
        {
            currentRecoil += weapon.Data.verticalRecoil;
            firedThisPress = true;
        }
    }

    private void HandleWeaponCollision()
    {
        var weapon = inventory.CurrentWeapon;
        if (weapon == null) return;

        Transform wpRoot = weapon.Root;
        if (wpRoot == null) return;

        cachedWeapon = weapon;
        originWpLocalPosition = weapon.Data.viewModelPosition;

        Vector3 recoilOffset = new Vector3(0f, 0f, -currentRecoil * 0.005f);
        Ray ray = characterCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        Vector3 targetPos = Physics.Raycast(ray, out _, wpBlockDist)
            ? originWpLocalPosition + blockedWpOffset + recoilOffset
            : originWpLocalPosition + recoilOffset;

        wpRoot.localPosition = Vector3.Lerp(wpRoot.localPosition, targetPos, Time.deltaTime * wpSwaySpeed);
        wpRoot.localRotation = Quaternion.Euler(weapon.Data.viewModelRotation);
    }

    private Ray BuildAimRay()
    {
        Vector2 offset = Random.insideUnitCircle * (currentRecoil * 0.002f);
        return characterCamera.ViewportPointToRay(new Vector3(0.5f + offset.x, 0.5f + offset.y, 0f));
    }
}
