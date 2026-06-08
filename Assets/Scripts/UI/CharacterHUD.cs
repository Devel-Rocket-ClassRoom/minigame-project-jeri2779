using TMPro;
using UnityEngine;

public class CharacterHUD : MonoBehaviour
{
    [SerializeField] private CharacterHealth characterHealth;
    [SerializeField] private CharacterStats characterStats;
    [SerializeField] private WeaponInventory weaponInventory;
    [SerializeField] private CharacterMoves characterMoves;
    [SerializeField] private RoundManager roundManager;
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
    [SerializeField] private TextMeshProUGUI totalDamageText;
    [SerializeField] private TextMeshProUGUI lastDamageText;
    [SerializeField] private float lastDamageDisplayTime = 1.5f;
    [SerializeField] private PlayerDamageCalculator damageCalculator;

    private float totalDamageDealt;
    private float lastDamageTimer;

    private void OnEnable()
    {
        if (characterHealth != null)
            characterHealth.OnHealthChanged += HandleHealthChanged;
        if (upgradeManager != null)
            upgradeManager.OnUpgradeLevelChanged += HandleUpgradeLevelChanged;
        if (roundManager != null)
            roundManager.OnRoundChanged += HandleRoundChanged;
        if (damageCalculator != null)
            damageCalculator.OnDamageDealt += HandleDamageDealt;

        UpdateHP();
        UpdateUpgradeLevels();
        UpdateWaveText();
        UpdateTotalDamage();
    }

    private void OnDisable()
    {
        if (characterHealth != null)
            characterHealth.OnHealthChanged -= HandleHealthChanged;
        if (upgradeManager != null)
            upgradeManager.OnUpgradeLevelChanged -= HandleUpgradeLevelChanged;
        if (roundManager != null)
            roundManager.OnRoundChanged -= HandleRoundChanged;
        if (damageCalculator != null)
            damageCalculator.OnDamageDealt -= HandleDamageDealt;
    }

    private void HandleHealthChanged(float current, float max)
    {
        if (hpText != null)
            hpText.text = $"{(int)current} / {(int)max}";
    }

    private void HandleUpgradeLevelChanged(int atkLevel, int hpLevel)
    {
        if (atkLvText != null)
            atkLvText.text = $"{atkLevel}";
        if (hpLvText != null)
            hpLvText.text = $"{hpLevel}";
    }

    private void HandleRoundChanged(int current, int total)
    {
        if (waveText != null)
            waveText.text = $"{current} / {total}";
    }

    private void Update()
    {
        UpdateAmmo();
        UpdateWave();
        UpdateKill();
        UpdateSession();
        UpdateStamina();
        UpdateLastDamage();
    }

    private void UpdateWaveText()
    {
        if (waveText == null || roundManager == null)
            return;

        waveText.text = $"{roundManager.CurrentRound} / {roundManager.TotalRounds}";
    }

    private void UpdateUpgradeLevels()
    {
        if (upgradeManager == null)
            return;

        if (atkLvText != null)
            atkLvText.text = $"{upgradeManager.AtkLevel}";
        if (hpLvText != null)
            hpLvText.text = $"{upgradeManager.HpLevel}";
    }

    private void UpdateHP()
    {
        if (hpText == null || characterHealth == null || characterStats == null)
            return;

        hpText.text = $"{(int)characterHealth.CurrentHealth} / {(int)characterStats.MaxHealth}";
    }

    private void UpdateAmmo()
    {
        if (ammoText == null || weaponInventory == null)
            return;

        var weapon = weaponInventory.CurrentWeapon;
        if (weapon == null || weapon.CurrentAmmo == -1)
            ammoText.text = "-- / --";
        else
            ammoText.text = $"{weapon.CurrentAmmo} / {weapon.ReserveAmmo}";
    }

    private void UpdateKill()
    {
        if (killText == null)
            return;

        killText.text = $"{EnemyRegistry.KilledCount}";
    }

    private void UpdateSession()
    {
        if (rewardController == null)
            return;

        if (moneyText != null)
            moneyText.text = $"{rewardController.Money}";
        if (scoreText != null)
            scoreText.text = $"{rewardController.Score}";
    }

    private void UpdateStamina()
    {
        if (staminaText == null || characterMoves == null)
            return;

        staminaText.text = $"{(int)characterMoves.CurrentStamina}";
    }

    private float lastDamageHomeY;
    private bool lastDamageHomeCaptured;

    private void HandleDamageDealt(float damage, bool isHeadshot)
    {
        totalDamageDealt += damage;
        UpdateTotalDamage();

        if (lastDamageText != null)
        {
            var rt = lastDamageText.rectTransform;

            // 최초 1회만 원점(홈) 좌표 저장 — 이후 항상 이 위치에서 시작
            if (!lastDamageHomeCaptured)
            {
                lastDamageHomeY = rt.anchoredPosition.y;
                lastDamageHomeCaptured = true;
            }

            lastDamageText.text = $"{(int)damage}";
            lastDamageText.gameObject.SetActive(true);
            lastDamageTimer = lastDamageDisplayTime;

            // 원점으로 리셋 후 알파 복구
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, lastDamageHomeY);
            var c = lastDamageText.color;
            lastDamageText.color = new Color(c.r, c.g, c.b, 1f);
        }
    }

    private void UpdateTotalDamage()
    {
        if (totalDamageText == null) return;
        totalDamageText.text = $"{(int)totalDamageDealt}";
    }

    [SerializeField] private float lastDamageFloatDistance = 40f;

    private void UpdateLastDamage()
    {
        if (lastDamageText == null) return;
        if (lastDamageTimer <= 0f) return;

        lastDamageTimer -= Time.deltaTime;
        float t = 1f - Mathf.Clamp01(lastDamageTimer / lastDamageDisplayTime);

        // 위로 떠오름 (항상 원점 기준)
        var rt = lastDamageText.rectTransform;
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, lastDamageHomeY + lastDamageFloatDistance * t);

        // 후반 절반에서 페이드아웃
        float alpha = t < 0.5f ? 1f : 1f - (t - 0.5f) * 2f;
        var c = lastDamageText.color;
        lastDamageText.color = new Color(c.r, c.g, c.b, alpha);

        if (lastDamageTimer <= 0f)
            lastDamageText.gameObject.SetActive(false);
    }

    private void UpdateWave()
    {
        if (roundManager == null)
            return;

        if (timerText != null)
        {
            if (roundManager.IsRoundActive)
            {
                int total = Mathf.FloorToInt(roundManager.RoundTimer);
                int min = total / 60;
                int sec = total % 60;
                timerText.text = $"{min:00}:{sec:00}";
            }
            else
            {
                timerText.text = "00:00";
            }
        }

        if (shopCountdownText != null)
        {
            bool isShopPhase = roundManager.IsShopPhase;
            shopCountdownText.gameObject.SetActive(isShopPhase);
            if (isShopPhase)
                shopCountdownText.text = $"라운드 시작 대기까지\n{Mathf.FloorToInt(roundManager.ShopTimer)}초";
        }
    }
}
