using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PausePanel : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject settingsPanel;

    private GameObject panelRoot;
    private bool isPaused = false;

    private void Awake()
    {
        panelRoot = transform.Find("PausePanel").gameObject;
        backButton.onClick.AddListener(Resume);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(Quit);
        panelRoot.SetActive(false);

        UIManager.EnsureInstance().SetFallbackEscape(HandleEscape);
    }

    private void HandleEscape()
    {
        // 설정 패널은 열려 있으면 자기 ESC 핸들러(SettingsPanel)가 스택 상단에서 먼저 처리한다.
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
