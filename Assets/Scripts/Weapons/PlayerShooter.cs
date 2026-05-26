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
    [SerializeField] private float defaultFOV = 60f;
    [SerializeField] private float adsFOV = 40f;
    [SerializeField] private float adsSpeed = 10f;

    private CharacterHealth health;
    private CharacterMoves characterMoves;
    private float currentRecoil;
    private bool isFiring;
    private bool firedThisPress;
    private bool isAiming;
    private bool altFiredThisPress;
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
        HandleAim();
        TickCurrentWeapon();
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

    public void OnRightClick(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isAiming = true;
            altFiredThisPress = false;
        }
        else if (context.canceled)
        {
            isAiming = false;
            altFiredThisPress = false;
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

    private void HandleAim()
    {
        var weapon = inventory.CurrentWeapon;

        bool adsActive = isAiming && weapon?.Data.category != WeaponCategory.Melee;
        float targetFOV = adsActive ? adsFOV : defaultFOV;
        characterCamera.fieldOfView = Mathf.Lerp(characterCamera.fieldOfView, targetFOV, Time.deltaTime * adsSpeed);

        if (!isAiming) return;
        if (weapon == null) return;
        if (weapon.Data.category != WeaponCategory.Melee) return;
        if (!characterMoves.CanMove) return;
        if (inventory.IsDrawing) return;
        if (altFiredThisPress) return;

        var ctx = new FireContext { ray = BuildAimRay(), layer = shootableLayer, isAiming = true };
        if (weapon.Use(ctx))
            altFiredThisPress = true;
    }

    private void TickCurrentWeapon()
    {
        var weapon = inventory.CurrentWeapon;
        if (weapon == null) return;
        weapon.Tick(new FireContext { ray = BuildAimRay(), layer = shootableLayer, isFiring = isFiring, isAiming = isAiming, isMoving = characterMoves.IsMoving, isSprinting = characterMoves.IsSprinting });
        if (weapon.Data.category == WeaponCategory.Throwable && weapon.CurrentAmmo == 0)
            inventory.DiscardSlot((int)WeaponCategory.Throwable);
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

        if (weapon is MeleeWeapon melee)
        {
            targetPos += melee.SwingPositionOffset;
            wpRoot.localPosition = Vector3.Lerp(wpRoot.localPosition, targetPos, Time.deltaTime * wpSwaySpeed);
            wpRoot.localRotation = Quaternion.Euler(weapon.Data.viewModelRotation + melee.SwingEulerOffset);
        }
        else
        {
            wpRoot.localPosition = Vector3.Lerp(wpRoot.localPosition, targetPos, Time.deltaTime * wpSwaySpeed);
            wpRoot.localRotation = Quaternion.Euler(weapon.Data.viewModelRotation);
        }
    }

    private Ray BuildAimRay()
    {
        Vector2 offset = Random.insideUnitCircle * (currentRecoil * 0.002f);
        return characterCamera.ViewportPointToRay(new Vector3(0.5f + offset.x, 0.5f + offset.y, 0f));
    }
}
