using Michsky.UI.ModernUIPack;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("Sounds")]
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider seVolumeSlider;

    [Header("Controls")]
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Toggle invertYToggle;

    [Header("Graphics")]
    [SerializeField] private HorizontalSelector fpsSelector; // 0:30 1:60 2:120 3:무제한
    [SerializeField] private HorizontalSelector qualitySelector; // QualitySettings 단계

    [Header("Gameplay")]
    [SerializeField] private Slider fovSlider;
    [SerializeField] private Toggle hudToggle;
    [SerializeField] private GameObject playingHud;

    [Header("Player Seams ")]
    [SerializeField] private CharacterMoves characterMoves;
    [SerializeField] private PlayerShooter playerShooter;

    [Header("Play Time Display")]
    [SerializeField] private TextMeshProUGUI playTimeTextSettings;
    [SerializeField] private TextMeshProUGUI playTimeTextPause;

    private static readonly int[] FpsByIndex = { 30, 60, 120, -1 };

    private float totalPlayTime;
    private bool isPlaying;
    private bool isInitialized = true;

    private void Start()
    {
        ApplySettingData();
    }

    private void Update()
    {
        if (!isPlaying || SaveManager.Instance == null)
            return;
        totalPlayTime += Time.unscaledDeltaTime;
        SaveManager.Instance.CurrentData.totalPlayTime = totalPlayTime;
        UpdateTimeText();
    }

    public void StartCount() => isPlaying = true;

    public void StopCount() => isPlaying = false;

    // ── 위젯 OnValueChanged 에 연결 ──

    public void SetBGMVolume(float value)
    {
        if (isInitialized) return;
        AudioManager.Instance?.SetBGMVolume(value);
    }

    public void SetSEVolume(float value)
    {
        if (isInitialized) return;
        AudioManager.Instance?.SetSEVolume(value);
    }

    public void SetMouseSensitivity(float value)
    {
        if (isInitialized) return;
        if (characterMoves != null) characterMoves.SetMouseSensitivity(value);
        if (SaveManager.Instance != null) SaveManager.Instance.CurrentData.mouseSensitivity = value;
    }

    public void SetInvertY(bool value)
    {
        if (isInitialized) return;
        if (characterMoves != null) characterMoves.SetInvertY(value);
        if (SaveManager.Instance != null) SaveManager.Instance.CurrentData.invertMouseY = value;
    }

    public void SetFov(float value)
    {
        if (isInitialized) return;
        if (playerShooter != null) playerShooter.SetDefaultFOV(value);
        if (SaveManager.Instance != null) SaveManager.Instance.CurrentData.fov = value;
    }

    public void SetHudVisible(bool value)
    {
        if (isInitialized) return;
        if (playingHud != null) playingHud.SetActive(value);
        if (SaveManager.Instance != null) SaveManager.Instance.CurrentData.hudVisible = value;
    }

    // HorizontalSelector.onValueChanged(int) 에 연결
    public void SetFrameRateByIndex(int index)
    {
        if (isInitialized) return;
        int fps = FpsByIndex[Mathf.Clamp(index, 0, FpsByIndex.Length - 1)];
        Application.targetFrameRate = fps;
        if (SaveManager.Instance != null) SaveManager.Instance.CurrentData.targetFPS = fps;
    }

    // HorizontalSelector.onValueChanged(int) 에 연결
    public void SetQualityByIndex(int index)
    {
        if (isInitialized) return;
        QualitySettings.SetQualityLevel(index, true);
        if (SaveManager.Instance != null) SaveManager.Instance.CurrentData.qualityLevel = index;
    }

    public void ResetSettings()
    {
        if (SaveManager.Instance == null) return;
        SaveManager.Instance.ResetData();
        ApplySettingData();
    }

    // SaveData → UI/도메인 일괄 반영. 시작 시(Start) + 리셋 시 호출.
    public void ApplySettingData()
    {
        if (SaveManager.Instance == null) return;
        SaveData data = SaveManager.Instance.CurrentData;

        isInitialized = true;

        // Sounds
        if (bgmVolumeSlider != null) bgmVolumeSlider.value = data.bgmVolume;
        if (seVolumeSlider != null) seVolumeSlider.value = data.seVolume;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(data.bgmVolume);
            AudioManager.Instance.SetSEVolume(data.seVolume);
        }

        // Controls
        if (characterMoves != null)
        {
            characterMoves.SetMouseSensitivity(data.mouseSensitivity);
            characterMoves.SetInvertY(data.invertMouseY);
        }
        if (mouseSensitivitySlider != null) mouseSensitivitySlider.value = data.mouseSensitivity;
        if (invertYToggle != null) invertYToggle.isOn = data.invertMouseY;

        // Graphics
        Application.targetFrameRate = data.targetFPS;
        if (fpsSelector != null && fpsSelector.itemList.Count > 0)
        {
            fpsSelector.index = IndexOfFps(data.targetFPS);
            fpsSelector.defaultIndex = fpsSelector.index;
            fpsSelector.UpdateUI();
        }
        if (data.qualityLevel >= 0) QualitySettings.SetQualityLevel(data.qualityLevel, true);
        if (qualitySelector != null && qualitySelector.itemList.Count > 0)
        {
            qualitySelector.index = Mathf.Clamp(QualitySettings.GetQualityLevel(), 0, qualitySelector.itemList.Count - 1);
            qualitySelector.defaultIndex = qualitySelector.index;
            qualitySelector.UpdateUI();
        }

        // Gameplay
        if (playerShooter != null) playerShooter.SetDefaultFOV(data.fov);
        if (fovSlider != null) fovSlider.value = data.fov;
        if (playingHud != null) playingHud.SetActive(data.hudVisible);
        if (hudToggle != null) hudToggle.isOn = data.hudVisible;

        totalPlayTime = data.totalPlayTime;
        UpdateTimeText();

        isInitialized = false;
    }

    private static int IndexOfFps(int fps)
    {
        for (int i = 0; i < FpsByIndex.Length; i++)
            if (FpsByIndex[i] == fps) return i;
        return 1; // 기본 60
    }

    private void UpdateTimeText()
    {
        string formatted = FormatTime(totalPlayTime);
        if (playTimeTextSettings != null) playTimeTextSettings.text = formatted;
        if (playTimeTextPause != null) playTimeTextPause.text = formatted;
    }

    private string FormatTime(float time)
    {
        int hours = (int)(time / 3600);
        int minutes = (int)((time % 3600) / 60);
        int seconds = (int)(time % 60);
        return $"{hours:00}:{minutes:00}:{seconds:00}";
    }
}
