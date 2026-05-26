using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    private ThrowableWeaponData data;
    private float explodeTime;

    public void Init(ThrowableWeaponData data)
    {
        this.data = data;
        explodeTime = Time.time + data.fuseTime;
    }

    private void Update()
    {
        if (Time.time >= explodeTime)
            Explode();
    }

    private void Explode()
    {
        if (data.explosionVFX != null)
            Destroy(Instantiate(data.explosionVFX, transform.position, Quaternion.identity), 5f);

        var hitSet = new HashSet<IDamageable>();
        Collider[] cols = Physics.OverlapSphere(transform.position, data.blastRadius);
        foreach (var col in cols)
        {
            var target = col.GetComponentInParent<IDamageable>();
            if (target == null || hitSet.Contains(target)) continue;
            target.TakeDamage(data.blastDamage);
            hitSet.Add(target);
        }
        Destroy(gameObject);
    }
}
