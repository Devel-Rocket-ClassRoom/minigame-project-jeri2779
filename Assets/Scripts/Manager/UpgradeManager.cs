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
    DoubleJump,
    CritChance,
    BonusAmmo,
    MeleeDamage,
    ExplosiveRounds,
    Lifesteal,
    HealthyTargetDamage,
    FightingSpirit,
    LastStand,
}

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private CharacterStats characterStats;
    [SerializeField] private CharacterHealth characterHealth;
    [SerializeField] private CharacterMoves characterMoves;

    [SerializeField] private float atkUpgradePercent = 0.2f;
    [SerializeField] private float hpUpgradeAmount = 100f;
    [SerializeField] private int upgradePrice = 3000;

    public int AtkLevel { get; private set; }
    public int HpLevel { get; private set; }
    public int UpgradePrice => upgradePrice;
    public float AtkUpgradePercent => atkUpgradePercent;
    public float HpUpgradeAmount => hpUpgradeAmount;

    public event Action<int, int> OnUpgradeLevelChanged;
    public event Action<StatType, int> OnStatUpgradeLevelChanged;

    private readonly Dictionary<StatType, int> statLevels = new Dictionary<StatType, int>();

    // ─── 기존 ATK / HP 강화 ───────────────────────────────────────────────

    // 결제는 ShopController가 한다. 여기서는 적용만.
    public void UpgradeAttack()
    {
        characterStats.ApplyAtkBonus(characterStats.BaseAttackMultiplier * atkUpgradePercent);
        AtkLevel++;
        OnUpgradeLevelChanged?.Invoke(AtkLevel, HpLevel);
    }

    public void UpgradeHp()
    {
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

    // 결제는 ShopController가 한다. 여기서는 레벨 증가 + 적용만.
    public bool Upgrade(StatType statType, float bonusPerLevel, int maxLevel)
    {
        if (GetLevel(statType) >= maxLevel)
            return false;

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
            case StatType.JumpHeight:
                characterStats.ApplyJumpHeightBonus(bonusPerLevel);
                break;
            case StatType.DamageReduction:
                characterStats.ApplyDamageReductionBonus(bonusPerLevel);
                break;
            case StatType.FireRate:
                characterStats.ApplyFireRateBonus(bonusPerLevel);
                break;
            case StatType.Range:
                characterStats.ApplyRangeBonus(bonusPerLevel);
                break;
            case StatType.MoneyGain:
                characterStats.ApplyMoneyGainBonus(bonusPerLevel);
                break;
            case StatType.DoubleJump:
                characterStats.AddExtraJump();
                break;
            case StatType.CritChance:
                characterStats.ApplyCritChanceBonus(bonusPerLevel);
                break;
            case StatType.BonusAmmo:
                characterStats.ApplyBonusAmmoBonus(bonusPerLevel);
                break;
            case StatType.MeleeDamage:
                characterStats.ApplyMeleeDamageBonus(bonusPerLevel);
                break;
            case StatType.ExplosiveRounds:
                characterStats.ApplyExplosiveDamageBonus(bonusPerLevel);
                break;
            case StatType.Lifesteal:
                characterStats.ApplyLifestealBonus(bonusPerLevel);
                break;
            case StatType.HealthyTargetDamage:
                characterStats.ApplyHealthyTargetBonus(bonusPerLevel);
                break;
            case StatType.FightingSpirit:
                characterStats.ApplyFightingSpiritBonus(bonusPerLevel);
                break;
            case StatType.LastStand:
                characterStats.ApplyLastStandBonus(bonusPerLevel);
                break;
        }

        OnStatUpgradeLevelChanged?.Invoke(statType, statLevels[statType]);
        return true;
    }
}
