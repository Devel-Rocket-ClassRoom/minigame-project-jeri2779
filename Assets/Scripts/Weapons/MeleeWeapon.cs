using UnityEngine;

public class MeleeWeapon : MonoBehaviour, IWeapon
{
    [SerializeField] private WeaponData data;
    [SerializeField] private Vector3 hitBoxHalfExtents = new Vector3(0.5f, 0.5f, 0.5f);

    private CharacterStats stats;
    private Transform playerRoot;
    private float nextSwingTime;

    public WeaponData Data => data;
    public int CurrentAmmo => -1;
    public int ReserveAmmo => 0;
    public bool IsReloading => false;
    public Transform Root => transform;
    public GameObject GameObject => gameObject;

    public void Init(CharacterStats stats)
    {
        this.stats = stats;
        playerRoot = stats.transform;
    }

    public bool Use(FireContext ctx)
    {
        if (Time.time < nextSwingTime) return false;
        nextSwingTime = Time.time + data.fireRate;

        Vector3 center = ctx.ray.origin + ctx.ray.direction * (data.range * 0.5f);
        Vector3 halfExtents = new Vector3(hitBoxHalfExtents.x, hitBoxHalfExtents.y, data.range * 0.5f);
        Quaternion orientation = Quaternion.LookRotation(ctx.ray.direction);

        Collider[] hits = Physics.OverlapBox(center, halfExtents, orientation, ctx.layer);
        foreach (Collider col in hits)
        {
            if (playerRoot != null && col.transform.IsChildOf(playerRoot)) continue;
            col.GetComponentInParent<IDamageable>()?.TakeDamage(data.damage * stats.AttackMultiplier);
        }
        return true;
    }

    public void TryReload() { }

    public void CancelAction() { }

    public void ResetAmmo() { }
}
