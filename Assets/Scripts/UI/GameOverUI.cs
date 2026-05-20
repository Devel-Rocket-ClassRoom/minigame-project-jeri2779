using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private RewardController rewardController;

   
    [SerializeField] private Button retryButton;
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
    }

    public void Show()
    {
        gameObject.SetActive(true);
        currentTime = restartTimer;
        isActive = true;

        if (waveResultText != null) waveResultText.SetActive(false);
        if (killResultText != null) killResultText.SetActive(false);
        if (scoreResultText != null) scoreResultText.SetActive(false);
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
    }

    private void OnRetryClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
