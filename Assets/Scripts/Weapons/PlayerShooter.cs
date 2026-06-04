using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
    [SerializeField] private float scopeFOV = 15f;

    private PlayerControl playerControl;
    private CharacterMoves characterMoves;
    private float currentRecoil;
    private bool isFiring;
    private bool firedThisPress;
    private bool isAiming;
    private bool altFiredThisPress;
    private bool aimToggle; // false=홀드, true=토글
    private IWeapon cachedWeapon;

    public bool IsAiming => isAiming;
    public void SetAimToggle(bool value) => aimToggle = value;
    public bool IsFirePressed => isFiring;
    // 설정창 FOV (SettingsController가 SaveData 값으로 호출) — 힙파이어 기준 FOV
    public void SetDefaultFOV(float value) => defaultFOV = value;
    private Vector3 originWpLocalPosition;

    // 발사/조준 공통 차단 조건 (이동 가능 + 비질주 + 비드로우)
    private bool CanFire => characterMoves.CanMove && !characterMoves.IsSprinting && !inventory.IsDrawing;

    private void Awake()
    {
        playerControl = GetComponent<PlayerControl>();
        characterMoves = GetComponent<CharacterMoves>();
    }

 

    private void Update()
    {
        if (playerControl.ControlState == PlayerControlState.Dead) return;

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
        if (aimToggle)
        {
            if (context.started)
            {
                isAiming = !isAiming;
                altFiredThisPress = false;
            }
            return;
        }

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
        if (context.performed && !inventory.IsDrawing && !characterMoves.IsSprinting)
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

        bool adsActive = isAiming && weapon?.Data.category != WeaponCategory.Melee && !characterMoves.IsSprinting;
        var rd = weapon?.Data as RangedWeaponData;
        bool scopeActive = adsActive && rd != null && rd.useScope;
        float targetFOV = scopeActive ? scopeFOV : (adsActive ? adsFOV : defaultFOV);
        characterCamera.fieldOfView = Mathf.Lerp(characterCamera.fieldOfView, targetFOV, Time.deltaTime * adsSpeed);

        if (!isAiming) return;
        if (weapon == null) return;
        if (weapon.Data.category != WeaponCategory.Melee) return;
        if (!CanFire) return;
        if (altFiredThisPress) return;

        var ctx = new FireContext { ray = BuildAimRay(), layer = shootableLayer, isAiming = true };
        if (weapon.Use(ctx))
            altFiredThisPress = true;
    }

    private void TickCurrentWeapon()
    {
        var weapon = inventory.CurrentWeapon;
        if (weapon == null) return;
        weapon.Tick(new FireContext { ray = BuildAimRay(), layer = shootableLayer, isFiring = isFiring, isAiming = isAiming && !characterMoves.IsSprinting, isMoving = characterMoves.IsMoving, isSprinting = characterMoves.IsSprinting });
        if (weapon.Data.category == WeaponCategory.Throwable && weapon.CurrentAmmo == 0)
            inventory.DiscardSlot((int)WeaponCategory.Throwable);
    }

    private static readonly List<RaycastResult> _uiRaycastResults = new List<RaycastResult>();
    private bool IsPointerOverUI()
    {
        if (Cursor.lockState == CursorLockMode.Locked) return false;
        if (EventSystem.current == null) return false;
        if (Mouse.current == null) return false;
        var pointer = new PointerEventData(EventSystem.current) { position = Mouse.current.position.ReadValue() };
        _uiRaycastResults.Clear();
        EventSystem.current.RaycastAll(pointer, _uiRaycastResults);
        return _uiRaycastResults.Count > 0;
    }

    private void HandleFire()
    {
        if (!CanFire) return;
        if (!isFiring) return;
        if (IsPointerOverUI()) return;

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

        if (!fired && weapon.CurrentAmmo == 0 && !firedThisPress)
        {
            weapon.TryReload();
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
