using TMPro;
using UnityEngine;

public class CharacterHUD : MonoBehaviour
{
   
    [SerializeField] private CharacterHealth characterHealth;
    [SerializeField] private CharacterStats characterStats;
    [SerializeField] private CharacterShooter characterShooter;
    [SerializeField] private EnemySpawner enemySpawner;

    [SerializeField] private TextMeshProUGUI hpText;

    [SerializeField] private TextMeshProUGUI ammoText;

    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI timerText;


    private void Start()
    {
        if (characterHealth == null)
            characterHealth = GetComponent<CharacterHealth>();

        if (characterStats == null)
            characterStats = GetComponent<CharacterStats>();

        if (characterShooter == null)
            characterShooter = GetComponent<CharacterShooter>();

        if (enemySpawner == null)
            enemySpawner = GetComponent<EnemySpawner>();
    }
    private void Update()
    {
        UpdateHP();
        UpdateAmmo();
        UpdateWave();
    }

    private void UpdateHP()
    {
        if (hpText == null) return;
        hpText.text = $"{(int)characterHealth.CurrentHealth} / {(int)characterStats.MaxHealth}";
    }

    private void UpdateAmmo()
    {
        if (ammoText == null) return;
        if (characterShooter.IsReloading)
            ammoText.text = "재장전 중";
        else
            ammoText.text = $"{characterShooter.CurrentAmmo} / {characterShooter.MagazineSize}";
    }

    private void UpdateWave()
    {
        if (waveText != null)
            waveText.text = $"Wave {enemySpawner.CurrentRound} / {enemySpawner.TotalRounds}";

        if (timerText != null)
        {
            if (enemySpawner.IsRoundActive)
                timerText.text = $"{Mathf.CeilToInt(enemySpawner.RoundTimer)}s";
            else
                timerText.text = "준비 중";
        }
    }
}
