using UnityEngine;

// 상태 기반 피해 계산의 단일 집결지.  
// 무기는 상황(DamageContext)만 넘기고, 최종 피해 = 기본 × 스탯 × 헤드샷/치명타
[RequireComponent(typeof(CharacterHealth))]
public class PlayerDamageCalculator : MonoBehaviour
{
    [SerializeField]
    private float headshotMultiplier = 2f;

    [SerializeField]
    private float critMultiplier = 2f;

    [Header("최후저항 (HP 비율이 임계 이하면 피해 증가)")]
    [SerializeField, Range(0f, 1f)]
    private float lastStandThreshold = 0.3f; // 이 HP 비율 이하에서 발동

    [SerializeField]
    private float lastStandDamageBonus = 0f; // 발동 시 추가 배율 (0 = 비활성, 0.5 = +50%)

    [Header("건강한 적 추가뎀 (타겟 HP 높을수록) — 수치는 업그레이드, 곡선만 여기")]
    [SerializeField, Range(0f, 1f)]
    private float healthyTargetFalloff = 0.5f; // 이 타겟 HP 비율 이하에선 추가뎀 0

    [Header("투지 (생존 적 수 많을수록) — 수치는 업그레이드, 곡선만 여기")]
    [SerializeField]
    private int fightingSpiritFullCount = 10; // 이 생존 수 이상에서 최대 보너스

    private CharacterStats stats;
    private CharacterHealth health;

    private void Awake()
    {
        stats = GetComponent<CharacterStats>();
        health = GetComponent<CharacterHealth>();
    }

    public float Compute(in DamageContext ctx)
    {
        float dmg = ctx.baseDamage * stats.AttackMultiplier * ctx.weaponMultiplier;

        if (ctx.isMelee)
            dmg *= stats.MeleeDamageMultiplier;
        if (ctx.isHeadshot)
            dmg *= headshotMultiplier;
        if (Random.value < stats.CritChance)
            dmg *= critMultiplier;

        // ── 상태 기반 조건부 보너스 풀 (덧셈으로 누적 후 한 번 곱) ──
        float conditional = 0f;

        // 최후저항: 플레이어 HP 비율이 임계 이하면 발동 (인스펙터 튜닝형)
        if (lastStandDamageBonus > 0f)
        {
            float maxHealth = stats.MaxHealth;
            float playerHpRatio = maxHealth > 0f ? health.CurrentHealth / maxHealth : 1f;
            if (playerHpRatio <= lastStandThreshold)
                conditional += lastStandDamageBonus;
        }

        // 건강한 적: 타겟 HP 높을수록 추가뎀 (업그레이드형). 100%→최대, falloff 이하→0
        if (stats.HealthyTargetBonus > 0f)
            conditional += stats.HealthyTargetBonus * Mathf.InverseLerp(healthyTargetFalloff, 1f, ctx.targetHealthRatio);

        // 투지: 생존 적 수 많을수록 추가뎀 (업그레이드형). 만원→최대, 0마리→0.
        // bonus>0 먼저 검사 → 미구매 시 레지스트리 접근 안 함(빈 풀 보존).
        if (stats.FightingSpiritBonus > 0f)
            conditional += stats.FightingSpiritBonus * Mathf.Clamp01((float)EnemyRegistry.AliveCount / fightingSpiritFullCount);

        dmg *= 1f + conditional;
        return dmg;
    }
}
