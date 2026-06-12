using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Michsky.UI.ModernUIPack;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject playingHUD;
    [SerializeField] private TextMeshProUGUI gameStartText;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private ModalWindowManager quitModal;
    [SerializeField] private Button quitConfirmButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private float introDelay = 2f;

    private void Awake()
    {
        startButton.onClick.AddListener(OnStartClicked);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
        if (quitConfirmButton != null)
            quitConfirmButton.onClick.AddListener(OnQuitClicked);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        mainMenuPanel.SetActive(true);
        playingHUD.SetActive(false);

        UIManager.EnsureInstance().RegisterOverlayOpened(this);
        UIManager.EnsureInstance().PushEscape(HandleEscape);
    }

    private void Start()
    {
        if (mainMenuPanel != null && mainMenuPanel.activeSelf)
            UIManager.EnsureInstance().RegisterOverlayOpened(this);
    }

    private void HandleEscape()
    {
        // 설정 패널은 열려 있으면 자기 ESC 핸들러(SettingsPanel)가 스택 상단에서 먼저 처리한다.
        quitModal?.AnimateWindow();
    }

    private void OnStartClicked()
    {
        IntroSequence().Forget();
    }

    private async UniTaskVoid IntroSequence()
    {
        mainMenuPanel.SetActive(false);
        UIManager.EnsureInstance().PopEscape();
        UIManager.EnsureInstance().RegisterOverlayClosed(this);

        gameManager.BeginIntro();
        playingHUD.SetActive(true);
        gameStartText.gameObject.SetActive(true);

        await UniTask.Delay(
            TimeSpan.FromSeconds(introDelay),
            cancellationToken: this.GetCancellationTokenOnDestroy()
        );

        gameStartText.gameObject.SetActive(false);
        gameManager.StartGame();
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
