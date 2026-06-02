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

    [Header("폭발탄")]
    [SerializeField] private GameObject explosionVFX;
    [SerializeField] private float explosiveInterval = 8f;
    [SerializeField] private float explosiveRadius = 3f;
    private float explosiveDamage;
    private float nextExplosiveTime = float.MaxValue;

    [Header("흡혈")]
    [SerializeField] private int lifestealKillThreshold = 5;
    private float lifestealHealAmount;
    private int killCounter;
    private CharacterHealth health;

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
    public bool IsExplosiveReady => explosiveDamage > 0f && Time.time >= nextExplosiveTime;
    public float ExplosiveDamage => explosiveDamage;
    public float ExplosiveRadius => explosiveRadius;
    public GameObject ExplosionVFX => explosionVFX;

    public int ExtraJumpCount => extraJumpCount;
    public float CritChance => Mathf.Clamp01(critChanceBonus * 0.01f);
    public float BonusAmmoPercent => bonusAmmoBonus * 0.01f;
    public float MeleeDamageMultiplier => 1f + meleeDamageBonus * 0.01f;

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

    public void ConsumeExplosive() => nextExplosiveTime = Time.time + explosiveInterval;
    public void ApplyExplosiveDamageBonus(float amount)
    {
        explosiveDamage += amount;
        if (nextExplosiveTime == float.MaxValue)
            nextExplosiveTime = Time.time + explosiveInterval;
    }

    public void ApplyLifestealBonus(float amount) => lifestealHealAmount += amount;
    public void RegisterKill()
    {
        if (lifestealHealAmount <= 0f) return;
        killCounter++;
        if (killCounter >= lifestealKillThreshold)
        {
            if (health == null) health = GetComponent<CharacterHealth>();
            health?.AddHealth(lifestealHealAmount);
            killCounter = 0;
        }
    }
}
