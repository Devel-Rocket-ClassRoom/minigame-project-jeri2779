using UnityEngine;

public class RangedWeapon : MonoBehaviour, IWeapon
{
    [SerializeField] private RangedWeaponData data;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private Animator weaponAnimator;

    private CharacterStats stats;
    private int currentAmmo;
    private int reserveAmmo;
    private bool isReloading;
    private float reloadEndTime;
    private float nextFireTime;
    private int headLayer;
    private bool isAiming;
    // SyncAnimationSpeed에서 계산한 기준 애니메이션 속도 저장
    private float baseReloadAnimSpeed;

    public WeaponData Data => data;
    public int CurrentAmmo => currentAmmo;
    public int ReserveAmmo => reserveAmmo;
    public bool IsReloading => isReloading;
    public Transform Root => transform;
    public GameObject GameObject => gameObject;

    private void Awake()
    {
        headLayer = LayerMask.NameToLayer("Head");
        currentAmmo = data.magazineSize;
        reserveAmmo = data.maxReserveAmmo;
      
        SyncAnimationSpeed();
    }

    private void OnEnable()
    {
        weaponAnimator.SetTrigger("Draw");
    }

    private void SyncAnimationSpeed()
    {
        if (weaponAnimator == null) return;
        if (weaponAnimator.runtimeAnimatorController == null) return;

        // 파라미터가 존재할 경우 기본값 1.0 보장 (클립 검색 실패 시 애니메이션 멈춤 방지)
        weaponAnimator.SetFloat("FireSpeed", 1f);
        weaponAnimator.SetFloat("ReloadSpeed", 1f);

        float fireLen = 0f;
        float reloadLen = 0f;
        foreach (var clip in weaponAnimator.runtimeAnimatorController.animationClips)
        {
            if (clip.name.Contains("Fire") && !clip.name.Contains("AimShoot") && !clip.name.Contains("Aimshoot"))
                fireLen = clip.length;
            else if (clip.name.Contains("Reload") && !clip.name.Contains("NoAmmo"))
                reloadLen = clip.length;
        }

        if (fireLen > 0f && data.fireRate > 0f)
            weaponAnimator.SetFloat("FireSpeed", fireLen / data.fireRate);
        if (reloadLen > 0f && data.reloadTime > 0f)
        {
            baseReloadAnimSpeed = reloadLen / data.reloadTime;
            weaponAnimator.SetFloat("ReloadSpeed", baseReloadAnimSpeed);
        }
    }

    private void Update()
    {
        if (!isReloading) return;
        if (Time.time < reloadEndTime) return;

        isReloading = false;
        int needed = data.magazineSize - currentAmmo;
        int taken = Mathf.Min(needed, reserveAmmo);
        currentAmmo += taken;
        reserveAmmo -= taken;
    }

    public void Init(CharacterStats stats)
    {
        this.stats = stats;
    }

    public void Tick(FireContext ctx)
    {
        isAiming = ctx.isAiming;
        if (weaponAnimator == null) return;
        weaponAnimator.SetBool("IsAiming", isAiming);
        weaponAnimator.SetBool("IsMoving", ctx.isMoving);
        weaponAnimator.SetBool("IsSprinting", ctx.isSprinting);
    }

    public bool Use(FireContext ctx)
    {
        if (isReloading) return false;
        if (currentAmmo <= 0) return false;
        if (Time.time < nextFireTime) return false;

        nextFireTime = Time.time + data.fireRate / (stats != null ? stats.FireRateMultiplier : 1f);
        currentAmmo--;
        PlayFireFx();

        for (int i = 0; i < data.pelletCount; i++)
        {
            Vector3 dir = ApplySpread(ctx.ray.direction);
            Debug.DrawRay(ctx.ray.origin, dir * data.range, Color.red, 1f);
            float range = data.range + (stats != null ? stats.RangeBonus : 0f);
            if (!Physics.Raycast(ctx.ray.origin, dir, out RaycastHit hit, range, ctx.layer))
                continue;

            Debug.DrawLine(ctx.ray.origin, hit.point, Color.yellow, 1f);
            Debug.DrawRay(hit.point, Vector3.up * 0.3f, Color.green, 1f);
            bool isHeadshot = hit.collider.gameObject.layer == headLayer;
            float damage = data.damage * (stats != null ? stats.AttackMultiplier : 1f) * (isHeadshot ? 2f : 1f);
            hit.collider.GetComponentInParent<IDamageable>()?.TakeDamage(damage);
        }
        return true;
    }

    public void TryReload()
    {
        if (isReloading) return;
        if (currentAmmo == data.magazineSize) return;
        if (reserveAmmo <= 0) return;

        isReloading = true;

        // stats.ReloadSpeedMultiplier 로 재장전 시간 단축 (강화 없을 때 1.0 → 변화 없음)
        float multiplier = stats != null ? stats.ReloadSpeedMultiplier : 1f;
        float actualReloadTime = data.reloadTime / multiplier;
        reloadEndTime = Time.time + actualReloadTime;

        // 애니메이션도 동일 배율로 빠르게
        if (weaponAnimator != null && baseReloadAnimSpeed > 0f)
            weaponAnimator.SetFloat("ReloadSpeed", baseReloadAnimSpeed * multiplier);

        weaponAnimator.SetTrigger("Reload");
    }

    public void CancelAction()
    {
        isReloading = false;
    }

    public void ResetAmmo()
    {
        currentAmmo = data.magazineSize;
        reserveAmmo = data.maxReserveAmmo;
        isReloading = false;
    }

    private void PlayFireFx()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleFlash.Play();
        }
        weaponAnimator.SetTrigger(isAiming ? "AimShoot" : "Fire");
    }

    private Vector3 ApplySpread(Vector3 direction)
    {
        if (data.spreadAngle <= 0f) return direction;
        return (direction + Random.insideUnitSphere * Mathf.Tan(data.spreadAngle * Mathf.Deg2Rad)).normalized;
    }
}
