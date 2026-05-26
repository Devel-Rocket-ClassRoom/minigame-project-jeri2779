using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private GameObject explosionVFX;

    private float damage;
    private float fuseTime;
    private float explosionRadius;
    private float timer;
    private Collider[] ignoreColliders;

    public void Init(float damage, float fuseTime, float explosionRadius, Collider[] ignoreColliders)
    {
        this.damage = damage;
        this.fuseTime = fuseTime;
        this.explosionRadius = explosionRadius;
        this.ignoreColliders = ignoreColliders;

        var col = GetComponent<Collider>();
        if (col != null)
            foreach (var c in ignoreColliders)
                Physics.IgnoreCollision(col, c);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= fuseTime)
            Explode();
    }

    private void Explode()
    {
        if (explosionVFX != null)
            Destroy(Instantiate(explosionVFX, transform.position, Quaternion.identity), 5f);

        var hits = Physics.OverlapSphere(transform.position, explosionRadius);
        var damaged = new HashSet<IDamageable>();

        foreach (var hit in hits)
        {
            var damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable == null || damaged.Contains(damageable)) continue;
            damaged.Add(damageable);
            damageable.TakeDamage(damage);
        }

        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
#endif
}
