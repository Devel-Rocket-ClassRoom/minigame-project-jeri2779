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
        if (Keyboard.current.escapeKey.wasPressedThisFrame
            && (shopPanel == null || !shopPanel.activeSelf)
            && (mainMenuPanel == null || !mainMenuPanel.activeSelf))
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
