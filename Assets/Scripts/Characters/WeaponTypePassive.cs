using System.Collections.Generic;
using UnityEngine;

// 총기 종류별 고유 패시브의 해금 상태 + 정책 단일 보관소 (플레이어 컴포넌트).
// 적/무기/탄약을 직접 건드리지 않는다 — 행동은 RangedWeapon이 한다.
public class WeaponTypePassive : MonoBehaviour
{
    [Header("HG 처형 HP비율")]
    [SerializeField, Range(0f, 1f)] private float hgExecuteThreshold = 0.1f;

    [Header("SG 근거리 추가 피해")]
    [SerializeField] private float sgMaxBonus = 0.5f;      // 최대 추가 피해 (0.5 = +50%)
    [SerializeField] private float sgFullBonusRange = 10f; // 이 거리까지는 풀 보너스
    [SerializeField] private float sgFalloffEnd = 20f;     // 이 거리에서 보너스 0 (그 사이 선형 감쇠)

    [Header("AR 관통 레이어 ")]
    [SerializeField] private LayerMask pierceEnemyMask; // 관통이 훑을 대상 (Enemy + Head)
    [SerializeField] private LayerMask pierceStopMask;  // 닿으면 관통 정지 (Invisible Wall)
    public LayerMask PierceEnemyMask => pierceEnemyMask;
    public LayerMask PierceStopMask => pierceStopMask;

    private readonly HashSet<WeaponType> unlocked = new HashSet<WeaponType>();

    public bool IsUnlocked(WeaponType type) => unlocked.Contains(type);
    public void Unlock(WeaponType type) => unlocked.Add(type);

    // HG 처형 — HG 해금 + 적 HP 비율이 임계 이하면 즉사. 그 외 false.
    public bool ShouldExecute(WeaponType type, float targetHealthRatio)
        => type == WeaponType.HG && unlocked.Contains(type) && targetHealthRatio <= hgExecuteThreshold;

    // SG 근거리 보너스 — weaponMultiplier에 곱할 배율. 잠김/비매칭=1.
    // (SR 헤드샷 증폭은 PlayerDamageCalculator가 처리 — 헤드샷 수치 일원화)
    public float GetDamageMultiplier(WeaponType type, float hitDistance)
    {
        float mul = 1f;
        if (!unlocked.Contains(type))
            return mul; // 잠김 = 무효값(1) 반환
        switch (type)
        {
            case WeaponType.SG:
                if (hitDistance <= sgFullBonusRange)
                    return mul + sgMaxBonus;          // 근거리 밴드: 풀 보너스
                if (hitDistance >= sgFalloffEnd)
                    return mul;                        // 사거리 밖: 보너스 없음
                // 밴드~끝: 선형 감쇠 (이 구간은 sgFalloffEnd > sgFullBonusRange일 때만 실행 → 0나눗셈 없음)
                float fade = mul - (hitDistance - sgFullBonusRange) / (sgFalloffEnd - sgFullBonusRange);
                return mul + sgMaxBonus * fade;
            default:
                return mul; // 그 외: 보너스 없음 (잠김과 동일한 거동)
        }
    }

    // AR 관통 — 해금 시 벽 만날 때까지 적 무제한 관통 (마릿수 제한·감쇠 없음). 그 외 false.
    public bool Pierces(WeaponType type) => type == WeaponType.AR && unlocked.Contains(type);

    // SMG 예비탄 우선 — 해금 시 예비탄(reserve)부터 소모, 0이 되면 탄창 사용. 그 외 false.
    public bool UsesReserveAmmoFirst(WeaponType type) => type == WeaponType.SMG && unlocked.Contains(type);
}
