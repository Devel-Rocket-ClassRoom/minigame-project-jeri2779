using TMPro;
using UnityEngine;

public class CharacterHUD : MonoBehaviour
{
   
    [SerializeField] private CharacterHealth characterHealth;
    [SerializeField] private CharacterStats characterStats;
    [SerializeField] private WeaponInventoryNew weaponInventory;
    [SerializeField] private CharacterMoves characterMoves;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private RewardController rewardController;
    [SerializeField] private UpgradeManager upgradeManager;

    [SerializeField] private TextMeshProUGUI hpText;

    [SerializeField] private TextMeshProUGUI ammoText;

    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI shopCountdownText;
    [SerializeField] private TextMeshProUGUI killText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI staminaText;
    [SerializeField] private TextMeshProUGUI atkLvText;
    [SerializeField] private TextMeshProUGUI hpLvText;


    private void Update()
    {
        UpdateHP();
        UpdateAmmo();
        UpdateWave();
        UpdateKill();
        UpdateSession();
        UpdateStamina();
        UpdateUpgradeLevels();
    }

    private void UpdateUpgradeLevels()
    {
        if (upgradeManager == null) return;
        if (atkLvText != null) atkLvText.text = $"{upgradeManager.AtkLevel}";
        if (hpLvText != null) hpLvText.text = $"{upgradeManager.HpLevel}";
    }

    private void UpdateHP()
    {
        if (hpText == null) return;
        hpText.text = $"{(int)characterHealth.CurrentHealth} / {(int)characterStats.MaxHealth}";
    }

    private void UpdateAmmo()
    {
        if (ammoText == null) return;
        var w = weaponInventory.CurrentWeapon;
        if (w == null || w.CurrentAmmo == -1)
            ammoText.text = "— / —";
        else
            ammoText.text = $"{w.CurrentAmmo} / {w.ReserveAmmo}";
    }

    private void UpdateKill()
    {
        if (killText == null) return;
        killText.text = $"Killed\n{enemySpawner.KilledCount}";
    }

    private void UpdateSession()
    {
        if (rewardController == null) return;
        if (moneyText != null)
            moneyText.text = $"{rewardController.Money}$";
        if (scoreText != null)
            scoreText.text = $"{rewardController.Score}";
    }

    private void UpdateStamina()
    {
        if (staminaText == null) return;
        staminaText.text = $"{(int)characterMoves.CurrentStamina}";
    }

    private void UpdateWave()
    {
        if (waveText != null)
            waveText.text = $"Round\n{enemySpawner.CurrentRound} / {enemySpawner.TotalRounds}";

        if (timerText != null)
        {
            if (enemySpawner.IsRoundActive)
            {
                int total = Mathf.FloorToInt(enemySpawner.RoundTimer);
                int min = total / 60;
                int sec = total % 60;
                timerText.text = $"남은 시간\n{min:00}:{sec:00}";
            }
            else
                timerText.text = $"남은 시간\n00:00";
        }

        if (shopCountdownText != null)
        {
            if (enemySpawner.IsShopPhase)
                shopCountdownText.text = $"라운드 시작까지\n{Mathf.FloorToInt(enemySpawner.ShopTimer)}초";
            else
                shopCountdownText.text = string.Empty;
        }
    }
}
