using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private CharacterStats characterStats;
    [SerializeField] private CharacterHealth characterHealth;
    [SerializeField] private RewardController rewardController;

    [SerializeField] private float atkUpgradePercent = 0.2f;
    [SerializeField] private float hpUpgradeAmount = 100f;
    [SerializeField] private int upgradePrice = 3000;

    public int AtkLevel { get; private set; }
    public int HpLevel { get; private set; }

    public void UpgradeAttack()
    {
        if (!rewardController.SpendMoney(upgradePrice))
        {
            Debug.Log("돈 부족");
            return;
        }
        float beforeMul = characterStats.AttackMultiplier;
        characterStats.ApplyAtkBonus(characterStats.BaseAttackMultiplier * atkUpgradePercent);
        Debug.Log($"공격 배율 {beforeMul:F2} -> {characterStats.AttackMultiplier:F2}");
        AtkLevel++;
    }

    public void UpgradeHp()
    {
 
         if (!rewardController.SpendMoney(upgradePrice))
          {
                Debug.Log("돈 부족");
                return;
          }
        characterStats.ApplyHpBonus(hpUpgradeAmount);
        characterHealth.AddHealth(hpUpgradeAmount);
        HpLevel++;
    }
}

