using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [SerializeField] float maxHealth = 1000f;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float staminaDrainRate = 20f;
    [SerializeField] float staminaRegenRate = 10f;
    [SerializeField] float attackMultiplier = 1f;

    private float hpBonus = 0f;
    private float atkBonus = 0f;
    private float mspBonus = 0f;
    private float staminaBonus = 0f;
    private float staminaRegenBonus = 0f;
    private float reloadSpeedBonus = 0f;
    private float jumpHeightBonus = 0f;
    private float damageReductionBonus = 0f;
    private float fireRateBonus = 0f;
    private float rangeBonus = 0f;
    private float moneyGainBonus = 0f;
    private int extraJumpCount = 0;
    private float critChanceBonus = 0f;
    private float bonusAmmoBonus = 0f;
    private float meleeDamageBonus = 0f;
    private float healthyTargetBonus = 0f;
    private float fightingSpiritBonus = 0f;
    private float healthRegenBonus = 0f;

    [Header("폭발탄")]
    [SerializeField] private GameObject explosionVFX;
    [SerializeField] private float explosiveInterval = 8f;
    [SerializeField] private float explosiveRadius = 3f;
    private float explosiveDamage;

    [Header("흡혈")]
    [SerializeField] private int lifestealKillThreshold = 5;
    private float lifestealHealAmount;

    public float MaxHealth => maxHealth + hpBonus;
    public float MoveSpeed => moveSpeed + mspBonus;
    public float MaxStamina => maxStamina + staminaBonus;
    public float StaminaDrainRate => staminaDrainRate;
    public float StaminaRegenRate => staminaRegenRate + staminaRegenBonus;
    public float AttackMultiplier => attackMultiplier + atkBonus;
    public float BaseAttackMultiplier => attackMultiplier;
    public float ReloadSpeedMultiplier => 1f + reloadSpeedBonus;
    // 모두 Inspector의 bonusPerLevel을 "퍼센트"로 해석 (5 → 5%)
    // JumpHeight만 1/10 스케일: 5 → +0.5 유닛
    public float JumpHeightBonus => jumpHeightBonus * 0.1f;
    public float DamageReduction => Mathf.Min(damageReductionBonus * 0.01f, 0.75f);
    public float FireRateMultiplier => 1f + fireRateBonus * 0.01f;
    public float RangeBonus => rangeBonus;
    public float MoneyGainMultiplier => 1f + moneyGainBonus * 0.01f;
    // 폭발탄 수치: 피해/반경/간격/VFX. 쿨다운·폭발 실행은 PlayerExplosive가 한다.
    public float ExplosiveDamage => explosiveDamage;
    public float ExplosiveRadius => explosiveRadius;
    public float ExplosiveInterval => explosiveInterval;
    public GameObject ExplosionVFX => explosionVFX;

    public int ExtraJumpCount => extraJumpCount;
    public float CritChance => Mathf.Clamp01(critChanceBonus * 0.01f);
    public float BonusAmmoPercent => bonusAmmoBonus * 0.01f;
    public float MeleeDamageMultiplier => 1f + meleeDamageBonus * 0.01f;
    // 건강한 적 추가뎀의 "최대 보너스"(타겟 HP 100%일 때). 적용 곡선은 PlayerDamageCalculator가 결정.
    public float HealthyTargetBonus => healthyTargetBonus * 0.01f;
    // 투지: 생존 적 수가 많을수록 추가뎀의 "최대 보너스"(만원일 때). 곡선은 PlayerDamageCalculator가 결정.
    public float FightingSpiritBonus => fightingSpiritBonus * 0.01f;
    // 자동 체력 회복: 초당 회복량(절대값). 회복 틱은 CharacterHealth가 한다.
    public float HealthRegenRate => healthRegenBonus;
    // 흡혈 수치: 1회 회복량 / 발동 처치 임계. 행동(카운트·회복)은 PlayerLifesteal이 한다.
    public float LifestealHealAmount => lifestealHealAmount;
    public int LifestealKillThreshold => lifestealKillThreshold;

    public void ApplyAtkBonus(float amount) => atkBonus += amount;
    public void ApplyHpBonus(float amount) => hpBonus += amount;
    public void ApplyMspBonus(float amount) => mspBonus += amount;
    public void ApplyStaminaBonus(float amount) => staminaBonus += amount;
    public void ApplyStaminaRegenBonus(float amount) => staminaRegenBonus += amount;
    public void ApplyReloadSpeedBonus(float amount) => reloadSpeedBonus += amount;
    public void ApplyJumpHeightBonus(float amount) => jumpHeightBonus += amount;
    public void ApplyDamageReductionBonus(float amount) => damageReductionBonus += amount;
    public void ApplyFireRateBonus(float amount) => fireRateBonus += amount;
    public void ApplyRangeBonus(float amount) => rangeBonus += amount;
    public void ApplyMoneyGainBonus(float amount) => moneyGainBonus += amount;
    public void AddExtraJump() => extraJumpCount++;
    public void ApplyCritChanceBonus(float amount) => critChanceBonus += amount;
    public void ApplyBonusAmmoBonus(float amount) => bonusAmmoBonus += amount;
    public void ApplyMeleeDamageBonus(float amount) => meleeDamageBonus += amount;
    public void ApplyHealthyTargetBonus(float amount) => healthyTargetBonus += amount;
    public void ApplyFightingSpiritBonus(float amount) => fightingSpiritBonus += amount;
    public void ApplyHealthRegenBonus(float amount) => healthRegenBonus += amount;

    public void ApplyExplosiveDamageBonus(float amount) => explosiveDamage += amount;

    public void ApplyLifestealBonus(float amount) => lifestealHealAmount += amount;
}
