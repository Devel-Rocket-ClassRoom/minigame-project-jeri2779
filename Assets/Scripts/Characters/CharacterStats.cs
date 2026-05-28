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

    public float MaxHealth => maxHealth + hpBonus;
    public float MoveSpeed => moveSpeed + mspBonus;
    public float MaxStamina => maxStamina + staminaBonus;
    public float StaminaDrainRate => staminaDrainRate;
    public float StaminaRegenRate => staminaRegenRate + staminaRegenBonus;
    public float AttackMultiplier => attackMultiplier + atkBonus;
    public float BaseAttackMultiplier => attackMultiplier;
    // 1.0 기본값. 강화 시 1.2, 1.4 ... 형태로 증가 → reloadTime / ReloadSpeedMultiplier 로 사용
    public float ReloadSpeedMultiplier => 1f + reloadSpeedBonus;

    public void ApplyAtkBonus(float amount) => atkBonus += amount;
    public void ApplyHpBonus(float amount) => hpBonus += amount;
    public void ApplyMspBonus(float amount) => mspBonus += amount;
    public void ApplyStaminaBonus(float amount) => staminaBonus += amount;
    public void ApplyStaminaRegenBonus(float amount) => staminaRegenBonus += amount;
    public void ApplyReloadSpeedBonus(float amount) => reloadSpeedBonus += amount;
}
