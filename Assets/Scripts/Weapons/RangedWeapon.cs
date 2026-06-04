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
    // 재장전 클립 원본 길이 (샷건 1발 장전 모션 속도 계산용)
    private float reloadClipLen;

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
            else if (clip.name.Contains("Reload"))
            {
                // 일반 Reload 우선, 없으면 ReloadNoAmmo로 폴백(SG1처럼 Reload 클립이 없는 무기)
                if (!clip.name.Contains("NoAmmo"))
                    reloadLen = clip.length;
                else if (reloadLen <= 0f)
                    reloadLen = clip.length;
            }
        }

        if (fireLen > 0f && data.fireRate > 0f)
        {
            hasFireClip = true;
            baseFireAnimSpeed = Mathf.Max(fireLen / data.fireRate, 1f);
        }
        weaponAnimator.SetFloat(FireSpeedHash, baseFireAnimSpeed);
        reloadClipLen = reloadLen;
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

        // 샷건(pelletCount>1)은 1발씩 장전 — 발사 입력 시 Use에서 중단된다(펌프식).
        if (IsShellReload)
        {
            currentAmmo++;
            reserveAmmo--;
            int shellBonus = (stats != null && stats.BonusAmmoPercent > 0f)
                ? Mathf.RoundToInt(data.magazineSize * stats.BonusAmmoPercent)
                : 0;
            if (currentAmmo >= data.magazineSize + shellBonus || reserveAmmo <= 0)
            {
                isReloading = false;
                return;
            }
            reloadEndTime = Time.time + PerShellTime();
            TriggerShellReloadAnim();
            return;
        }

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
        if (isReloading)
        {
            // 샷건: 1발씩 장전 중 발사 입력 → 장전 중단하고 이미 장전된 탄으로 즉시 발사(펌프식)
            if (IsShellReload && currentAmmo > 0)
                isReloading = false;
            else
                return false;
        }
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
                float dmg = damageCalc.Compute(new DamageContext
                {
                    baseDamage = data.damage,
                    isHeadshot = isHeadshot,
                    isMelee = false,
                    weaponMultiplier = 1f,
                    targetHealthRatio = (target as IHealthInfo)?.HealthRatio ?? 1f,
                });
                target.TakeDamage(dmg);
                damageCalc.ReportDamage(dmg);
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
        // 샷건은 1발 장전 간격, 그 외는 전체 장전 시간
        reloadEndTime = Time.time + (IsShellReload ? PerShellTime() : data.reloadTime / multiplier);

        if (IsShellReload)
        {
            // 샷건: 첫 발 장전 모션 (이후 발은 Update에서 매 발 트리거)
            TriggerShellReloadAnim();
            return;
        }

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

    // 샷건류는 1발씩 장전 (펠릿 다발 = 샷건으로 식별)
    private bool IsShellReload => data.pelletCount > 1;

    // 1발 장전 간격 = 전체 장전 시간 / 탄창 크기 (전체 체감 시간은 기존과 동일)
    private float PerShellTime()
    {
        float multiplier = stats != null ? stats.ReloadSpeedMultiplier : 1f;
        return (data.reloadTime / multiplier) / Mathf.Max(1, data.magazineSize);
    }

    // 샷건 1발 장전 모션: 클립이 1발 간격(PerShellTime) 동안 1회 완주하도록 속도를 맞춰 매 발 재생
    private void TriggerShellReloadAnim()
    {
        if (weaponAnimator == null) return;
        float shellTime = PerShellTime();
        if (reloadClipLen > 0f && shellTime > 0f)
            weaponAnimator.SetFloat(ReloadSpeedHash, reloadClipLen / shellTime);
        weaponAnimator.SetTrigger(ReloadTrigger);
    }

    private Vector3 ApplySpread(Vector3 direction)
    {
        if (data.spreadAngle <= 0f) return direction;
        return (direction + Random.insideUnitSphere * Mathf.Tan(data.spreadAngle * Mathf.Deg2Rad)).normalized;
    }
}
