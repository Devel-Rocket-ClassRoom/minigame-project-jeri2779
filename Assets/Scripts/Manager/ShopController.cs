using UnityEngine;

// 상점 구매/판매/업그레이드 명령 처리. UI는 ShopUI가 담당.
// UI(ShopUI/상점 아이템)는 여기에 묻고/주문만 하며 직접 계산하지 않는다.
public class ShopController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private RewardController rewardController;
    [SerializeField] private WeaponInventoryNew inventory;
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private WeaponTypePassive weaponTypePassive;
    [SerializeField] private float purchaseWindowDuration = 90f;

    // 라운드 중 구매창 남은 시간 (UI 표시용)
    private float purchaseTimeRemaining;
    private bool wasRoundActive;

     // 지금 상점을 열 수 있는지(상점 페이즈 or 라운드 중 구매창 활성)
    public bool CanShop =>
        gameManager != null
        && roundManager != null
        && !gameManager.IsStopped
        && (roundManager.IsShopPhase || purchaseTimeRemaining > 0f);

    public float PurchaseTimeRemaining => purchaseTimeRemaining;

    private void Update()
    {
        // 라운드 시작 순간 구매창 부여 및 타이머 초기화
        if (roundManager == null)
            return;

        bool isRoundActive = roundManager.IsRoundActive;
        if (isRoundActive && !wasRoundActive)
            purchaseTimeRemaining = purchaseWindowDuration;
        wasRoundActive = isRoundActive;

        if (purchaseTimeRemaining > 0f)
            purchaseTimeRemaining -= Time.deltaTime;
    }
    // ─── 무기 ───
    public bool IsWeaponOwned(WeaponData data) =>
        data != null && inventory != null && inventory.HasWeapon(data);

    public bool CanBuyWeapon(WeaponData data) =>
        data != null
        && inventory != null
        && rewardController != null
        && !inventory.HasWeapon(data)
        && rewardController.Money >= data.price;

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

    // ─── 공통 스탯 강화 ──── 
    public int GetStatLevel(StatType statType) =>
        upgradeManager != null ? upgradeManager.GetLevel(statType) : 0;

    public bool CanUpgradeStat(StatType statType, int price, int maxLevel) =>
        upgradeManager != null
        && rewardController != null
        && upgradeManager.GetLevel(statType) < maxLevel
        && rewardController.Money >= price;

    public bool UpgradeStat(StatType statType, int price, float bonusPerLevel, int maxLevel)
    {
        if (!CanUpgradeStat(statType, price, maxLevel))
            return false;
        if (!rewardController.SpendMoney(price))
            return false;

        return upgradeManager.Upgrade(statType, bonusPerLevel, maxLevel);
    }

    // ─── ATK / HP 강화 ───
    public bool UpgradeAttack()
    {
        if (rewardController == null || upgradeManager == null)
            return false;
        if (!rewardController.SpendMoney(upgradeManager.UpgradePrice))
            return false;

        upgradeManager.UpgradeAttack();
        return true;
    }

    public bool UpgradeHp()
    {
        if (rewardController == null || upgradeManager == null)
            return false;
        if (!rewardController.SpendMoney(upgradeManager.UpgradePrice))
            return false;

        upgradeManager.UpgradeHp();
        return true;
    }

    // ─── 종류별 무기 패시브 해금 (레벨 없는 단발 해금) ───
    public bool IsWeaponTypeUnlocked(WeaponType type) =>
        weaponTypePassive != null && weaponTypePassive.IsUnlocked(type);

    public bool CanUnlockWeaponType(WeaponType type, int price) =>
        weaponTypePassive != null
        && rewardController != null
        && !weaponTypePassive.IsUnlocked(type)
        && rewardController.Money >= price;

    public bool UnlockWeaponType(WeaponType type, int price)
    {
        if (!CanUnlockWeaponType(type, price))
            return false;
        if (!rewardController.SpendMoney(price))
            return false;

        weaponTypePassive.Unlock(type);
        return true;
    }
}
