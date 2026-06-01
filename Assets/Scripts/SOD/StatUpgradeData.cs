using UnityEngine;

[CreateAssetMenu(fileName = "StatUpgradeData", menuName = "Upgrade/Stat Upgrade")]
public class StatUpgradeData : UpgradeData
{
    public StatType statType;
    public float bonusPerLevel;
}
