using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// UserInfoPanel: 현재 로그인 유저의 닉네임/이메일 표시 + 로그아웃 + 닉네임 수정 진입.
// ESC/닫기 생명주기는 SettingsPanel 패턴을 따른다.
public class UserInfoPanel : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI nicknameText;

    [SerializeField]
    private TextMeshProUGUI emailText;

    [SerializeField]
    private Button customNicknameButton;

    [SerializeField]
    private Button logoutButton;

    [SerializeField]
    private Button closeButton;

    [SerializeField]
    private GameObject customNamePanel;

    private bool pushed;

    private void Awake()
    {
        if (customNicknameButton != null)
            customNicknameButton.onClick.AddListener(OnCustomNicknameClicked);
        if (logoutButton != null)
            logoutButton.onClick.AddListener(OnLogoutClicked);
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    private void OnEnable()
    {
        var ui = UIManager.Instance;
        if (ui != null)
        {
            ui.PushEscape(Close);
            pushed = true;
        }
        Refresh().Forget();
    }

    private void OnDisable()
    {
        if (!pushed)
            return;
        pushed = false;
        UIManager.Instance?.PopEscape();
    }

    private async UniTaskVoid Refresh()
    {
        if (AuthManager.Instance == null || !AuthManager.Instance.IsLoggedIn)
        {
            if (nicknameText != null)
                nicknameText.text = "-";
            if (emailText != null)
                emailText.text = "비로그인";
            return;
        }

        string email = AuthManager.Instance.CurrentUser?.Email;
        if (emailText != null)
            emailText.text = string.IsNullOrEmpty(email) ? "익명 계정" : email;

        // 캐시 우선, 없으면 로드
        var profile = ProfileManager.Instance != null ? ProfileManager.Instance.cachedProfile : null;
        if (profile == null && ProfileManager.Instance != null)
        {
            var (loaded, _) = await ProfileManager.Instance.LoadProfileAsync();
            profile = loaded;
        }
        if (nicknameText != null)
            nicknameText.text = profile != null ? profile.nickname : "(닉네임 없음)";
    }

    private void OnCustomNicknameClicked()
    {
        if (customNamePanel == null)
            return;
        // 편집 중엔 정보 패널을 숨긴다. 수정 패널을 닫으면 CustomNamePanel이 이 패널을 복귀시킨다.
        customNamePanel.SetActive(true);
        gameObject.SetActive(false);
    }

    private void OnLogoutClicked()
    {
        AuthManager.Instance?.SignOut();
        Close();
    }

    public void Close() => gameObject.SetActive(false);
}
