using TMPro;
using UnityEngine;

/// <summary>
/// UpgradeBoard 행 1개. Show/Hide로 표시 여부를 제어한다.
/// </summary>
public class UpgradeBoardRow : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text valueText;

    public void Show(string displayName, int level, float bonusPerLevel, bool isPercentage = false, bool isMaxBonus = false)
    {
        gameObject.SetActive(true);
        if (nameText != null) nameText.text = displayName;
        if (levelText != null) levelText.text = $"Lv.{level}";
        if (valueText != null)
        {
            string prefix = isMaxBonus ? "최대 " : "";
            if (bonusPerLevel <= 0f)
                valueText.text = string.Empty;
            else if (isPercentage)
                valueText.text = $"{prefix}+{bonusPerLevel * level:F0}%";
            else
                valueText.text = $"{prefix}+{bonusPerLevel * level:F1}";
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
