using TMPro;
using UnityEngine;
using UnityEngine.UI;

 
// 상점 내 공통 스탯 강화 버튼 1개에 붙이는 스크립트
// inspector에서 StatType, price, maxLevel, bonusPerLevel 을 설정한다.
 
public class StatUpgradeItem : MonoBehaviour
{
    [SerializeField] private StatType statType;
    [SerializeField] private int price = 2000;
    [SerializeField] private int maxLevel = 5;
    [SerializeField] private float bonusPerLevel = 1f;

    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text priceText;

    private Button button;
    private ShopUI shopUI;

    private void Start()
    {
        button = GetComponent<Button>();
        shopUI = GetComponentInParent<ShopUI>(true);
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        shopUI.TryStatUpgrade(statType, price, bonusPerLevel, maxLevel);
    }

 
    public void Refresh(int money, UpgradeManager upgradeManager)
    {
        if (button == null) button = GetComponent<Button>();

        int level = upgradeManager.GetLevel(statType);
        bool maxed = level >= maxLevel;

        button.interactable = !maxed && money >= price;

        if (levelText != null)
            levelText.text = $"{level}/{maxLevel}";

        if (priceText != null)
            priceText.text = maxed ? "MAX" : $"{price}G";
    }
}
