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
        RefreshAll();
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
        shopPanel.SetActive(true);
        RefreshAll();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Close()
    {
        SoftClose();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // 게임 중단 등 외부 상태로 인한 닫힘. 커서 잠금 안 함.
    private void SoftClose()
    {
        if (openPanelIndex >= 0)
            categoryPanels[openPanelIndex].SetActive(false);
        openPanelIndex = -1;
        weaponInfoPanel?.Hide();
        shopPanel.SetActive(false);
    }
}
