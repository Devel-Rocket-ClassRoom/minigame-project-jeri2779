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

    public float MaxHealth => maxHealth + hpBonus;
    public float MoveSpeed => moveSpeed + mspBonus;
    public float MaxStamina => maxStamina + staminaBonus;
    public float StaminaDrainRate => staminaDrainRate;
    public float StaminaRegenRate => staminaRegenRate;
    public float AttackMultiplier => attackMultiplier + atkBonus;
    public float BaseAttackMultiplier => attackMultiplier;

    public void ApplyAtkBonus(float amount) => atkBonus += amount;
    public void ApplyHpBonus(float amount) => hpBonus += amount;
    public void ApplyMspBonus(float amount) => mspBonus += amount;
    public void ApplyStaminaBonus(float amount) => staminaBonus += amount;
}
