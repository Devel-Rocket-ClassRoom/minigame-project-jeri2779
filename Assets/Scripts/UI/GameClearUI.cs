using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameClearUI : MonoBehaviour
{
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private RewardController rewardController;

    [SerializeField] private GameObject playingHUD;
    [SerializeField] private GameObject crossHair;
    [SerializeField] private GameObject fpWeapon;

    [SerializeField] private Button mainMenuButton;
    [SerializeField] private TextMeshProUGUI waveResultText;
    [SerializeField] private TextMeshProUGUI killResultText;
    [SerializeField] private TextMeshProUGUI scoreResultText;

    [Header("이번 판 추가 기록")]
    [SerializeField] private PlayerDamageCalculator damageCalc;
    [SerializeField] private TextMeshProUGUI damageResultText;
    [SerializeField] private TextMeshProUGUI headshotResultText;
    [SerializeField] private TextMeshProUGUI moneyResultText;
    [SerializeField] private TextMeshProUGUI timeResultText;

    [Header("숫자 롤업 연출")]
    [SerializeField] private float rollDuration = 0.6f;
    [SerializeField] private float gap = 0.15f;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    public void Show()
    {
        gameObject.SetActive(true);

        UIManager.EnsureInstance().RegisterOverlayOpened(this);

        if (playingHUD != null) playingHUD.SetActive(false);
        if (crossHair != null) crossHair.SetActive(false);
        if (fpWeapon != null) fpWeapon.SetActive(false);

        int total = roundManager.TotalRounds;

        // 라벨은 씬의 Label 객체가 담당. 코드는 값(숫자)만 0부터 굴린다. 순서 = Col_L 위→아래, 그 다음 Col_R.
        var items = new List<StatRollup.Item>
        {
            new StatRollup.Item { text = waveResultText,     target = total,                            fmt = n => $"{n} / {total}" },
            new StatRollup.Item { text = killResultText,     target = EnemyRegistry.KilledCount,        fmt = n => n.ToString() },
            new StatRollup.Item { text = scoreResultText,    target = rewardController.Score,           fmt = n => n.ToString() },
            new StatRollup.Item { text = damageResultText,   target = (int)damageCalc.TotalDamageDealt, fmt = n => n.ToString() },
            new StatRollup.Item { text = headshotResultText, target = damageCalc.HeadshotCount,         fmt = n => n.ToString() },
            new StatRollup.Item { text = moneyResultText,    target = rewardController.TotalEarned,     fmt = n => n.ToString() },
            new StatRollup.Item { text = timeResultText,     target = (int)roundManager.RunTime,        fmt = n => FormatTime(n) },
        };
        StartCoroutine(StatRollup.Cascade(items, rollDuration, gap));
    }

    private string FormatTime(int sec)
    {
        return $"{sec / 60:00}:{sec % 60:00}";
    }

    private void OnMainMenuClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
