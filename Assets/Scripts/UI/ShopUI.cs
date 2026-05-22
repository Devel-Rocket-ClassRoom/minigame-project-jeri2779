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
    [SerializeField] private Button weaponShopButton;
    [SerializeField] private WeaponData arData;
    [SerializeField] private WeaponInventoryNew weaponInventory;
    [SerializeField] private RewardController rewardController;

    private void Awake()
    {
        atkUpgradeButton.onClick.AddListener(upgradeManager.UpgradeAttack);
        hpUpgradeButton.onClick.AddListener(upgradeManager.UpgradeHp);
        weaponShopButton?.onClick.AddListener(BuyAR);
        shopPanel.SetActive(false);
    }

    private void BuyAR()
    {
        if (arData == null || weaponInventory == null) return;
        if (weaponInventory.HasWeapon(arData))
        {
            Debug.Log("이미 장착됨");
            return;
        }
        if (!rewardController.SpendMoney(arData.price))
        {
            Debug.Log("돈 부족");
            return;
        }
        weaponInventory.EquipByCategory(arData);
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
        shopPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
