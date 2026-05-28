using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private RewardController rewardController;

   
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private GameObject restartLabel;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject gameOverText;
    [SerializeField] private float restartTimer = 10f;

 
    [SerializeField] private GameObject waveResultText;
    [SerializeField] private GameObject killResultText;
    [SerializeField] private GameObject scoreResultText;

    private float currentTime;
    private bool isActive = false;

    private void Awake()
    {
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        currentTime = restartTimer;
        isActive = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (waveResultText != null) waveResultText.SetActive(false);
        if (killResultText != null) killResultText.SetActive(false);
        if (scoreResultText != null) scoreResultText.SetActive(false);
    }

    private void Update()
    {
        // 다른 시스템(ShopUI 등)이 커서를 잠그더라도 게임오버 동안엔 무조건 풀린 상태 유지
        if (Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        UpdateTimer();
    }

    private void UpdateTimer()
    {
        if (!isActive) return;

        currentTime -= Time.deltaTime;
        if (timerText != null)
            timerText.text = $"{Mathf.FloorToInt(currentTime)}s";

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

        if (waveResultText != null)
        {
            waveResultText.SetActive(true);
            waveResultText.GetComponent<TextMeshProUGUI>().text =
                $"도달 웨이브  {enemySpawner.CurrentRound} / {enemySpawner.TotalRounds}";
        }
        if (killResultText != null)
        {
            killResultText.SetActive(true);
            killResultText.GetComponent<TextMeshProUGUI>().text =
                $"처치 수  {enemySpawner.KilledCount}";
        }
        if (scoreResultText != null)
        {
            scoreResultText.SetActive(true);
            scoreResultText.GetComponent<TextMeshProUGUI>().text =
                $"점수  {rewardController.Score}";
        }

        if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(true);
    }

    private void OnMainMenuClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnRetryClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
