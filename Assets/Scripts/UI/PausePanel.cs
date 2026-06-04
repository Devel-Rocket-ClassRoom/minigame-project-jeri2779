using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PausePanel : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject mainMenuPanel;

    private GameObject panelRoot;
    private bool isPaused = false;

    private void Awake()
    {
        panelRoot = transform.Find("PausePanel").gameObject;
        backButton.onClick.AddListener(Resume);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(Quit);
        panelRoot.SetActive(false);
    }

    private void Update()
    {
        if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;

        // 메인메뉴 표시 중에는 MainMenuUI가, 상점 열림 중에는 ShopUI가 esc를 전담한다
        if (mainMenuPanel != null && mainMenuPanel.activeSelf) return;
        if (shopPanel != null && shopPanel.activeSelf) return;

        // 설정창이 열려 있으면 esc는 설정창만 닫는다 (pause는 그대로 유지)
        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            settingsPanel.SetActive(false);
            return;
        }

        SetPause(!isPaused);
    }

    private void SetPause(bool pause)
    {
        isPaused = pause;
        Time.timeScale = isPaused ? 0f : 1f;

        if (isPaused)
            UIManager.EnsureInstance().ShowOverlay(panelRoot, this);
        else
            UIManager.EnsureInstance().HideOverlay(panelRoot, this);
    }

    private void Resume()
    {
        SetPause(false);
    }

    private void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    private void Quit()
    {
        Time.timeScale = 1f;

        if (isPaused)
        {
            isPaused = false;
            UIManager.EnsureInstance().HideOverlay(panelRoot, this);
        }

        SceneManager.LoadScene(0);
    }
}
