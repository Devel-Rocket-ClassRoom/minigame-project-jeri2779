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
    private IDamageable damageable;
    private CharacterHealth health;

    private Vector3 originWpLocalPosition;
    private bool isFiring;
    private float nextFireTime;
    private int currentAmmo;
    private bool isReloading;
    private float reloadEndTime;

    private void Awake()
    {
        health = GetComponent<CharacterHealth>();
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
    }

    private void Update()
    {
        if (health.State == CharacterHealth.CharacterState.Dead)
            return;

        HandleReloadComplete();
        HandleFire();
        HandleWeaponCollision();
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
        if (!isFiring) return;
        if (isReloading) return;
        if (currentAmmo <= 0) return;
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + weaponData.fireRate;
        currentAmmo--;

        if (weaponAnimator != null)
            weaponAnimator.SetTrigger("Fire");

        if (muzzleFlash != null)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleFlash.Play();
        }

        Ray ray = characterCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Debug.DrawRay(ray.origin, ray.direction * weaponData.range, Color.red, 1f);
        if (Physics.Raycast(ray, out RaycastHit hit, weaponData.range, shootableLayer))
        {
            
            damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
            {
                float damage = weaponData.damage * characterStats.AttackMultiplier;
                damageable.TakeDamage(damage);
                Debug.Log($"Hit {hit.collider.name} for {damage} damage.");
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
        currentAmmo = weaponData.magazineSize;
    }

    private void HandleWeaponCollision()
    {
        if (wpRoot == null) return;

        Ray ray = characterCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, wpBlockDist))
        {
            Vector3 targetPos = originWpLocalPosition + blockedWpOffset;
            wpRoot.localPosition = Vector3.Lerp(wpRoot.localPosition, targetPos, Time.deltaTime * wpSwaySpeed);
        }
        else
        {
            wpRoot.localPosition = Vector3.Lerp(wpRoot.localPosition, originWpLocalPosition, Time.deltaTime * wpSwaySpeed);
        }
    }
}
