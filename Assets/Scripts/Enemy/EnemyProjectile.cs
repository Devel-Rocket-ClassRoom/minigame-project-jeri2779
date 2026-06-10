using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private GameObject explosionVFX;

    private ProjectilePool pool;
    private float damage;
    private float fuseTime;
    private float explosionRadius;
    private float timer;
    private Collider[] ignoreColliders;
    private Collider projectileCollider;
    private Rigidbody rb;
    private bool hasExploded;

    private void Awake()
    {
        projectileCollider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
    }

    public void AssignPool(ProjectilePool pool)
    {
        this.pool = pool;
    }

    public void Init(float damage, float fuseTime, float explosionRadius, Collider[] ignoreColliders)
    {
        ClearIgnoredCollisions();

        this.damage = damage;
        this.fuseTime = fuseTime;
        this.explosionRadius = explosionRadius;
        this.ignoreColliders = ignoreColliders;
        timer = 0f;
        hasExploded = false;

        ResetMotion();
        ApplyIgnoredCollisions();
    }

    public void ResetForPool()
    {
        ClearIgnoredCollisions();
        ResetMotion();
        damage = 0f;
        fuseTime = 0f;
        explosionRadius = 0f;
        timer = 0f;
        hasExploded = false;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= fuseTime)
            Explode();
    }

    private void Explode()
    {
        if (hasExploded)
            return;

        hasExploded = true;

        if (explosionVFX != null)
            Destroy(Instantiate(explosionVFX, transform.position, Quaternion.identity), 5f);

        var hits = Physics.OverlapSphere(transform.position, explosionRadius);
        var damaged = new HashSet<IDamageable>();

        foreach (var hit in hits)
        {
            var damageable = hit.GetComponentInParent<IDamageable>();
            // 적 폭발은 플레이어에게만 피해 (다른 적·투척자 자해 방지)
            if (damageable is not CharacterHealth || damaged.Contains(damageable)) continue;
            damaged.Add(damageable);
            damageable.TakeDamage(damage);
            hit.GetComponentInParent<IImpactReceiver>()?.ApplyImpact(); // 투사체 피격 시 시점 교란
        }

        if (pool != null)
            pool.Release(this);
        else
            Destroy(gameObject);
    }

    private void ApplyIgnoredCollisions()
    {
        if (projectileCollider == null || ignoreColliders == null)
            return;

        foreach (var ignored in ignoreColliders)
        {
            if (ignored != null)
                Physics.IgnoreCollision(projectileCollider, ignored, true);
        }
    }

    private void ClearIgnoredCollisions()
    {
        if (projectileCollider == null || ignoreColliders == null)
            return;

        foreach (var ignored in ignoreColliders)
        {
            if (ignored != null)
                Physics.IgnoreCollision(projectileCollider, ignored, false);
        }

        ignoreColliders = null;
    }

    private void ResetMotion()
    {
        if (rb == null)
            return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
#endif
}
