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
}
