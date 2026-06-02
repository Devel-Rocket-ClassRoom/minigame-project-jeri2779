using System;
using System.Collections.Generic;
using UnityEngine;

// 상점에서 구매 가능한 공통 스탯 종류
public enum StatType
{
    MoveSpeed,
    MaxStamina,
    StaminaRegen,
    ReloadSpeed,
    JumpHeight,
    DamageReduction,
    FireRate,
    Range,
    MoneyGain,
    HealthRegen,
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
    public int UpgradePrice => upgradePrice;

 
    public event Action<int, int> OnUpgradeLevelChanged;
    public event Action<StatType, int> OnStatUpgradeLevelChanged;   

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
        OnUpgradeLevelChanged?.Invoke(AtkLevel, HpLevel);
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
        OnUpgradeLevelChanged?.Invoke(AtkLevel, HpLevel);
    }

    // ─── 공통 스탯 강화 ───────────────────────────────────────────────────

    /// <summary>현재 스탯 레벨 반환 (미강화 시 0)</summary>
    public int GetLevel(StatType statType)
    {
        return statLevels.TryGetValue(statType, out int level) ? level : 0;
    }

  
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
            {
                float before = characterStats.MoveSpeed;
                characterStats.ApplyMspBonus(bonusPerLevel);
                Debug.Log($"이동속도 {before:F2} -> {characterStats.MoveSpeed:F2}");
                break;
            }
            case StatType.MaxStamina:
            {
                float before = characterStats.MaxStamina;
                characterStats.ApplyStaminaBonus(bonusPerLevel);
                characterMoves.AddStamina(bonusPerLevel);
                Debug.Log($"최대 스태미너 {before:F2} -> {characterStats.MaxStamina:F2}");
                break;
            }
            case StatType.StaminaRegen:
            {
                float before = characterStats.StaminaRegenRate;
                characterStats.ApplyStaminaRegenBonus(bonusPerLevel);
                Debug.Log($"스태미너 회복 {before:F2} -> {characterStats.StaminaRegenRate:F2}");
                break;
            }
            case StatType.ReloadSpeed:
            {
                float before = characterStats.ReloadSpeedMultiplier;
                characterStats.ApplyReloadSpeedBonus(bonusPerLevel);
                Debug.Log($"재장전 배율 {before:F2} -> {characterStats.ReloadSpeedMultiplier:F2}");
                break;
            }
            case StatType.JumpHeight:
            {
                float before = characterStats.JumpHeightBonus;
                characterStats.ApplyJumpHeightBonus(bonusPerLevel);
                Debug.Log($"점프 보너스 {before:F2} -> {characterStats.JumpHeightBonus:F2}");
                break;
            }
            case StatType.DamageReduction:
            {
                float before = characterStats.DamageReduction;
                characterStats.ApplyDamageReductionBonus(bonusPerLevel);
                Debug.Log($"피해 감소 {before:P0} -> {characterStats.DamageReduction:P0}");
                break;
            }
            case StatType.FireRate:
            {
                float before = characterStats.FireRateMultiplier;
                characterStats.ApplyFireRateBonus(bonusPerLevel);
                Debug.Log($"연사 배율 {before:F2} -> {characterStats.FireRateMultiplier:F2}");
                break;
            }
            case StatType.Range:
            {
                float before = characterStats.RangeBonus;
                characterStats.ApplyRangeBonus(bonusPerLevel);
                Debug.Log($"사거리 보너스 {before:F2} -> {characterStats.RangeBonus:F2}");
                break;
            }
            case StatType.MoneyGain:
            {
                float before = characterStats.MoneyGainMultiplier;
                characterStats.ApplyMoneyGainBonus(bonusPerLevel);
                Debug.Log($"재화 배율 {before:F2} -> {characterStats.MoneyGainMultiplier:F2}");
                break;
            }
        }

        Debug.Log($"{statType} 강화 완료 → Lv {statLevels[statType]}/{maxLevel}");
        OnStatUpgradeLevelChanged?.Invoke(statType, statLevels[statType]);
        return true;
    }
}
