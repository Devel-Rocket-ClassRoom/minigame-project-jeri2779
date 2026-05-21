using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private CharacterStats characterStats;
    [SerializeField] private CharacterHealth characterHealth;
    [SerializeField] private CharacterShooter characterShooter;

    [SerializeField] private float atkUpgradePercent = 0.2f;
    [SerializeField] private float hpUpgradeAmount = 100f;

    public int AtkLevel { get; private set; }
    public int HpLevel { get; private set; }

    public void UpgradeAttack()
    {
        float before = characterShooter.CurrentDamage;
        characterStats.ApplyAtkBonus(characterStats.BaseAttackMultiplier * atkUpgradePercent);
        Debug.Log($"{before:F1} -> {characterShooter.CurrentDamage:F1}");
        AtkLevel++;
    }

    public void UpgradeHp()
    {
        characterStats.ApplyHpBonus(hpUpgradeAmount);
        characterHealth.AddHealth(hpUpgradeAmount);
        HpLevel++;
    }
}

