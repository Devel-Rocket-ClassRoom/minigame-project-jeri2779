using System.Collections.Generic;
using UnityEngine;

public class UpgradeBoardUI : MonoBehaviour
{
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private Transform itemContainer;  // UpgradePanel: 두 탭의 StatUpgradeItem 전부 탐색
    [SerializeField] private Transform rowContainer;   // Upgrading: Row들이 동적 생성되는 컨테이너
    [SerializeField] private GameObject rowPrefab;

    private StatUpgradeItem[] items;
    private UpgradeBoardRow[] rows;

    private void OnEnable()
    {
        upgradeManager.OnStatUpgradeLevelChanged += HandleStatUpgraded;
        upgradeManager.OnUpgradeLevelChanged += HandleAtkHpUpgraded;
        BuildRows();
        Refresh();
    }

    private void OnDisable()
    {
        upgradeManager.OnStatUpgradeLevelChanged -= HandleStatUpgraded;
        upgradeManager.OnUpgradeLevelChanged -= HandleAtkHpUpgraded;
    }

    private void BuildRows()
    {
        items = itemContainer.GetComponentsInChildren<StatUpgradeItem>(true);
        int needed = items.Length + 2; // +2: 공격력, 체력

        if (rows != null && rows.Length == needed) return;

        var existing = new List<GameObject>();
        foreach (Transform child in rowContainer)
            existing.Add(child.gameObject);
        foreach (var go in existing)
            Destroy(go);

        rows = new UpgradeBoardRow[needed];
        for (int i = 0; i < needed; i++)
        {
            var go = Instantiate(rowPrefab, rowContainer);
            rows[i] = go.GetComponent<UpgradeBoardRow>();
        }
    }

    private void HandleStatUpgraded(StatType _, int __) => Refresh();

    private void HandleAtkHpUpgraded(int _, int __) => Refresh();

    private void Refresh()
    {
        if (items == null || rows == null) return;

        for (int i = 0; i < items.Length; i++)
        {
            int level = upgradeManager.GetLevel(items[i].StatType);
            if (level > 0)
                rows[i].Show(items[i].DisplayName, level, FormatEffect(items[i], level));
            else
                rows[i].Hide();
        }

        int atkIdx = items.Length;
        if (upgradeManager.AtkLevel > 0)
            rows[atkIdx].Show("공격력", upgradeManager.AtkLevel, $"+{upgradeManager.AtkUpgradePercent * 100f * upgradeManager.AtkLevel:F0}%");
        else
            rows[atkIdx].Hide();

        int hpIdx = atkIdx + 1;
        if (upgradeManager.HpLevel > 0)
            rows[hpIdx].Show("체력", upgradeManager.HpLevel, $"+{upgradeManager.HpUpgradeAmount * upgradeManager.HpLevel:F0}");
        else
            rows[hpIdx].Hide();
    }

    // 현황판 효과 문구. 대부분 % (displayMaxPercent 기준), 베이스없는 능력은 실수치, 더블점프는 해금.
    private string FormatEffect(StatUpgradeItem item, int level)
    {
        float flat = item.BonusPerLevel * level; // 누적 실수치 (회복/흡혈/폭발)
        switch (item.StatType)
        {
            case StatType.HealthRegen:     return $"{flat:F0}HP/초";
            case StatType.Lifesteal:       return $"{flat:F0}회복";
            case StatType.ExplosiveRounds: return $"{flat:F0}대미지";
            case StatType.DoubleJump:      return "해금";
            default:
                if (item.MaxLevel <= 0) return string.Empty;
                float pct = item.DisplayMaxPercent * level / item.MaxLevel;
                return $"{(item.IsMaxBonus ? "최대": "")}+{pct:F0}%";
        }
    }
}
