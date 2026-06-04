using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour, IWeapon
{
    [SerializeField] private MeleeWeaponData data;
    [SerializeField] private Animator weaponAnimator;
    [SerializeField] private LayerMask headLayer;

    private static readonly int FireTrigger = Animator.StringToHash("Fire");
    private static readonly int AltFireTrigger = Animator.StringToHash("AltFire");

    private CharacterStats stats;
    private PlayerDamageCalculator damageCalc;
    private float nextSwingTime;
    private float pendingHitTime = -1f;
    private FireContext pendingCtx;

    public WeaponData Data => data;
    public int CurrentAmmo => -1;
    public int ReserveAmmo => 0;
    public bool IsReloading => false;
    public Transform Root => transform;
    public GameObject GameObject => gameObject;

    public void Init(CharacterStats stats)
    {
        this.stats = stats;
        damageCalc = stats.GetComponent<PlayerDamageCalculator>();
    }

    public bool Use(FireContext ctx)
    {
        if (Time.time < nextSwingTime) return false;
        nextSwingTime = Time.time + data.fireRate;

        if (ctx.isAiming)
        {
            weaponAnimator?.SetTrigger(AltFireTrigger);
            HitDetection(ctx, data.altDamageMultiplier, data.altHalfAngleX, data.altHalfAngleY, 1);
            return true;
        }

        weaponAnimator?.SetTrigger(FireTrigger);

        if (data.swingWindup <= 0f)
        {
            HitDetection(ctx, 1f, data.halfAngleX, data.halfAngleY, data.maxTargets);
            return true;
        }

        pendingHitTime = Time.time + data.swingWindup;
        pendingCtx = ctx;
        return true;
    }

    public void Tick(FireContext ctx)
    {
        if (pendingHitTime < 0f) return;
        pendingCtx = ctx;
        if (Time.time >= pendingHitTime)
        {
            HitDetection(pendingCtx, 1f, data.halfAngleX, data.halfAngleY, data.maxTargets);
            pendingHitTime = -1f;
        }
    }

    public void TryReload() { }

    public void CancelAction()
    {
        pendingHitTime = -1f;
    }

    public void ResetAmmo() { }

    private void HitDetection(FireContext ctx, float dmgMult, float angleX, float angleY, int maxHits)
    {
        var hitSet = new HashSet<IDamageable>();

        // Stage 1: Raycast — center precision + headshot
        if (Physics.Raycast(ctx.ray.origin, ctx.ray.direction, out RaycastHit hit, data.range, ctx.layer))
        {
            bool isHead = (headLayer.value & (1 << hit.collider.gameObject.layer)) != 0;
            var target = hit.collider.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                float dmg = damageCalc.Compute(new DamageContext
                {
                    baseDamage = data.damage,
                    isHeadshot = isHead,
                    isMelee = true,
                    weaponMultiplier = dmgMult,
                    targetHealthRatio = (target as IHealthInfo)?.HealthRatio ?? 1f,
                });
                target.TakeDamage(dmg);
                damageCalc.ReportDamage(dmg);
                hitSet.Add(target);
            }
        }

        if (hitSet.Count >= maxHits) return;

        // Stage 2: OverlapSphere + angle filter
        Collider[] cols = Physics.OverlapSphere(ctx.ray.origin, data.sphereRadius, ctx.layer);

        Quaternion rot = Quaternion.LookRotation(ctx.ray.direction);
        foreach (Collider col in cols)
        {
            if (hitSet.Count >= maxHits) break;

            if (col.transform.IsChildOf(stats.transform)) continue;
            var target = col.GetComponentInParent<IDamageable>();
            if (target == null || hitSet.Contains(target)) continue;

            Vector3 local = Quaternion.Inverse(rot) * (col.transform.position - ctx.ray.origin);
            float horizAngle = Mathf.Abs(Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg);
            float vertAngle = Mathf.Abs(Mathf.Atan2(local.y, local.z) * Mathf.Rad2Deg);
            if (horizAngle > angleX || vertAngle > angleY) continue;

            float dmg2 = damageCalc.Compute(new DamageContext
            {
                baseDamage = data.damage,
                isHeadshot = false,
                isMelee = true,
                weaponMultiplier = dmgMult,
                targetHealthRatio = (target as IHealthInfo)?.HealthRatio ?? 1f,
            });
            target.TakeDamage(dmg2);
            damageCalc.ReportDamage(dmg2);
            hitSet.Add(target);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (data == null) return;
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, data.sphereRadius);
    }
#endif
}
