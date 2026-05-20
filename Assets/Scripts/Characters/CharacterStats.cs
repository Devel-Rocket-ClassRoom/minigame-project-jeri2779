using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [SerializeField] float maxHealth = 1000f;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float staminaDrainRate = 20f;
    [SerializeField] float staminaRegenRate = 10f;
    [SerializeField] float attackMultiplier = 1f;

    public float MaxHealth => maxHealth;
    public float MoveSpeed => moveSpeed;
    public float MaxStamina => maxStamina;
    public float StaminaDrainRate => staminaDrainRate;
    public float StaminaRegenRate => staminaRegenRate;
    public float AttackMultiplier => attackMultiplier;
    public int UpgradeAtkLevel => upgradeAtkLevel;
    public int UpgradeHpLevel => upgradeHpLevel;
    public int UpgradeMspLevel => upgradeMspLevel;
    public int UpgradeStaminaLevel => upgradeStmLevel;
    private int upgradeAtkLevel = 0;
    private int upgradeHpLevel = 0;
    private int upgradeMspLevel = 0;
    private int upgradeStmLevel = 0;

    public void UpgradeHp(float amount)
    {
        maxHealth += amount;
        upgradeHpLevel++;
    
    }
    public void UpgradeMoveSpeed(float amount)
    {
        moveSpeed += amount;
        upgradeMspLevel++;
      
    }
    public void UpgradeStamina(float amount)
    {
        maxStamina += amount;
        upgradeStmLevel++;
    }
    public void UpgradeAttack( )
    {
        attackMultiplier *= 1.2f;
        upgradeAtkLevel++;
    }
}