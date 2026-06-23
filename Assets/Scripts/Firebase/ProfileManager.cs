using System;
using Cysharp.Threading.Tasks;
using Firebase.Database;
using UnityEngine;

 
public class ProfileManager : MonoBehaviour
{
    private static ProfileManager instance;
    public static ProfileManager Instance => instance;

    private DatabaseReference databaseRef;
    private DatabaseReference usersRef;

    private UserProfile _cachedProfile;
    public UserProfile cachedProfile => _cachedProfile;

    private bool isInitialized;
    public bool IsInitialized => isInitialized;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private async UniTaskVoid Start()
    {
        if (!await FirebaseInitializer.Instance.WaitForInitializationAsync())
        {
            FirebaseLog.Error("Profile", "firebase 초기화 실패 — profile 초기화 불가");
            return;
        }

        databaseRef = FirebaseInitializer.Instance.Database.RootReference;
        usersRef = databaseRef.Child("users");

        AuthManager.Instance.LoginStateChanged += OnLoginStateChanged;

        // 시작 시 이미 로그인돼 있으면 프로필 보장
        await EnsureProfileAsync();

        isInitialized = true;
        FirebaseLog.Log("Profile", "초기화 완료");
    }

    private void OnDestroy()
    {
        if (AuthManager.Instance != null)
        {
            AuthManager.Instance.LoginStateChanged -= OnLoginStateChanged;
        }
    }

    private void OnLoginStateChanged(bool signedIn)
    {
        if (signedIn)
        {
            EnsureProfileAsync().Forget();
        }
    }

    // 로그인한 모든 유저가 users/{uid}에 프로필을 갖도록 보장.
    // (이메일 유저=이메일 앞부분, 익명=게스트+uid 앞 4자리)
    public async UniTask EnsureProfileAsync()
    {
        if (AuthManager.Instance == null || !AuthManager.Instance.IsLoggedIn)
        {
            return;
        }

        var (profile, _) = await LoadProfileAsync();
        if (profile != null)
        {
            return; // 이미 존재
        }

        string email = AuthManager.Instance.CurrentUser?.Email;
        string defaultNick;
        if (!string.IsNullOrEmpty(email))
        {
            int at = email.IndexOf('@');
            defaultNick = at > 0 ? email.Substring(0, at) : email;
        }
        else
        {
            string uid = AuthManager.Instance.UserId;
            defaultNick = "게스트" + (uid.Length >= 4 ? uid.Substring(0, 4) : uid);
        }

        await SaveProfileAsync(defaultNick);
        FirebaseLog.Log("Profile", $"프로필 자동 생성: {defaultNick}");
    }

    public async UniTask<(bool success, string error)> SaveProfileAsync(string nickname)
    {
        if (!AuthManager.Instance.IsLoggedIn)
        {
            return (false, "로그인 필요");
        }

        string userId = AuthManager.Instance.UserId;
        string email = AuthManager.Instance.CurrentUser.Email ?? "익명";

        try
        {
            FirebaseLog.Log("Profile", "프로필 저장 시도");

            UserProfile profile = new UserProfile(nickname, email);
            string json = profile.ToJson();

            await usersRef.Child(userId).SetRawJsonValueAsync(json);
            _cachedProfile = profile;

            FirebaseLog.Log("Profile", "프로필 저장 성공");
            return (true, null);
        }
        catch (Exception ex)
        {
            FirebaseLog.Error("Profile", $"프로필 저장 실패 {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async UniTask<(UserProfile profile, string error)> LoadProfileAsync()
    {
        if (!AuthManager.Instance.IsLoggedIn)
        {
            return (null, "로그인 필요");
        }

        string userId = AuthManager.Instance.UserId;

        try
        {
            FirebaseLog.Log("Profile", "프로필 로드 시도");

            DataSnapshot snapshot = await usersRef.Child(userId).GetValueAsync();
            if (!snapshot.Exists)
            {
                FirebaseLog.Log("Profile", "프로필 없음");
                return (null, "프로필 존재 X");
            }

            string json = snapshot.GetRawJsonValue();

            UserProfile profile = UserProfile.FromJson(json);
            _cachedProfile = profile;
            FirebaseLog.Log("Profile", "프로필 로드 성공");
            return (profile, null);
        }
        catch (Exception ex)
        {
            FirebaseLog.Error("Profile", $"프로필 로드 실패 {ex.Message}");
            return (null, ex.Message);
        }
    }

    public async UniTask<(bool success, string error)> UpdateNickNameAsync(string nickname)
    {
        if (!AuthManager.Instance.IsLoggedIn)
        {
            return (false, "로그인 필요");
        }

        string userId = AuthManager.Instance.UserId;

        try
        {
            FirebaseLog.Log("Profile", "닉네임 수정 시도");
            // 전체 노드가 아니라 nickname 필드만 갱신 (email/createdAt 보존)
            await usersRef.Child(userId).Child("nickname").SetValueAsync(nickname);
            if (_cachedProfile != null)
            {
                _cachedProfile.nickname = nickname;
            }

            FirebaseLog.Log("Profile", "닉네임 수정 성공");
            if (LeaderBoardManager.Instance != null)
            {
                await LeaderBoardManager.Instance.UpdateNickNameAsync(nickname);
            }
            return (true, null);
        }
        catch (Exception ex)
        {
            FirebaseLog.Error("Profile", $"닉네임 수정 실패 {ex.Message}");
            return (false, ex.Message);
        }
    }
}
