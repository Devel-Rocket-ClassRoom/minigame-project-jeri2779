using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private RoundManager roundManager;
    [SerializeField] private RewardController rewardController;

    [SerializeField] private GameObject playingHUD;
    [SerializeField] private GameObject crossHair;
    [SerializeField] private GameObject fpWeapon;

    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private GameObject restartLabel;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject gameOverText;
    [SerializeField] private float restartTimer = 10f;

    [SerializeField] private GameObject resultGroup; // 결과 행 전체 컨테이너(ResultText). 카운트다운 중 숨김.
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

    [Header("배경 암전")]
    [SerializeField] private Image backgroundImage; // 카운트다운 동안 점점 검게

    private Color bgStartColor;
    private float currentTime;
    private bool isActive = false;

    private void Awake()
    {
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    // 사망 즉시(틸트 전) 게임플레이 UI/무기모델 숨김. CharacterDead가 호출.
    public void HideGameplayUI()
    {
        if (playingHUD != null) playingHUD.SetActive(false);
        if (crossHair != null) crossHair.SetActive(false);
        if (fpWeapon != null) fpWeapon.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);

        // 게임 종료(사망) 시점 점수 자동 제출. 로그인 상태일 때만 ScoreManager 내부에서 기록됨.
        ScoreManager.Instance?.SubmitRun(rewardController.Score, roundManager.CurrentRound, roundManager.RunTime).Forget();

        currentTime = restartTimer;
        isActive = true;
        if (backgroundImage != null) bgStartColor = backgroundImage.color;

        UIManager.EnsureInstance().RegisterOverlayOpened(this);

        // 결과 블록 전체를 숨김 (라벨 포함). 시간초과 후 ShowResult에서 켠다.
        if (resultGroup != null) resultGroup.SetActive(false);
    }

    private void Update()
    {
        UpdateTimer();
    }

    private void UpdateTimer()
    {
        if (!isActive) return;

        currentTime -= Time.deltaTime;
        if (timerText != null)
            timerText.text = $"{Mathf.FloorToInt(currentTime)}s";

        if (backgroundImage != null)
        {
            float progress = Mathf.Clamp01(1f - currentTime / restartTimer);
            backgroundImage.color = Color.Lerp(bgStartColor, Color.black, progress);
        }

        if (currentTime <= 0f)
        {
            isActive = false;
            ShowResult();
        }
    }

    private void ShowResult()
    {
        if (retryButton != null) retryButton.gameObject.SetActive(false);
        if (restartLabel != null) restartLabel.SetActive(false);
        if (timerText != null) timerText.gameObject.SetActive(false);
        if (gameOverText != null) gameOverText.SetActive(false);
        if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(true); // 버튼은 순차 연출에서 제외, 즉시 표시

        // 결과 블록 켜고, 코드는 값(숫자)만 0부터 굴린다. 라벨은 씬 담당.
        if (resultGroup != null) resultGroup.SetActive(true);

        int total = roundManager.TotalRounds;

        // 순서 = Col_L 위→아래, 그 다음 Col_R.
        var items = new List<StatRollup.Item>
        {
            new StatRollup.Item { text = waveResultText,     target = roundManager.CurrentRound,        fmt = n => $"{n} / {total}" },
            new StatRollup.Item { text = killResultText,     target = EnemyRegistry.KilledCount,        fmt = n => n.ToString() },
            new StatRollup.Item { text = scoreResultText,    target = rewardController.Score,           fmt = n => n.ToString() },
            new StatRollup.Item { text = damageResultText,   target = (int)damageCalc.TotalDamageDealt, fmt = n => n.ToString() },
            new StatRollup.Item { text = headshotResultText, target = damageCalc.HeadshotCount,         fmt = n => n.ToString() },
            new StatRollup.Item { text = moneyResultText,    target = rewardController.TotalEarned,     fmt = n => n.ToString() },
            new StatRollup.Item { text = timeResultText,     target = (int)roundManager.RunTime,        fmt = n => FormatTime(n) },
        };
        StatRollup.Cascade(items, rollDuration, gap, this.GetCancellationTokenOnDestroy()).Forget();
    }

    private string FormatTime(int sec)
    {
        return $"{sec / 60:00}:{sec % 60:00}";
    }

    private void OnMainMenuClicked()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnRetryClicked()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }
}
