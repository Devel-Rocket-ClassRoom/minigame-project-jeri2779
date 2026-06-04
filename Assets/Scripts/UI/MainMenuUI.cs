using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
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
    }

    private void Start()
    {
        if (mainMenuPanel != null && mainMenuPanel.activeSelf)
            UIManager.EnsureInstance().RegisterOverlayOpened(this);
    }

    private void Update()
    {
        if (quitModal == null) return;
        if (!mainMenuPanel.activeSelf) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            quitModal.AnimateWindow();
    }

    private void OnStartClicked()
    {
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        mainMenuPanel.SetActive(false);
        UIManager.EnsureInstance().RegisterOverlayClosed(this);

        gameManager.BeginIntro();
        playingHUD.SetActive(true);
        gameStartText.gameObject.SetActive(true);

        yield return new WaitForSeconds(introDelay);

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
