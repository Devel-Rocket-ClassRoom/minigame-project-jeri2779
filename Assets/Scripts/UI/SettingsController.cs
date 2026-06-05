using System.Collections.Generic;
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
    [SerializeField] private Toggle invertXToggle;
    [SerializeField] private Toggle invertYToggle;
    [SerializeField] private Toggle aimToggleUI;    // on=토글 조준, off=홀드
    [SerializeField] private Toggle sprintToggleUI; // on=토글 달리기, off=홀드

    [Header("Graphics")]
    [SerializeField] private HorizontalSelector fpsSelector; // 0:30 1:60 2:120 3:무제한
    [SerializeField] private HorizontalSelector qualitySelector; // QualitySettings 단계
    [SerializeField] private CustomDropdown resolutionDropdown; // 런타임에 기기 해상도로 채움
    [SerializeField] private HorizontalSelector windowModeSelector; // 0:전체화면 1:창모드

    [Header("Gameplay")]
    [SerializeField] private Slider fovSlider;
    [SerializeField] private Toggle hudToggle;
    [SerializeField] private GameObject playingHud;
    [SerializeField] private Toggle controlsGuideToggle;
    [SerializeField] private GameObject controlsGuide; // 중앙 하단 조작 안내 패널

    [Header("Crosshair")]
    [SerializeField] private HorizontalSelector crosshairSelector;
    [SerializeField] private Image crosshairImage;   // 게임플레이 실제 크로스헤어
    [SerializeField] private Image crosshairPreview; // 설정창 내 미리보기
    [SerializeField] private Sprite[] crosshairSprites;
    [SerializeField] private HorizontalSelector crosshairColorSelector;
    [SerializeField] private Color[] crosshairColors;

    [Header("Reset")]
    [SerializeField] private Button resetButton;
    [SerializeField] private ModalWindowManager resetModal;
    [SerializeField] private Button resetConfirmButton;

    [Header("Player Seams ")]
    [SerializeField] private CharacterMoves characterMoves;
    [SerializeField] private PlayerShooter playerShooter;

    [Header("Play Time Display")]
    [SerializeField] private TextMeshProUGUI playTimeTextSettings;
    [SerializeField] private TextMeshProUGUI playTimeTextPause;

    private static readonly int[] FpsByIndex = { 30, 60, 120, -1 };
    private List<Vector2Int> availableResolutions; // 드롭다운 인덱스 → 해상도 매핑

    private float totalPlayTime;
    private bool isPlaying;
    private bool isInitialized = true;

    private void Start()
    {
        BuildResolutionDropdown();
        if (windowModeSelector != null)
            windowModeSelector.onValueChanged.AddListener(SetWindowModeByIndex);
        if (crosshairSelector != null)
            crosshairSelector.onValueChanged.AddListener(SetCrosshairByIndex);
        if (crosshairColorSelector != null)
            crosshairColorSelector.onValueChanged.AddListener(SetCrosshairColorByIndex);
        if (aimToggleUI != null)
            aimToggleUI.onValueChanged.AddListener(SetAimToggle);
        if (sprintToggleUI != null)
            sprintToggleUI.onValueChanged.AddListener(SetSprintToggle);
        if (resetButton != null && resetModal != null)
            resetButton.onClick.AddListener(resetModal.OpenWindow);
        if (resetConfirmButton != null)
            resetConfirmButton.onClick.AddListener(OnResetConfirmed);
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

    public void SetInvertX(bool value)
    {
        if (isInitialized) return;
        if (characterMoves != null) characterMoves.SetInvertX(value);
        if (SaveManager.Instance != null) SaveManager.Instance.CurrentData.invertMouseX = value;
    }

    public void SetInvertY(bool value)
    {
        if (isInitialized) return;
        if (characterMoves != null) characterMoves.SetInvertY(value);
        if (SaveManager.Instance != null) SaveManager.Instance.CurrentData.invertMouseY = value;
    }

    public void SetAimToggle(bool value)
    {
        if (isInitialized) return;
        if (playerShooter != null) playerShooter.SetAimToggle(value);
        if (SaveManager.Instance != null) SaveManager.Instance.CurrentData.aimToggle = value;
    }

    public void SetSprintToggle(bool value)
    {
        if (isInitialized) return;
        if (characterMoves != null) characterMoves.SetSprintToggle(value);
        if (SaveManager.Instance != null) SaveManager.Instance.CurrentData.sprintToggle = value;
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

    public void SetControlsGuideVisible(bool value)
    {
        if (isInitialized) return;
        if (controlsGuide != null) controlsGuide.SetActive(value);
        if (SaveManager.Instance != null) SaveManager.Instance.CurrentData.controlsGuideVisible = value;
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

    // 해상도 드롭다운을 기기에서 쓸 수 있는 해상도로 채운다(런타임 동적). Start에서 ApplySettingData 전에 호출.
    private void BuildResolutionDropdown()
    {
        if (resolutionDropdown == null) return;

        availableResolutions = new List<Vector2Int>();
        foreach (Resolution r in Screen.resolutions)
        {
            var res = new Vector2Int(r.width, r.height);
            if (!availableResolutions.Contains(res)) availableResolutions.Add(res); // 주사율 중복 제거
        }

        resolutionDropdown.enableIcon = false; // 닫힌 상태 선택 아이콘 제거
        resolutionDropdown.dropdownItems.Clear();
        foreach (var res in availableResolutions)
            resolutionDropdown.CreateNewItemFast($"{res.x} x {res.y}", null);
        resolutionDropdown.SetupDropdown();

        // 항목 아이콘 제거: itemObject 템플릿이 공유 prefab(Imported)이라 prefab 대신
        // 생성된 항목들의 Icon만 끈다(이 드롭다운 한정).
        if (resolutionDropdown.itemParent != null)
            foreach (Transform itemTr in resolutionDropdown.itemParent)
            {
                Transform icon = itemTr.Find("Icon");
                if (icon != null) icon.gameObject.SetActive(false);
            }

        resolutionDropdown.dropdownEvent.AddListener(SetResolutionByIndex);
    }

    // 저장된 해상도(없거나 목록에 없으면 현재 해상도)에 해당하는 드롭다운 인덱스
    private int ResolveResolutionIndex(SaveData data)
    {
        if (availableResolutions == null || availableResolutions.Count == 0) return -1;
        if (data.resolutionWidth > 0)
        {
            int i = availableResolutions.IndexOf(new Vector2Int(data.resolutionWidth, data.resolutionHeight));
            if (i >= 0) return i;
        }
        return availableResolutions.IndexOf(new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height));
    }

    // 드롭다운 dropdownEvent 에 연결
    public void SetResolutionByIndex(int index)
    {
        if (isInitialized) return;
        if (availableResolutions == null || index < 0 || index >= availableResolutions.Count) return;
        var res = availableResolutions[index];
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.CurrentData.resolutionWidth = res.x;
            SaveManager.Instance.CurrentData.resolutionHeight = res.y;
        }
        ApplyScreen();
    }

    // 창모드 셀렉터 onValueChanged 에 연결 (0:전체화면 1:창모드)
    public void SetWindowModeByIndex(int index)
    {
        if (isInitialized) return;
        if (SaveManager.Instance != null)
            SaveManager.Instance.CurrentData.fullscreen = (index == 0);
        ApplyScreen();
    }

    // 해상도+창모드를 함께 적용(한쪽만 바꿔도 다른 쪽 보존). ※ 빌드에서만 실제 반영됨(에디터 무효).
    private void ApplyScreen()
    {
        if (SaveManager.Instance == null) return;
        SaveData data = SaveManager.Instance.CurrentData;
        int w = data.resolutionWidth > 0 ? data.resolutionWidth : Screen.currentResolution.width;
        int h = data.resolutionHeight > 0 ? data.resolutionHeight : Screen.currentResolution.height;
        var mode = data.fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.SetResolution(w, h, mode);
    }

    // 크로스헤어 셀렉터 onValueChanged 에 연결
    public void SetCrosshairByIndex(int index)
    {
        if (isInitialized) return;
        ApplyCrosshair(index);
        if (SaveManager.Instance != null)
            SaveManager.Instance.CurrentData.crosshairIndex = index;
    }

    private void ApplyCrosshair(int index)
    {
        if (crosshairSprites == null || crosshairSprites.Length == 0) return;
        index = Mathf.Clamp(index, 0, crosshairSprites.Length - 1);
        Sprite sp = crosshairSprites[index];
        if (crosshairImage != null) crosshairImage.sprite = sp;
        if (crosshairPreview != null) crosshairPreview.sprite = sp;
    }

    // 크로스헤어 색 셀렉터 onValueChanged 에 연결
    public void SetCrosshairColorByIndex(int index)
    {
        if (isInitialized) return;
        ApplyCrosshairColor(index);
        if (SaveManager.Instance != null)
            SaveManager.Instance.CurrentData.crosshairColorIndex = index;
    }

    private void ApplyCrosshairColor(int index)
    {
        if (crosshairColors == null || crosshairColors.Length == 0) return;
        index = Mathf.Clamp(index, 0, crosshairColors.Length - 1);
        Color c = crosshairColors[index];
        if (crosshairImage != null) crosshairImage.color = c;
        if (crosshairPreview != null) crosshairPreview.color = c;
    }

    public void ResetSettings()
    {
        if (SaveManager.Instance == null) return;
        SaveManager.Instance.ResetData();
        ApplySettingData();
    }

    // 확인 모달의 '확인' 버튼에 연결 — 초기화 실행 후 모달 닫기
    private void OnResetConfirmed()
    {
        ResetSettings();
        if (resetModal != null) resetModal.CloseWindow();
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
            characterMoves.SetInvertX(data.invertMouseX);
            characterMoves.SetInvertY(data.invertMouseY);
        }
        if (mouseSensitivitySlider != null) mouseSensitivitySlider.value = data.mouseSensitivity;
        if (invertXToggle != null) invertXToggle.isOn = data.invertMouseX;
        if (invertYToggle != null) invertYToggle.isOn = data.invertMouseY;

        if (playerShooter != null) playerShooter.SetAimToggle(data.aimToggle);
        if (aimToggleUI != null) aimToggleUI.isOn = data.aimToggle;
        if (characterMoves != null) characterMoves.SetSprintToggle(data.sprintToggle);
        if (sprintToggleUI != null) sprintToggleUI.isOn = data.sprintToggle;

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

        // Screen (해상도 + 창모드)
        int resIndex = ResolveResolutionIndex(data);
        if (resolutionDropdown != null && resIndex >= 0)
            resolutionDropdown.ChangeDropdownInfo(resIndex); // 이벤트 미발생 → Set* 재호출 안 됨
        if (windowModeSelector != null && windowModeSelector.itemList.Count > 0)
        {
            windowModeSelector.index = data.fullscreen ? 0 : 1;
            windowModeSelector.defaultIndex = windowModeSelector.index;
            windowModeSelector.UpdateUI();
        }
        ApplyScreen();

        // Gameplay
        if (playerShooter != null) playerShooter.SetDefaultFOV(data.fov);
        if (fovSlider != null) fovSlider.value = data.fov;
        if (playingHud != null) playingHud.SetActive(data.hudVisible);
        if (hudToggle != null) hudToggle.isOn = data.hudVisible;
        if (controlsGuide != null) controlsGuide.SetActive(data.controlsGuideVisible);
        if (controlsGuideToggle != null) controlsGuideToggle.isOn = data.controlsGuideVisible;

        // Crosshair (모양 + 색)
        ApplyCrosshair(data.crosshairIndex);
        if (crosshairSelector != null && crosshairSelector.itemList.Count > 0)
        {
            crosshairSelector.index = Mathf.Clamp(data.crosshairIndex, 0, crosshairSelector.itemList.Count - 1);
            crosshairSelector.defaultIndex = crosshairSelector.index;
            crosshairSelector.UpdateUI();
        }
        ApplyCrosshairColor(data.crosshairColorIndex);
        if (crosshairColorSelector != null && crosshairColorSelector.itemList.Count > 0)
        {
            crosshairColorSelector.index = Mathf.Clamp(data.crosshairColorIndex, 0, crosshairColorSelector.itemList.Count - 1);
            crosshairColorSelector.defaultIndex = crosshairColorSelector.index;
            crosshairColorSelector.UpdateUI();
        }

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
