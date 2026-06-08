using System.Collections.Generic;
using UnityEngine;

// 폭발탄 행동 전담. 쿨다운 관리 + 명중 지점 폭발(OverlapSphere) 실행.
[RequireComponent(typeof(CharacterStats))]
[RequireComponent(typeof(PlayerCombatEvents))]
public class PlayerExplosive : MonoBehaviour
{
    private CharacterStats stats;
    private PlayerCombatEvents combatEvents;
    private UpgradeManager upgradeManager;
    private PlayerDamageCalculator damageCalc;
    private float nextExplosiveTime = float.MaxValue;

    private void Awake()
    {
        stats = GetComponent<CharacterStats>();
        combatEvents = GetComponent<PlayerCombatEvents>();
        upgradeManager = GetComponent<UpgradeManager>();
        damageCalc = GetComponent<PlayerDamageCalculator>();
    }

    private void OnEnable()
    {
        if (combatEvents != null)
            combatEvents.OnRangedHit += HandleRangedHit;
        if (upgradeManager != null)
            upgradeManager.OnStatUpgradeLevelChanged += HandleStatUpgrade;
    }

    private void OnDisable()
    {
        if (combatEvents != null)
            combatEvents.OnRangedHit -= HandleRangedHit;
        if (upgradeManager != null)
            upgradeManager.OnStatUpgradeLevelChanged -= HandleStatUpgrade;
    }

    // 폭발탄 강화 구매 시 — 쿨다운 시작(최초 1회만, 레벨업은 무시)
    private void HandleStatUpgrade(StatType statType, int level)
    {
        if (statType != StatType.ExplosiveRounds)
            return;
        if (nextExplosiveTime == float.MaxValue)
            nextExplosiveTime = Time.time + stats.ExplosiveInterval;
    }

    
     // 원거리 명중 시 — 쿨다운 충족이면 폭발하고 쿨다운 재설정
    private void HandleRangedHit(Vector3 point)
    {
        if (stats.ExplosiveDamage <= 0f || Time.time < nextExplosiveTime)
            return;

        if (stats.ExplosionVFX != null)
            Destroy(Instantiate(stats.ExplosionVFX, point, Quaternion.identity), 3f);

        var hitSet = new HashSet<IDamageable>();
        Collider[] cols = Physics.OverlapSphere(point, stats.ExplosiveRadius);
        foreach (var col in cols)
        {
            var target = col.GetComponentInParent<IDamageable>();
            if (target == null || hitSet.Contains(target))
                continue;
            if (target is CharacterHealth)
                continue; // 폭발탄: 플레이어는 자해 데미지 제외
            target.TakeDamage(stats.ExplosiveDamage);
            damageCalc?.ReportDamage(stats.ExplosiveDamage); // totalDamage 집계에 폭발 피해 포함
            (target as EnemyHealth)?.SpawnBloodAt(col.bounds.center);
            hitSet.Add(target);
        }

        nextExplosiveTime = Time.time + stats.ExplosiveInterval;
    }
}
