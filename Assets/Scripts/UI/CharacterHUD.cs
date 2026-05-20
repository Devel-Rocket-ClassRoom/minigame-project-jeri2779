using TMPro;
using UnityEngine;

public class CharacterHUD : MonoBehaviour
{
   
    [SerializeField] private CharacterHealth characterHealth;
    [SerializeField] private CharacterStats characterStats;
    [SerializeField] private CharacterShooter characterShooter;
    [SerializeField] private CharacterMoves characterMoves;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private RewardController rewardController;

    [SerializeField] private TextMeshProUGUI hpText;

    [SerializeField] private TextMeshProUGUI ammoText;

    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI killText;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI staminaText;


    private void Update()
    {
        UpdateHP();
        UpdateAmmo();
        UpdateWave();
        UpdateKill();
        UpdateSession();
        UpdateStamina();
    }

    private void UpdateHP()
    {
        if (hpText == null) return;
        hpText.text = $"{(int)characterHealth.CurrentHealth} / {(int)characterStats.MaxHealth}";
    }

    private void UpdateAmmo()
    {
        if (ammoText == null) return;
        ammoText.text = $"{characterShooter.CurrentAmmo} / {characterShooter.MagazineSize}";
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
                timerText.text = $"남은 시간\n{Mathf.CeilToInt(enemySpawner.RoundTimer)}s";
            else
                timerText.text = "남은 시간\n준비 중";
        }
    }
}
