using System;
using Cysharp.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;

public class AuthManager : MonoBehaviour
{
    private static AuthManager instance;
    public static AuthManager Instance => instance;

    private FirebaseAuth auth;
    private FirebaseUser currentUser;
    private bool isInitialized = false;
    private bool lastNotifiedSignedIn = false;

    public FirebaseUser CurrentUser => currentUser;
    public bool IsLoggedIn => currentUser != null;
    public string UserId => currentUser?.UserId ?? string.Empty;
    public bool IsInitialized => isInitialized;

    public event Action<bool> LoginStateChanged;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        if (auth != null)
        {
            auth.StateChanged -= OnAuthStateChanged;
        }

        if (instance == this)
        {
            instance = null;
        }
    }

    private async UniTaskVoid Start()
    {
        bool isReady = await FirebaseInitializer.Instance.WaitForInitializationAsync();
        if (!isReady)
        {
            FirebaseLog.Error("Auth", "Firebase 초기화 실패 — Auth 사용 불가");
            return;
        }

        auth = FirebaseInitializer.Instance.Auth;
        auth.StateChanged += OnAuthStateChanged;

        currentUser = auth.CurrentUser;
        FirebaseLog.Log("Auth", currentUser != null ? "이미 로그인 됨" : "로그인 필요");

        isInitialized = true;
        NotifyLoginState();
    }

    private void OnAuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth.CurrentUser != currentUser)
        {
            currentUser = auth.CurrentUser;
        }

        NotifyLoginState();
    }

    private void NotifyLoginState()
    {
        bool signedIn = IsLoggedIn;
        if (signedIn == lastNotifiedSignedIn)
            return;

        lastNotifiedSignedIn = signedIn;

        FirebaseLog.Log("Auth", signedIn ? $"로그인 상태 : {UserId}" : "로그아웃");
        LoginStateChanged?.Invoke(signedIn);
    }

    public async UniTask<(bool success, string error)> SignInAnonAsync()
    {
        if (!isInitialized || auth == null)
        {
            return (false, "인증이 아직 초기화되지 않았습니다.");
        }

        try
        {
            AuthResult result = await auth.SignInAnonymouslyAsync().AsUniTask();
            currentUser = result.User;
            NotifyLoginState();

            FirebaseLog.Log("Auth", $"익명 로그인 성공: {currentUser.UserId}");
            return (true, null);
        }
        catch (Exception ex)
        {
            FirebaseLog.Log("Auth", $"익명 로그인 실패: {ex.Message}");
            return (false, ParseFirebaseError(GetErrorMessage(ex)));
        }
    }

    public async UniTask<(bool success, string error)> CreateUserWithEmailAsync(string email, string password)
    {
        if (!isInitialized || auth == null)
        {
            return (false, "인증이 아직 초기화되지 않았습니다.");
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
        {
            return (false, "이메일과 비밀번호를 입력해주세요.");
        }

        try
        {
            AuthResult result = await auth.CreateUserWithEmailAndPasswordAsync(email, password).AsUniTask();
            currentUser = result.User;
            NotifyLoginState();

            FirebaseLog.Log("Auth", $"회원가입 성공: {currentUser.UserId}");
            return (true, null);
        }
        catch (Exception ex)
        {
            FirebaseLog.Log("Auth", $"회원가입 실패: {ex.Message}");
            return (false, ParseFirebaseError(GetErrorMessage(ex)));
        }
    }

    public async UniTask<(bool success, string error)> SignInUserWithEmailAsync(string email, string password)
    {
        if (!isInitialized || auth == null)
        {
            return (false, "인증이 아직 초기화되지 않았습니다.");
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
        {
            return (false, "이메일과 비밀번호를 입력해주세요.");
        }

        try
        {
            AuthResult result = await auth.SignInWithEmailAndPasswordAsync(email, password).AsUniTask();
            currentUser = result.User;
            NotifyLoginState();

            FirebaseLog.Log("Auth", $"로그인 성공: {UserId}");
            return (true, null);
        }
        catch (Exception ex)
        {
            FirebaseLog.Log("Auth", $"로그인 실패: {ex.Message}");
            return (false, ParseFirebaseError(GetErrorMessage(ex)));
        }
    }

    public void SignOut()
    {
        if (auth == null)
            return;

        auth.SignOut();
        currentUser = null;
        NotifyLoginState();

        FirebaseLog.Log("Auth", "로그아웃");
    }

    private string GetErrorMessage(Exception ex)
    {
        Exception inner = ex;
        while (inner.InnerException != null)
        {
            inner = inner.InnerException;
        }
        return inner.Message;
    }

    private string ParseFirebaseError(string error)
    {
        FirebaseLog.Warn("Auth", $"Firebase 에러 원문: {error}");

        string lower = error.ToLowerInvariant();

        if (lower.Contains("already in use") || lower.Contains("email-already"))
        {
            return "이미 사용 중인 이메일입니다.";
        }
        if (lower.Contains("at least 6") || lower.Contains("weak") || lower.Contains("password is invalid"))
        {
            return "비밀번호는 6자 이상이어야 합니다.";
        }
        if (lower.Contains("badly formatted") || lower.Contains("invalid-email"))
        {
            return "이메일 형식이 올바르지 않습니다.";
        }
        if (lower.Contains("network"))
        {
            return "네트워크 연결을 확인해주세요.";
        }

        return "이메일 또는 비밀번호를 확인해주세요.";
    }
}
