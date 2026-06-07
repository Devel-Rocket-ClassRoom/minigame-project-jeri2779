using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    private ThrowableWeaponData data;
    private PlayerDamageCalculator damageCalc;
    private float explodeTime;

    public void Init(ThrowableWeaponData data, PlayerDamageCalculator damageCalc)
    {
        this.data = data;
        this.damageCalc = damageCalc;
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
            hitSet.Add(target);
            if (target is CharacterHealth)
            {
                target.TakeDamage(data.blastDamage * 0.5f); // 플레이어 자해: 절반, totalDamage 미집계
            }
            else
            {
                target.TakeDamage(data.blastDamage);
                damageCalc?.ReportDamage(data.blastDamage); // 적 피해만 totalDamage 집계
            }
        }
        Destroy(gameObject);
    }
}
