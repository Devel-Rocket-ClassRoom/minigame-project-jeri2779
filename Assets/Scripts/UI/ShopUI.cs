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

    [Header("종류별 패시브 설명창 (마우스오버)")]
    [SerializeField] private GameObject passiveInfoPanel;
    [SerializeField] private TMP_Text passiveInfoNameText;
    [SerializeField] private TMP_Text passiveInfoDescText;

    private WeaponShopItem[] shopItems;
    private StatUpgradeItem[] statItems;
    private WeaponTypePassiveItem[] passiveItems;
    private CanvasGroup[] categoryNormalCGs;
    private CanvasGroup[] categoryHighlightedCGs;
    private int openPanelIndex = -1;

    private void Awake()
    {
        if (atkUpgradeButton != null)
            atkUpgradeButton.onClick.AddListener(OnAtkUpgrade);
        if (hpUpgradeButton != null)
            hpUpgradeButton.onClick.AddListener(OnHpUpgrade);
        if (shopPanel != null)
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

        shopItems = shopPanel != null ? shopPanel.GetComponentsInChildren<WeaponShopItem>(true) : new WeaponShopItem[0];
        statItems = shopPanel != null ? shopPanel.GetComponentsInChildren<StatUpgradeItem>(true) : new StatUpgradeItem[0];
        passiveItems = shopPanel != null ? shopPanel.GetComponentsInChildren<WeaponTypePassiveItem>(true) : new WeaponTypePassiveItem[0];
    }

    private void OnAtkUpgrade()
    {
        shopController?.UpgradeAttack();
        RefreshAll();
    }

    private void OnHpUpgrade()
    {
        shopController?.UpgradeHp();
        RefreshAll();
    }

    private void RefreshAll()
    {
        if (shopController != null)
        {
            foreach (var item in shopItems)
                item.Refresh(shopController);

            foreach (var item in statItems)
                item.Refresh(shopController);

            foreach (var item in passiveItems)
                item.Refresh(shopController);
        }

        if (upgradeManager == null)
            return;

        if (atkLevelText != null)
            atkLevelText.text = $"LV.{upgradeManager.AtkLevel}";
        if (hpLevelText != null)
            hpLevelText.text = $"LV.{upgradeManager.HpLevel}";
        if (atkPriceText != null)
            atkPriceText.text = $"{upgradeManager.UpgradePrice}$";
        if (hpPriceText != null)
            hpPriceText.text = $"{upgradeManager.UpgradePrice}$";
    }

    private void ShowPanel(int index)
    {
        bool isAlreadyOpen = categoryPanels[index].activeSelf;
        for (int i = 0; i < categoryPanels.Length; i++)
            categoryPanels[i].SetActive(!isAlreadyOpen && i == index);

        openPanelIndex = isAlreadyOpen ? -1 : index;
        UpdateCategorySelection();
        RefreshAll();
    }

    public void ClosePanel()
    {
        if (openPanelIndex < 0)
            return;

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
            if (normalCG == null || highlightedCG == null)
                continue;

            bool selected = i == openPanelIndex;
            normalCG.alpha = selected ? 0f : 1f;
            highlightedCG.alpha = selected ? 1f : 0f;
            categoryButtons[i].interactable = !selected;
        }
    }

    public void TryBuy(WeaponData data)
    {
        if (shopController != null && shopController.BuyWeapon(data))
        {
            RefreshAll();
            Close();
        }
    }

    public void ShowWeaponInfo(WeaponData data) => weaponInfoPanel?.Show(data);

    public void HideWeaponInfo() => weaponInfoPanel?.Hide();

    public void ShowPassiveInfo(string label, string description)
    {
        if (passiveInfoPanel != null) passiveInfoPanel.SetActive(true);
        if (passiveInfoNameText != null) passiveInfoNameText.text = label;
        if (passiveInfoDescText != null) passiveInfoDescText.text = description;
    }

    public void HidePassiveInfo()
    {
        if (passiveInfoPanel != null) passiveInfoPanel.SetActive(false);
    }

    public void TryStatUpgrade(StatType statType, int price, float bonusPerLevel, int maxLevel)
    {
        if (shopController != null && shopController.UpgradeStat(statType, price, bonusPerLevel, maxLevel))
            RefreshAll();
    }

    public void TryUnlockWeaponType(WeaponType type, int price)
    {
        if (shopController != null && shopController.UnlockWeaponType(type, price))
            RefreshAll();
    }

    private void Update()
    {
        // 일시정지(Time.timeScale==0) 중에는 상점 입력을 차단한다. 일시정지와 상점은 동시에 열리면 안 된다.
        if (Time.timeScale == 0f) return;

        if (shopPanel != null && shopPanel.activeSelf)
        {
            if (shopController == null || !shopController.CanShop)
            {
                if (gameManager != null && gameManager.IsStopped)
                    SoftClose();
                else
                    Close();
                return;
            }

            UpdateInfoTexts();
        }

        if (!Keyboard.current.bKey.wasPressedThisFrame)
            return;
        if (!shopController.CanShop)
            return;

        if (shopPanel != null && shopPanel.activeSelf)
            Close();
        else
            Open();
    }

    private void UpdateInfoTexts()
    {
        if (shopMoneyText != null && rewardController != null)
            shopMoneyText.text = $"{rewardController.Money}$";

        if (shopTimeText != null && roundManager != null && shopController != null)
        {
            float display = roundManager.IsShopPhase ? roundManager.ShopTimer : shopController.PurchaseTimeRemaining;
            shopTimeText.text = $"{Mathf.FloorToInt(display)}초";
        }
    }

    private void Open()
    {
        UIManager.EnsureInstance().ShowOverlay(shopPanel, this);
        UIManager.EnsureInstance().PushEscape(HandleEscape);
        RefreshAll();
    }

    private void HandleEscape()
    {
        if (openPanelIndex >= 0)
            ClosePanel();
        else
            Close();
    }

    private void Close()
    {
        UIManager.EnsureInstance().PopEscape();
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
        HidePassiveInfo();

        if (shopPanel != null)
            shopPanel.SetActive(false);
    }
}
