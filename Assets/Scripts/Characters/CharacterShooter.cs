using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterShooter : MonoBehaviour
{
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private CharacterStats characterStats;

    [SerializeField] private Camera characterCamera;
    [SerializeField] private LayerMask shootableLayer;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private Animator weaponAnimator;
    [SerializeField] private Transform wpRoot;
    [SerializeField] private float wpBlockDist = 0.8f;
    [SerializeField] private Vector3 blockedWpOffset = new Vector3(0f, -0.2f, -0.25f);
    [SerializeField] private float wpSwaySpeed = 12f;

    [SerializeField] private float recoilAmount = 2f;
    [SerializeField] private float recoilResetSpeed = 5f;
    [SerializeField] private int reserveAmmo = 90;
    private int initialReserveAmmo;
    private float currentRecoil = 0f;
    private IDamageable damageable;
    private CharacterHealth health;
    private CharacterMoves characterMoves;

    private Vector3 originWpLocalPosition;
    public int CurrentAmmo => currentAmmo;
    public int ReserveAmmo => reserveAmmo;
    public bool IsReloading => isReloading;
    public int MagazineSize => weaponData.magazineSize;
    public float CurrentDamage => weaponData.damage * characterStats.AttackMultiplier;
    private bool isFiring;
    private float nextFireTime;
    private int currentAmmo;
    private bool isReloading;
    private float reloadEndTime;
    private bool isHeadshot;
    private string headLayerName = "Head";

    private void Awake()
    {
        health = GetComponent<CharacterHealth>();
        characterMoves = GetComponent<CharacterMoves>();
        if (muzzleFlash == null)
            muzzleFlash = GetComponentInChildren<ParticleSystem>(true);
        if (weaponAnimator == null)
            weaponAnimator = GetComponentInChildren<Animator>(true);
        if (wpRoot == null && weaponAnimator != null)
            wpRoot = weaponAnimator.transform;
        if (wpRoot != null)
            originWpLocalPosition = wpRoot.localPosition;
        if (muzzleFlash != null)
            muzzleFlash.Stop();
        currentAmmo = weaponData.magazineSize;
        initialReserveAmmo = reserveAmmo;
    }

    public void ResetAmmo()
    {
        currentAmmo = weaponData.magazineSize;
        reserveAmmo = initialReserveAmmo;
    }

    private void Update()
    {
        if (health.State == CharacterHealth.CharacterState.Dead)
            return;

        HandleReloadComplete();
        HandleFire();
        HandleWeaponCollision();
        currentRecoil = Mathf.Lerp(currentRecoil, 0f, Time.deltaTime * recoilResetSpeed);
        characterMoves.RecoilPitch = currentRecoil;
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.started)
            isFiring = true;
        else if (context.canceled)
            isFiring = false;
    }

    private void HandleFire()
    {
        if (!characterMoves.CanMove) return;
        if (!isFiring) return;
        if (isReloading) return;
        if (currentAmmo <= 0) return;
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + weaponData.fireRate;
        currentAmmo--;

     
        currentRecoil += recoilAmount;

        if (weaponAnimator != null)
            weaponAnimator.SetTrigger("Fire");

        if (muzzleFlash != null)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleFlash.Play();
        }

        float spread = currentRecoil * 0.002f;
        Ray ray = characterCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f + spread, 0f));
        Debug.DrawRay(ray.origin, ray.direction * weaponData.range, Color.red, 1f);
        if (Physics.Raycast(ray, out RaycastHit hit, weaponData.range, shootableLayer))
        {
            
            damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                isHeadshot = hit.collider.gameObject.layer == LayerMask.NameToLayer(headLayerName);
                float damage = weaponData.damage * characterStats.AttackMultiplier;
                if (isHeadshot) damage *= 2f;
                damageable.TakeDamage(damage);
                Debug.Log($"{damage} damage.{(isHeadshot ? " [HEADSHOT]" : "")}");
            }
            
        }
        
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.performed)
            HandleReload();
    }

    private void HandleReload()
    {
        if (isReloading) return;
        if (currentAmmo == weaponData.magazineSize) return;
        if (reserveAmmo <= 0) return;

        isReloading = true;
        reloadEndTime = Time.time + weaponData.reloadTime;

        if (weaponAnimator != null)
            weaponAnimator.SetTrigger("Reload");
    }

    private void HandleReloadComplete()
    {
        if (!isReloading) return;
        if (Time.time < reloadEndTime) return;

        isReloading = false;
        int needed = weaponData.magazineSize - currentAmmo;
        int taken = Mathf.Min(needed, reserveAmmo);
        currentAmmo += taken;
        reserveAmmo -= taken;
    }

    private void HandleWeaponCollision()
    {
        if (wpRoot == null) return;

        Vector3 recoilPos = new Vector3(0f, 0f, -currentRecoil * 0.005f);
        Ray ray = characterCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, wpBlockDist))
        {
            wpRoot.localPosition = Vector3.Lerp(wpRoot.localPosition, originWpLocalPosition + blockedWpOffset + recoilPos, Time.deltaTime * wpSwaySpeed);
        }
        else
        {
            wpRoot.localPosition = Vector3.Lerp(wpRoot.localPosition, originWpLocalPosition + recoilPos, Time.deltaTime * wpSwaySpeed);
        }
    }
}
