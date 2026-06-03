using UnityEngine;

public class RangedWeapon : MonoBehaviour, IWeapon
{
    [SerializeField] private RangedWeaponData data;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private Animator weaponAnimator;
    [SerializeField] private LayerMask headLayer;

    private static readonly int FireSpeedHash = Animator.StringToHash("FireSpeed");
    private static readonly int ReloadSpeedHash = Animator.StringToHash("ReloadSpeed");
    private static readonly int IsAimingHash = Animator.StringToHash("IsAiming");
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int IsSprintingHash = Animator.StringToHash("IsSprinting");
    private static readonly int DrawTrigger = Animator.StringToHash("Draw");
    private static readonly int ReloadTrigger = Animator.StringToHash("Reload");
    private static readonly int AimShootTrigger = Animator.StringToHash("AimShoot");
    private static readonly int FireState = Animator.StringToHash("Fire");

    private CharacterStats stats;
    private PlayerDamageCalculator damageCalc;
    private PlayerCombatEvents combatEvents;
    private int currentAmmo;
    private int reserveAmmo;
    private bool isReloading;
    private float reloadEndTime;
    private float nextFireTime;
    private bool isAiming;
    // SyncAnimationSpeed에서 계산한 기준 애니메이션 속도 저장
    private float baseFireAnimSpeed = 1f;
    private bool hasFireClip = false;
    private float baseReloadAnimSpeed;

    public WeaponData Data => data;
    public int CurrentAmmo => currentAmmo;
    public int ReserveAmmo => reserveAmmo;
    public bool IsReloading => isReloading;
    public Transform Root => transform;
    public GameObject GameObject => gameObject;

    private void Awake()
    {
        currentAmmo = data.magazineSize;
        reserveAmmo = data.maxReserveAmmo;

        SyncAnimationSpeed();
    }

    private void OnEnable()
    {
        weaponAnimator.SetTrigger(DrawTrigger);
    }

    private void SyncAnimationSpeed()
    {
        if (weaponAnimator == null) return;
        if (weaponAnimator.runtimeAnimatorController == null) return;

        // 파라미터가 존재할 경우 기본값 1.0 보장 (클립 검색 실패 시 애니메이션 멈춤 방지)
        weaponAnimator.SetFloat(FireSpeedHash, 1f);
        weaponAnimator.SetFloat(ReloadSpeedHash, 1f);

        float fireLen = 0f;
        float reloadLen = 0f;
        foreach (var clip in weaponAnimator.runtimeAnimatorController.animationClips)
        {
            bool isFire = clip.name.IndexOf("fire", System.StringComparison.OrdinalIgnoreCase) >= 0;
            bool isAimShot = clip.name.IndexOf("AimShoot", System.StringComparison.OrdinalIgnoreCase) >= 0;
            if (isFire && !isAimShot)
                fireLen = clip.length;
            else if (clip.name.Contains("Reload") && !clip.name.Contains("NoAmmo"))
                reloadLen = clip.length;
        }

        if (fireLen > 0f && data.fireRate > 0f)
        {
            hasFireClip = true;
            baseFireAnimSpeed = Mathf.Max(fireLen / data.fireRate, 1f);
        }
        weaponAnimator.SetFloat(FireSpeedHash, baseFireAnimSpeed);
        if (reloadLen > 0f && data.reloadTime > 0f)
        {
            baseReloadAnimSpeed = reloadLen / data.reloadTime;
            weaponAnimator.SetFloat(ReloadSpeedHash, baseReloadAnimSpeed);
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
        if (stats != null && stats.BonusAmmoPercent > 0f)
            currentAmmo += Mathf.RoundToInt(data.magazineSize * stats.BonusAmmoPercent);
    }

    public void Init(CharacterStats stats)
    {
        this.stats = stats;
        damageCalc = stats.GetComponent<PlayerDamageCalculator>();
        combatEvents = stats.GetComponent<PlayerCombatEvents>();
    }

    public void Tick(FireContext ctx)
    {
        isAiming = ctx.isAiming;
        if (weaponAnimator == null) return;
        weaponAnimator.SetBool(IsAimingHash, isAiming);
        weaponAnimator.SetBool(IsMovingHash, ctx.isMoving);
        weaponAnimator.SetBool(IsSprintingHash, ctx.isSprinting);
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
            float range = data.range + (stats != null ? stats.RangeBonus : 0f);
            if (!Physics.Raycast(ctx.ray.origin, dir, out RaycastHit hit, range, ctx.layer))
                continue;

            bool isHeadshot = (headLayer.value & (1 << hit.collider.gameObject.layer)) != 0;
            var target = hit.collider.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damageCalc.Compute(new DamageContext
                {
                    baseDamage = data.damage,
                    isHeadshot = isHeadshot,
                    isMelee = false,
                    weaponMultiplier = 1f,
                    targetHealthRatio = (target as IHealthInfo)?.HealthRatio ?? 1f,
                }));
            }

            combatEvents?.RaiseRangedHit(hit.point);
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
            weaponAnimator.SetFloat(ReloadSpeedHash, baseReloadAnimSpeed * multiplier);

        weaponAnimator.SetTrigger(ReloadTrigger);
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
        if (isAiming)
        {
            weaponAnimator.SetTrigger(AimShootTrigger);
        }
        else if (hasFireClip)
        {
            float multiplier = stats != null ? stats.FireRateMultiplier : 1f;
            weaponAnimator.SetFloat(FireSpeedHash, baseFireAnimSpeed * multiplier);
            weaponAnimator.Play(FireState, 0, 0f);
        }
    }

    private Vector3 ApplySpread(Vector3 direction)
    {
        if (data.spreadAngle <= 0f) return direction;
        return (direction + Random.insideUnitSphere * Mathf.Tan(data.spreadAngle * Mathf.Deg2Rad)).normalized;
    }
}
