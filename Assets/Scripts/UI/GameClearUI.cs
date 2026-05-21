using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameClearUI : MonoBehaviour
{
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private RewardController rewardController;

    [SerializeField] private GameObject playingHUD;
    [SerializeField] private GameObject crossHair;
    [SerializeField] private GameObject fpWeapon;

    [SerializeField] private Button mainMenuButton;
    [SerializeField] private TextMeshProUGUI waveResultText;
    [SerializeField] private TextMeshProUGUI killResultText;
    [SerializeField] private TextMeshProUGUI scoreResultText;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    public void Show()
    {
        gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playingHUD != null) playingHUD.SetActive(false);
        if (crossHair != null) crossHair.SetActive(false);
        if (fpWeapon != null) fpWeapon.SetActive(false);

        if (waveResultText != null && enemySpawner != null)
            waveResultText.text = $"라운드 {enemySpawner.TotalRounds} / {enemySpawner.TotalRounds}";
        if (killResultText != null && enemySpawner != null)
            killResultText.text = $"처치 수 {enemySpawner.KilledCount}";
        if (scoreResultText != null && rewardController != null)
            scoreResultText.text = $"점수 {rewardController.Score}";
    }

    private void OnMainMenuClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
