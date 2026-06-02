using UnityEngine;

// 상점 "주방". 구매 가능 판정과 구매 실행(조리)을 전담한다.
// UI(ShopUI/상점 아이템)는 여기에 묻고/주문만 하며 직접 계산하지 않는다.
// 읽기는 위(GameManager/RoundManager/Reward), 명령은 아래(Inventory/UpgradeManager).
public class ShopController : MonoBehaviour
{
    [SerializeField]
    private GameManager gameManager;

    [SerializeField]
    private RoundManager roundManager;

    [SerializeField]
    private RewardController rewardController;

    [SerializeField]
    private WeaponInventoryNew inventory;

    [SerializeField]
    private UpgradeManager upgradeManager;

    [SerializeField]
    private float purchaseWindowDuration = 90f;

    private float purchaseTimeRemaining;
    private bool wasRoundActive;

    // 지금 상점을 열 수 있는가 (상점 페이즈 or 라운드 중 구매창 활성)
    public bool CanShop =>
        !gameManager.IsStopped && (roundManager.IsShopPhase || purchaseTimeRemaining > 0f);

    // 라운드 중 구매창 남은 시간 (UI 표시용)
    public float PurchaseTimeRemaining => purchaseTimeRemaining;

    private void Update()
    {
        // 라운드 시작 순간 구매창 부여
        bool isRoundActive = roundManager.IsRoundActive;
        if (isRoundActive && !wasRoundActive)
            purchaseTimeRemaining = purchaseWindowDuration;
        wasRoundActive = isRoundActive;

        if (purchaseTimeRemaining > 0f)
            purchaseTimeRemaining -= Time.deltaTime;
    }

    // ─── 무기 ────────────────────────────────────────────────
    public bool IsWeaponOwned(WeaponData data) => inventory.HasWeapon(data);

    public bool CanBuyWeapon(WeaponData data) =>
        data != null && !inventory.HasWeapon(data) && rewardController.Money >= data.price;

    public bool BuyWeapon(WeaponData data)
    {
        if (!CanBuyWeapon(data))
            return false;
        if (!rewardController.SpendMoney(data.price))
            return false;

        inventory.EquipByCategory(data);
        inventory.SwitchSlot((int)data.category);
        return true;
    }

    // ─── 공통 스탯 강화 ──────────────────────────────────────
    public int GetStatLevel(StatType statType) => upgradeManager.GetLevel(statType);

    public bool CanUpgradeStat(StatType statType, int price, int maxLevel) =>
        upgradeManager.GetLevel(statType) < maxLevel && rewardController.Money >= price;

    public bool UpgradeStat(StatType statType, int price, float bonusPerLevel, int maxLevel) =>
        upgradeManager.Upgrade(statType, price, bonusPerLevel, maxLevel);
}
