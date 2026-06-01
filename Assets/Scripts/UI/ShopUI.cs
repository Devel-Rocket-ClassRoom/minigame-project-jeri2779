using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private Button atkUpgradeButton;
    [SerializeField] private Button hpUpgradeButton;
    [SerializeField] private TMP_Text atkLevelText;
    [SerializeField] private TMP_Text atkPriceText;
    [SerializeField] private TMP_Text hpLevelText;
    [SerializeField] private TMP_Text hpPriceText;
    [SerializeField] private WeaponInventoryNew weaponInventory;
    [SerializeField] private RewardController rewardController;

    [SerializeField] private Button[] categoryButtons;
    [SerializeField] private GameObject[] categoryPanels;

    [SerializeField] private TMP_Text shopMoneyText;
    [SerializeField] private TMP_Text shopTimeText;
    [SerializeField] private WeaponInfoPanel weaponInfoPanel;

    [SerializeField] private float purchaseWindowDuration = 30f;

    private WeaponShopItem[] shopItems;
    private StatUpgradeItem[] statItems;
    private float purchaseTimeRemaining;
    private bool wasRoundActive;

    private bool CanShop => !enemySpawner.IsGameStopped && (enemySpawner.IsShopPhase || purchaseTimeRemaining > 0f);

    private void Awake()
    {
        atkUpgradeButton.onClick.AddListener(OnAtkUpgrade);
        hpUpgradeButton.onClick.AddListener(OnHpUpgrade);
        shopPanel.SetActive(false);

        for (int i = 0; i < categoryButtons.Length; i++)
        {
            int idx = i;
            categoryButtons[i].onClick.AddListener(() => ShowPanel(idx));
        }

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
            item.Refresh(rewardController.Money, weaponInventory);

        foreach (var item in statItems)
            item.Refresh(rewardController.Money, upgradeManager);

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

    //선택한 카테고리 버튼의 highligted상태 유지용.
    private void UpdateCategorySelection()
    {
        for (int i = 0; i < categoryButtons.Length; i++)
        {
            Transform t = categoryButtons[i].transform;
            CanvasGroup normalCG = t.Find("Normal")?.GetComponent<CanvasGroup>();
            CanvasGroup highlightedCG = t.Find("Highlighted")?.GetComponent<CanvasGroup>();
            if (normalCG == null || highlightedCG == null) continue;

            bool selected = i == openPanelIndex;
            normalCG.alpha = selected ? 0f : 1f;
            highlightedCG.alpha = selected ? 1f : 0f;
            categoryButtons[i].interactable = !selected;
        }
    }

    public void TryBuy(WeaponData data)
    {
        if (data == null || weaponInventory == null) return;
        if (weaponInventory.HasWeapon(data))
        {
            Debug.Log("이미 장착됨");
            return;
        }
        if (!rewardController.SpendMoney(data.price))
        {
            Debug.Log("돈 부족");
            return;
        }
        weaponInventory.EquipByCategory(data);
        RefreshAll();
        weaponInventory.SwitchSlot((int)data.category);
        Close();
    }

    public void ShowWeaponInfo(WeaponData data) => weaponInfoPanel?.Show(data);

    public void HideWeaponInfo() => weaponInfoPanel?.Hide();

    /// <summary>StatUpgradeItem에서 호출. 강화 시도 후 UI 갱신.</summary>
    public void TryStatUpgrade(StatType statType, int price, float bonusPerLevel, int maxLevel)
    {
        upgradeManager.Upgrade(statType, price, bonusPerLevel, maxLevel);
        RefreshAll();
    }

    private void Update()
    {
      

        // 라운드 시작 감지 → 구매 가능 시간 초기화
        bool isRoundActive = enemySpawner.IsRoundActive;
        if (isRoundActive && !wasRoundActive)
            purchaseTimeRemaining = purchaseWindowDuration;
        wasRoundActive = isRoundActive;

        if (purchaseTimeRemaining > 0f)
        {
            purchaseTimeRemaining -= Time.deltaTime;
            if (purchaseTimeRemaining <= 0f && shopPanel.activeSelf)
                Close();
        }

        if (shopPanel.activeSelf)
        {
            if (!CanShop)
            {
                if (enemySpawner.IsGameStopped) SoftClose();
                else Close();
                return;
            }
            UpdateInfoTexts();
        }

        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame && shopPanel.activeSelf)
        {
            if (openPanelIndex >= 0)
            {
                categoryPanels[openPanelIndex].SetActive(false);
                openPanelIndex = -1;
                UpdateCategorySelection();
            }
            else
            {
                Close();
            }
            return;
        }

        if (!Keyboard.current.bKey.wasPressedThisFrame) return;
        if (!CanShop) return;

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
            float display = enemySpawner.IsShopPhase ? enemySpawner.ShopTimer : purchaseTimeRemaining;
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
