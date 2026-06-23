using Cysharp.Threading.Tasks;
using Michsky.UI.ModernUIPack;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField emailInput;

    [SerializeField]
    private TMP_InputField passwordInput;

    [SerializeField]
    private Button signInButton;

    [SerializeField]
    private Button signUpButton;

    [SerializeField]
    private Button anonButton;

    [SerializeField]
    private NotificationManager errorPopup;

    private bool pushed;
    private bool busy; // 중복 클릭 방지

    private void Awake()
    {
        signInButton.onClick.AddListener(OnSignInClicked);
        signUpButton.onClick.AddListener(OnSignUpClicked);
        if (anonButton != null)
        {
            anonButton.onClick.AddListener(OnAnonClicked);
        }
    }

    private void OnEnable()
    {
        var ui = UIManager.Instance;
        if (ui == null)
            return;
        ui.PushEscape(Close);
        pushed = true;
    }

    private void OnDisable()
    {
        if (!pushed)
            return;
        pushed = false;
        UIManager.Instance?.PopEscape();
    }

    public void Close() => gameObject.SetActive(false);

    private void OnSignInClicked() => RunAuth(AuthMode.SignIn).Forget();

    private void OnSignUpClicked() => RunAuth(AuthMode.SignUp).Forget();

    private void OnAnonClicked() => RunAuth(AuthMode.Anon).Forget();

    private enum AuthMode
    {
        SignIn,
        SignUp,
        Anon,
    }

    private async UniTaskVoid RunAuth(AuthMode mode)
    {
        if (busy)
            return;

        if (AuthManager.Instance == null || !AuthManager.Instance.IsInitialized)
        {
            ShowError("잠시 후 다시 시도해주세요. (초기화 중)");
            return;
        }

        busy = true;

        string email = emailInput != null ? emailInput.text.Trim() : string.Empty;
        string pwd = passwordInput != null ? passwordInput.text : string.Empty;

        (bool success, string error) result;
        switch (mode)
        {
            case AuthMode.SignUp:
                result = await AuthManager.Instance.CreateUserWithEmailAsync(email, pwd);
                break;
            case AuthMode.Anon:
                result = await AuthManager.Instance.SignInAnonAsync();
                break;
            default:
                result = await AuthManager.Instance.SignInUserWithEmailAsync(email, pwd);
                break;
        }
        busy = false;

        if (result.success)
        {
            Close();
        }
        else
        {
            ShowError(result.error);
        }
    }

    private void ShowError(string message)
    {
        if (errorPopup == null)
        {
            FirebaseLog.Warn("LoginUI", $"errorPopup 미할당 — {message}");
            return;
        }

        errorPopup.title = "로그인 오류";
        errorPopup.description = message;
        errorPopup.UpdateUI();
        errorPopup.OpenNotification();
    }
}
