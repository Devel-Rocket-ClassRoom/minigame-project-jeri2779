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

    public void Show(string displayName, int level, string effectText)
    {
        gameObject.SetActive(true);
        if (nameText != null) nameText.text = displayName;
        if (levelText != null) levelText.text = $"Lv.{level}";
        if (valueText != null) valueText.text = effectText;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
