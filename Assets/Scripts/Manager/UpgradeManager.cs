using System.Collections.Generic;
using UnityEngine;

// 상점에서 구매 가능한 공통 스탯 종류
public enum StatType
{
    MoveSpeed,
    MaxStamina,
    StaminaRegen,
    ReloadSpeed,
}

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private CharacterStats characterStats;
    [SerializeField] private CharacterHealth characterHealth;
    [SerializeField] private CharacterMoves characterMoves;
    [SerializeField] private RewardController rewardController;

    [SerializeField] private float atkUpgradePercent = 0.2f;
    [SerializeField] private float hpUpgradeAmount = 100f;
    [SerializeField] private int upgradePrice = 3000;

    public int AtkLevel { get; private set; }
    public int HpLevel { get; private set; }

    private readonly Dictionary<StatType, int> statLevels = new Dictionary<StatType, int>();

    // ─── 기존 ATK / HP 강화 (현행 유지) ───────────────────────────────────

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

    // ─── 공통 스탯 강화 ───────────────────────────────────────────────────

    /// <summary>현재 스탯 레벨 반환 (미강화 시 0)</summary>
    public int GetLevel(StatType statType)
    {
        return statLevels.TryGetValue(statType, out int level) ? level : 0;
    }

    /// <summary>
    /// 스탯 강화 시도. 재화 부족 또는 최대 레벨 도달 시 false 반환.
    /// </summary>
    public bool Upgrade(StatType statType, int price, float bonusPerLevel, int maxLevel)
    {
        if (GetLevel(statType) >= maxLevel)
        {
            Debug.Log($"{statType} 최대 레벨 도달");
            return false;
        }

        if (!rewardController.SpendMoney(price))
        {
            Debug.Log("돈 부족");
            return false;
        }

        if (!statLevels.ContainsKey(statType))
            statLevels[statType] = 0;
        statLevels[statType]++;

        switch (statType)
        {
            case StatType.MoveSpeed:
                characterStats.ApplyMspBonus(bonusPerLevel);
                break;
            case StatType.MaxStamina:
                characterStats.ApplyStaminaBonus(bonusPerLevel);
                characterMoves.AddStamina(bonusPerLevel);
                break;
            case StatType.StaminaRegen:
                characterStats.ApplyStaminaRegenBonus(bonusPerLevel);
                break;
            case StatType.ReloadSpeed:
                characterStats.ApplyReloadSpeedBonus(bonusPerLevel);
                break;
        }

        Debug.Log($"{statType} 강화 완료 → Lv {statLevels[statType]}/{maxLevel}");
        return true;
    }
}
