using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour, IWeapon
{
    [SerializeField] private MeleeWeaponData data;
    [SerializeField] private Animator weaponAnimator;

    private CharacterStats stats;
    private float nextSwingTime;
    private float pendingHitTime = -1f;
    private FireContext pendingCtx;
    private int headLayer;

    public WeaponData Data => data;
    public int CurrentAmmo => -1;
    public int ReserveAmmo => 0;
    public bool IsReloading => false;
    public Transform Root => transform;
    public GameObject GameObject => gameObject;

    private void Awake()
    {
        headLayer = LayerMask.NameToLayer("Head");
    }

    public void Init(CharacterStats stats)
    {
        this.stats = stats;
    }

    public bool Use(FireContext ctx)
    {
        if (Time.time < nextSwingTime) return false;
        nextSwingTime = Time.time + data.fireRate;

        if (ctx.isAiming)
        {
            weaponAnimator?.SetTrigger("AltFire");
            HitDetection(ctx, data.altDamageMultiplier, data.altHalfAngleX, data.altHalfAngleY, 1);
            return true;
        }

        weaponAnimator?.SetTrigger("Fire");

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
        float dmgBase = data.damage * stats.AttackMultiplier * dmgMult;

        // Stage 1: Raycast — center precision + headshot
        if (Physics.Raycast(ctx.ray.origin, ctx.ray.direction, out RaycastHit hit, data.range, ctx.layer))
        {
            bool isHead = hit.collider.gameObject.layer == headLayer;
            var target = hit.collider.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(dmgBase * (isHead ? 2f : 1f));
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

            target.TakeDamage(dmgBase);
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
