using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private Button atkUpgradeButton;
    [SerializeField] private Button hpUpgradeButton;
    [SerializeField] private TMP_Text atkLevelText;
    [SerializeField] private TMP_Text atkPriceText;
    [SerializeField] private TMP_Text hpLevelText;
    [SerializeField] private TMP_Text hpPriceText;
    [SerializeField] private ShopController shopController;
    [SerializeField] private RewardController rewardController;

    [SerializeField] private Button[] categoryButtons;
    [SerializeField] private GameObject[] categoryPanels;
    [SerializeField] private Button closeButton;

    [SerializeField] private TMP_Text shopMoneyText;
    [SerializeField] private TMP_Text shopTimeText;
    [SerializeField] private WeaponInfoPanel weaponInfoPanel;

    private WeaponShopItem[] shopItems;
    private StatUpgradeItem[] statItems;
    private CanvasGroup[] categoryNormalCGs;
    private CanvasGroup[] categoryHighlightedCGs;

    private void Awake()
    {
        atkUpgradeButton.onClick.AddListener(OnAtkUpgrade);
        hpUpgradeButton.onClick.AddListener(OnHpUpgrade);
        shopPanel.SetActive(false);

        categoryNormalCGs = new CanvasGroup[categoryButtons.Length];
        categoryHighlightedCGs = new CanvasGroup[categoryButtons.Length];
        for (int i = 0; i < categoryButtons.Length; i++)
        {
            int idx = i;
            categoryButtons[i].onClick.AddListener(() => ShowPanel(idx));

            Transform t = categoryButtons[i].transform;
            categoryNormalCGs[i] = t.Find("Normal")?.GetComponent<CanvasGroup>();
            categoryHighlightedCGs[i] = t.Find("Highlighted")?.GetComponent<CanvasGroup>();
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        foreach (var panel in categoryPanels)
            panel.SetActive(false);

        shopItems = shopPanel.GetComponentsInChildren<WeaponShopItem>(true);
        statItems = shopPanel.GetComponentsInChildren<StatUpgradeItem>(true);
    }

    private void OnAtkUpgrade()
    {
        upgradeManager.UpgradeAttack();
        RefreshAll();
    }

    private void OnHpUpgrade()
    {
        upgradeManager.UpgradeHp();
        RefreshAll();
    }

    private void RefreshAll()
    {
        foreach (var item in shopItems)
            item.Refresh(shopController);

        foreach (var item in statItems)
            item.Refresh(shopController);

        if (atkLevelText != null) atkLevelText.text = $"LV.{upgradeManager.AtkLevel}";
        if (hpLevelText != null) hpLevelText.text = $"LV.{upgradeManager.HpLevel}";
        if (atkPriceText != null) atkPriceText.text = $"{upgradeManager.UpgradePrice}$";
        if (hpPriceText != null) hpPriceText.text = $"{upgradeManager.UpgradePrice}$";
    }

    private int openPanelIndex = -1;

    private void ShowPanel(int index)
    {
        bool isAlreadyOpen = categoryPanels[index].activeSelf;
        for (int i = 0; i < categoryPanels.Length; i++)
        {
            categoryPanels[i].SetActive(!isAlreadyOpen && i == index);
        }
        openPanelIndex = isAlreadyOpen ? -1 : index;
        UpdateCategorySelection();
        RefreshAll();
    }

     
    public void ClosePanel()
    {
        if (openPanelIndex < 0) return;
        categoryPanels[openPanelIndex].SetActive(false);
        openPanelIndex = -1;
        UpdateCategorySelection();
    }

    //선택한 카테고리 버튼의 highligted상태 유지용.
    private void UpdateCategorySelection()
    {
        for (int i = 0; i < categoryButtons.Length; i++)
        {
            CanvasGroup normalCG = categoryNormalCGs[i];
            CanvasGroup highlightedCG = categoryHighlightedCGs[i];
            if (normalCG == null || highlightedCG == null) continue;

            bool selected = i == openPanelIndex;
            normalCG.alpha = selected ? 0f : 1f;
            highlightedCG.alpha = selected ? 1f : 0f;
            categoryButtons[i].interactable = !selected;
        }
    }

    public void TryBuy(WeaponData data)
    {
        if (shopController.BuyWeapon(data))
        {
            RefreshAll();
            Close();
        }
    }

    public void ShowWeaponInfo(WeaponData data) => weaponInfoPanel?.Show(data);

    public void HideWeaponInfo() => weaponInfoPanel?.Hide();

    /// <summary>StatUpgradeItem에서 호출. 강화 시도 후 UI 갱신.</summary>
    public void TryStatUpgrade(StatType statType, int price, float bonusPerLevel, int maxLevel)
    {
        if (shopController.UpgradeStat(statType, price, bonusPerLevel, maxLevel))
            RefreshAll();
    }

    private void Update()
    {
        if (shopPanel.activeSelf)
        {
            if (!shopController.CanShop)
            {
                if (gameManager.IsStopped) SoftClose();
                else Close();
                return;
            }
            UpdateInfoTexts();
        }

        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame && shopPanel.activeSelf)
        {
            if (openPanelIndex >= 0)
                ClosePanel();
            else
                Close();
            return;
        }

        if (!Keyboard.current.bKey.wasPressedThisFrame) return;
        if (!shopController.CanShop) return;

        if (shopPanel.activeSelf)
            Close();
        else
            Open();
    }

    private void UpdateInfoTexts()
    {
        if (shopMoneyText != null)
            shopMoneyText.text = $"{rewardController.Money}G";
        if (shopTimeText != null)
        {
            float display = roundManager.IsShopPhase ? roundManager.ShopTimer : shopController.PurchaseTimeRemaining;
            shopTimeText.text = $"{Mathf.FloorToInt(display)}초";
        }
    }

    private void Open()
    {
        UIManager.EnsureInstance().ShowOverlay(shopPanel, this);
        RefreshAll();
    }

    private void Close()
    {
        SoftClose();
        UIManager.EnsureInstance().RegisterOverlayClosed(this);
    }

    // 게임 중단 등 외부 상태로 인한 닫힘. 커서 잠금 안 함.
    private void SoftClose()
    {
        if (openPanelIndex >= 0)
            categoryPanels[openPanelIndex].SetActive(false);
        openPanelIndex = -1;
        UpdateCategorySelection();
        weaponInfoPanel?.Hide();
        shopPanel.SetActive(false);
    }
}
