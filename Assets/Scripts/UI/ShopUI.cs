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

    private void Awake()
    {
        atkUpgradeButton.onClick.AddListener(upgradeManager.UpgradeAttack);
        hpUpgradeButton.onClick.AddListener(upgradeManager.UpgradeHp);
        shopPanel.SetActive(false);

        for (int i = 0; i < categoryButtons.Length; i++)
        {
            int idx = i;
            categoryButtons[i].onClick.AddListener(() => ShowPanel(idx));
        }

        foreach (var panel in categoryPanels)
            panel.SetActive(false);
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
    }

    private void Update()
    {
        if (shopPanel.activeSelf && !enemySpawner.IsShopPhase)
        {
            Close();
            return;
        }

        if (Keyboard.current == null) return;
        if (!Keyboard.current.bKey.wasPressedThisFrame) return;
        if (!enemySpawner.IsShopPhase) return;

        if (shopPanel.activeSelf)
            Close();
        else
            Open();
    }

    private void Open()
    {
        shopPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Close()
    {
        if (openPanelIndex >= 0)
        {
            categoryPanels[openPanelIndex].SetActive(false);
        }
        openPanelIndex = -1;
        shopPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
