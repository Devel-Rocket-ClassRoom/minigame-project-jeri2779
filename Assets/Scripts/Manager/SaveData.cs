 
// 설정값은 SettingsController.
public class SaveData
{
    // ── 설정값 ──
    public float bgmVolume = 0.5f;
    public float seVolume = 0.5f;
    public int targetFPS = 60;
    public float fov = 60f;
    public float mouseSensitivity = 0.1f;
    public bool invertMouseX = false;
    public bool invertMouseY = false;
    public int qualityLevel = -1; // -1 = 프로젝트 기본값 사용
    public int resolutionWidth = 0; // 0 = 현재 해상도 사용
    public int resolutionHeight = 0;
    public bool fullscreen = true;
    
    public bool hudVisible = true;
    public int crosshairIndex = 0; // 크로스헤어 모양 프리셋 인덱스
    public int crosshairColorIndex = 0; // 크로스헤어 색 프리셋 인덱스 (0=흰색)
    public bool aimToggle = false; // 조준: false=홀드, true=토글
    public bool sprintToggle = false; // 달리기: false=홀드, true=토글
    public string keyBindings = ""; // New Input System 바인딩 오버라이드(JSON)

    // ── 누적 통계 ──
    public int stageClearCount = 0; // 라운드 클리어 수
    public int totalMoneyEarned = 0; // 모은 총재화
    public int totalMoneySpent = 0; // 소모 총재화
    public float totalDamageDealt = 0f; // 가한 총 피해
    public int headshotCount = 0; // 헤드샷 수
    public float totalPlayTime = 0f; // 누적 플레이타임(초)
}
