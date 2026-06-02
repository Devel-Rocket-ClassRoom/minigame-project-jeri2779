using TMPro;
using UnityEngine;
using UnityEngine.UI;

 
// 상점 내 공통 스탯 강화 버튼 1개에 붙이는 스크립트
// inspector에서 StatType, price, maxLevel, bonusPerLevel 을 설정한다.
 
public class StatUpgradeItem : MonoBehaviour
{
    [SerializeField] private string displayName;
    [SerializeField] private StatType statType;
    [SerializeField] private int price = 2000;
    [SerializeField] private int maxLevel = 5;
    [SerializeField] private float bonusPerLevel = 1f;
    [SerializeField] private bool isPercentage;

    // SO 마이그레이션 시 아래 프로퍼티의 반환값만 SO 필드로 교체하면 됨
    public string DisplayName => displayName;
    public StatType StatType => statType;
    public float BonusPerLevel => bonusPerLevel;
    public bool IsPercentage => isPercentage;

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

 
    public void Refresh(ShopController shop)
    {
        if (button == null) button = GetComponent<Button>();

        int level = shop.GetStatLevel(statType);
        bool maxed = level >= maxLevel;

        button.interactable = shop.CanUpgradeStat(statType, price, maxLevel);

        if (levelText != null)
            levelText.text = $"{level}/{maxLevel}";

        if (priceText != null)
            priceText.text = maxed ? "MAX" : $"{price}G";
    }
}
