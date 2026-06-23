using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using UnityEngine.PlayerLoop;

 public class LeaderBoardManager : MonoBehaviour
{
    private static LeaderBoardManager instance;
    public static LeaderBoardManager Instance => instance;

    private const int DefaultTopCount = 10;

    private DatabaseReference leaderboardRef;
    public bool IsReady => leaderboardRef != null;
    private Query listenerQuery;
    private bool isListenerActive;
    public event Action<List<LeaderboardEntry>> OnLeaderboardUpdated;

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

    private async UniTaskVoid Start()
    {
        if (!await FirebaseInitializer.Instance.WaitForInitializationAsync())
        {
            FirebaseLog.Error("Leaderboard", "Firebase 초기화 실패");
            return;
        }

        leaderboardRef = FirebaseInitializer.Instance.Database.RootReference.Child("leaderboard");
        FirebaseLog.Log("Leaderboard", "초기화 완료");
    }

    private void OnDestroy()
    {
        StopRealtimeListener();
    }

    public async UniTask<(bool success, string error)> SaveToLeaderboard(int score, int round)
    {
        if (!AuthManager.Instance.IsLoggedIn)
        {
            return (false, "로그인 필요");
        }

        if (leaderboardRef == null)
        {
            return (false, "leaderboardRef 미초기화");
        }

        string userId = AuthManager.Instance.UserId;
        string nickname = ProfileManager.Instance != null ? ProfileManager.Instance.cachedProfile?.nickname : null;

   
        if (string.IsNullOrEmpty(nickname) && ProfileManager.Instance != null)
        {
            var (profile, _) = await ProfileManager.Instance.LoadProfileAsync();
            nickname = profile?.nickname;
        }

        if (string.IsNullOrEmpty(nickname))
        {
            nickname = "게스트";
        }

        try
        {
            Dictionary<string, object> entryData = new Dictionary<string, object>
            {
                { "userId", userId },
                { "nickname", nickname },
                { "score", score },
                { "round", round },
                { "timestamp", ServerValue.Timestamp },
            };

            await leaderboardRef.Child(userId).UpdateChildrenAsync(entryData);
            FirebaseLog.Log("Leaderboard", $"갱신 {nickname} score={score} round={round}");
            return (true, null);
        }
        catch (Exception ex)
        {
            FirebaseLog.Error("Leaderboard", $"저장 실패: {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async UniTask UpdateNickNameAsync(string nickname)
    {
        if (!AuthManager.Instance.IsLoggedIn || leaderboardRef == null)
            return;

        string userId = AuthManager.Instance.UserId;

        try
        {
            DataSnapshot snapshot = await leaderboardRef.Child(userId).GetValueAsync();
            if (!snapshot.Exists)
                return;

            await leaderboardRef.Child(userId).Child("nickname").SetValueAsync(nickname);
            FirebaseLog.Log("Leaderboard", $"닉네임 동기화 {nickname}");
        }
        catch (Exception ex)
        {
            FirebaseLog.Error("Leaderboard", $"닉네임 동기화 실패: {ex.Message}");
        }
    }

    public async UniTask<List<LeaderboardEntry>> LoadLeaderboardAsync(int topN = DefaultTopCount)
    {
        if (leaderboardRef == null)
        {
            return new List<LeaderboardEntry>();
        }

        try
        {
            Query query = leaderboardRef.OrderByChild("score").LimitToLast(topN);
            DataSnapshot snapshot = await query.GetValueAsync();

            List<LeaderboardEntry> leaderboard = ParseEntries(snapshot);
            FirebaseLog.Log("Leaderboard", $"로드 {leaderboard.Count}명");
            return leaderboard;
        }
        catch (Exception ex)
        {
            FirebaseLog.Error("Leaderboard", $"로드 실패: {ex.Message}");
            return new List<LeaderboardEntry>();
        }
    }

    public List<LeaderboardEntry> ParseEntries(DataSnapshot snapshot)
    {
        List<LeaderboardEntry> list = new List<LeaderboardEntry>();
        if (snapshot != null && snapshot.Exists)
        {
            foreach (DataSnapshot child in snapshot.Children)
            {
                list.Add(LeaderboardEntry.FromJson(child.GetRawJsonValue()));
            }
            list.Reverse();  
        }
        return list;
    }

    public void StartRealtimeListener(int limit = DefaultTopCount)
    {
        if (leaderboardRef == null)
        {
            FirebaseLog.Warn("Leaderboard", "leaderboardRef 미초기화 — 실시간 리스너 시작 불가");
            return;
        }

        StopRealtimeListener(); 

        FirebaseLog.Log("Leaderboard", "실시간 리스너 시작");
        listenerQuery = leaderboardRef.OrderByChild("score").LimitToLast(limit);
        listenerQuery.ValueChanged += OnValueChanged;
        isListenerActive = true;
    }

    public void StopRealtimeListener()
    {
        if (isListenerActive && listenerQuery != null)
        {
            FirebaseLog.Log("Leaderboard", "실시간 리스너 중지");
            listenerQuery.ValueChanged -= OnValueChanged;
            listenerQuery = null;
            isListenerActive = false;
        }
    }

    public void OnValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            FirebaseLog.Error("Leaderboard", $"리스너 에러: {args.DatabaseError.Message}");
            return;
        }

        List<LeaderboardEntry> leaderboard = ParseEntries(args.Snapshot);
        DispatchUpdateAsync(leaderboard).Forget();
    }

    private async UniTaskVoid DispatchUpdateAsync(List<LeaderboardEntry> leaderboard)
    {
        await UniTask.SwitchToMainThread();
        OnLeaderboardUpdated?.Invoke(leaderboard);
    }
}
