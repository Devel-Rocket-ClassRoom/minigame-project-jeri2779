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

        await LoadProfileAsync();

        isInitialized = true;
        FirebaseLog.Log("Profile", "초기화 완료");
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
