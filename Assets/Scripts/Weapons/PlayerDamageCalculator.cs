using System;
using UnityEngine;
using Random = UnityEngine.Random;

// 상태 기반 피해 계산의 단일 집결지.
[RequireComponent(typeof(CharacterHealth))]
public class PlayerDamageCalculator : MonoBehaviour
{
    // 대미지 적용 시 발행 (최종 대미지, 헤드샷 여부). HUD/데미지팝업 등이 구독.
    public event Action<float, bool> OnDamageDealt;

    // 이번 판 누적 통계 (씬 리로드 시 컴포넌트 재생성으로 자동 0). 끝화면이 읽음.
    public float TotalDamageDealt { get; private set; }
    public int HeadshotCount { get; private set; }

    [SerializeField]
    private float headshotMultiplier = 2f;

    [SerializeField]
    private float srHeadshotMultiplier = 5f; // SR 해금 시 헤드샷 최종 배율 (기존 ×2 대신)

    [SerializeField]
    private float critMultiplier = 2f;

    [Header("HP 비율 낮을수록 대미지 증가")]
    [SerializeField, Range(0f, 1f)]
    private float lastStandThreshold = 0.3f; // 이 HP 비율 이하에서 발동

    [Header("최대HP 적 추가 대미지 ")]
    [SerializeField, Range(0f, 1f)]
    private float healthyTargetFalloff = 0.5f; // 이 타겟 HP 비율 이하에선 추가뎀 0

    [Header("생존적 수 비례 대미지 증가")]
    [SerializeField]
    private int fightingSpiritFullCount = 10; // 이 생존 수 이상에서 최대 보너스

    private CharacterStats stats;
    private CharacterHealth health;
    private WeaponTypePassive passive;

    private void Awake()
    {
        stats = GetComponent<CharacterStats>();
        health = GetComponent<CharacterHealth>();
        passive = GetComponent<WeaponTypePassive>();
    }

    public float Compute(in DamageContext ctx)
    {
        float dmg = ctx.baseDamage * stats.AttackMultiplier * ctx.weaponMultiplier;

        if (ctx.isMelee)
            dmg *= stats.MeleeDamageMultiplier;
        if (ctx.isHeadshot)
        {
            HeadshotCount++;
            dmg *= (ctx.weaponType == WeaponType.SR && passive != null && passive.IsUnlocked(WeaponType.SR))
                ? srHeadshotMultiplier
                : headshotMultiplier;
        }
        if (Random.value < stats.CritChance)
            dmg *= critMultiplier;

        // ── 상태 기반 조건부 보너스 풀 (덧셈으로 누적 후 한 번 곱) ──
        float conditional = 0f;

        // 최후저항: 플레이어 HP 비율이 임계 이하면 발동 (수치는 업그레이드, 임계만 여기)
        if (stats.LastStandBonus > 0f)
        {
            float maxHealth = stats.MaxHealth;
            float playerHpRatio = maxHealth > 0f ? health.CurrentHealth / maxHealth : 1f;
            if (playerHpRatio <= lastStandThreshold)
                conditional += stats.LastStandBonus;
        }

        // 건강한 적: 타겟 HP 높을수록 추가뎀 (업그레이드형). 100%→최대, falloff 이하→0
        if (stats.HealthyTargetBonus > 0f)
            conditional += stats.HealthyTargetBonus * Mathf.InverseLerp(healthyTargetFalloff, 1f, ctx.targetHealthRatio);

        // 투지: 생존 적 수 많을수록 추가뎀 (업그레이드형). 만원→최대, 0마리→0.
        if (stats.FightingSpiritBonus > 0f)
            conditional += stats.FightingSpiritBonus * Mathf.Clamp01((float)EnemyRegistry.AliveCount / fightingSpiritFullCount);

        dmg *= 1f + conditional;
        return dmg;
    }

    // 대미지 적용 후 호출 — HUD/팝업에 알림. 헤드샷 여부는 색 구분용(폭발/근접 등 기본 false).
    public void ReportDamage(float damage, bool isHeadshot = false)
    {
        TotalDamageDealt += damage;
        OnDamageDealt?.Invoke(damage, isHeadshot);
    }
}
