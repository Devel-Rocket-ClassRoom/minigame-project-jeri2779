using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Database;
using UnityEngine;

// scores/{uid} 점수 저장/최고기록/히스토리. Round+Score 기준.
// 게임 종료 시 호출 진입점 = SubmitRun (실제 호출 배선은 #5 GameOverUI/GameClearUI).
public class ScoreManager : MonoBehaviour
{
    private static ScoreManager instance;
    public static ScoreManager Instance => instance;

    private DatabaseReference scoresRef;

    private int cachedBestScore;
    private int cachedBestRound;
    public int CachedBestScore => cachedBestScore;
    public int CachedBestRound => cachedBestRound;

    // scoresRef 초기화(Start 완료) 여부 — UI/외부가 조기 진입 시 대기용
    public bool IsReady => scoresRef != null;

    private Query historyQuery;
    private EventHandler<ValueChangedEventArgs> historyHandler;

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
            FirebaseLog.Error("Score", "firebase 초기화 X");
            return;
        }
        await UniTask.WaitUntil(() => AuthManager.Instance.IsInitialized);
        await UniTask.WaitUntil(() => ProfileManager.Instance.IsInitialized);

        scoresRef = FirebaseInitializer.Instance.Database.RootReference.Child("scores");
        FirebaseLog.Log("Score", "초기화 완료");

        AuthManager.Instance.LoginStateChanged += OnLoginStatusChanged;

        if (AuthManager.Instance.IsLoggedIn)
        {
            await SyncBestScoreToLeaderboardAsync();
        }
    }

    private void OnDestroy()
    {
        if (AuthManager.Instance != null)
        {
            AuthManager.Instance.LoginStateChanged -= OnLoginStatusChanged;
        }
        StopHistoryListener();
    }

    // 게임 종료 시 진입점. fire-and-forget(.Forget()) 또는 await 둘 다 가능하도록 UniTask 반환.
    public UniTask SubmitRun(int score, int round, float playTimeSec)
    {
        return SaveAndUpdateBestAsync(score, round, playTimeSec);
    }

    private void OnLoginStatusChanged(bool signIn)
    {
        if (signIn)
        {
            SyncBestScoreToLeaderboardAsync().Forget();
        }
        else
        {
            cachedBestScore = 0;
            cachedBestRound = 0;
        }
    }

    private async UniTask SyncBestScoreToLeaderboardAsync()
    {
        int best = await LoadBestScoreAsync();
        if (best <= 0 || LeaderBoardManager.Instance == null)
        {
            return;
        }

        var (success, error) = await LeaderBoardManager.Instance.SaveToLeaderboard(best, cachedBestRound);
        if (success)
        {
            FirebaseLog.Log("Score", $"{best}점 최고 기록 리더보드 동기화");
        }
        else
        {
            FirebaseLog.Warn("Score", $"최고 기록 리더보드 동기화 실패: {error}");
        }
    }

    private async UniTask SaveAndUpdateBestAsync(int score, int round, float playTimeSec)
    {
        var (success, error) = await SaveScoreAsync(score, round, playTimeSec);
        if (!success)
        {
            FirebaseLog.Warn("Score", $"점수 저장 실패: {error}");
            return;
        }

        int best = await LoadBestScoreAsync();
        if (score > best)
        {
            // 최고기록 갱신 성공 시에만 리더보드 반영 (부분 실패 시 best/리더보드 불일치 방지)
            bool bestUpdated = await UpdateBestScoreAsync(score, round);

            if (bestUpdated && LeaderBoardManager.Instance != null)
            {
                var (lbSuccess, lbError) = await LeaderBoardManager.Instance.SaveToLeaderboard(score, round);
                if (!lbSuccess)
                {
                    FirebaseLog.Warn("Score", $"리더보드 갱신 실패: {lbError}");
                }
            }
        }
    }

    public async UniTask<(bool success, string error)> SaveScoreAsync(int score, int round, float playTimeSec)
    {
        if (!AuthManager.Instance.IsLoggedIn)
        {
            return (false, "로그인 필요");
        }

        string userId = AuthManager.Instance.UserId;

        try
        {
            FirebaseLog.Log("Score", "점수 저장 시도");

            DatabaseReference newHistoryRef = scoresRef.Child(userId).Child("history").Push();
            Dictionary<string, object> scoreData = new Dictionary<string, object>()
            {
                { "score", score },
                { "round", round },
                { "timestamp", ServerValue.Timestamp },
                { "playTimeSec", playTimeSec },
            };
            await newHistoryRef.UpdateChildrenAsync(scoreData);

            FirebaseLog.Log("Score", $"점수 저장 성공 score={score} round={round}");
            return (true, null);
        }
        catch (Exception ex)
        {
            FirebaseLog.Error("Score", $"점수 저장 실패 {ex.Message}");
            return (false, ex.Message);
        }
    }

    private async UniTask<bool> UpdateBestScoreAsync(int newBestScore, int newBestRound)
    {
        string userId = AuthManager.Instance.UserId;

        try
        {
            await scoresRef.Child(userId)
                .UpdateChildrenAsync(new Dictionary<string, object>()
                {
                    { "bestscore", newBestScore },
                    { "bestround", newBestRound },
                });
            cachedBestScore = newBestScore;
            cachedBestRound = newBestRound;
            return true;
        }
        catch (Exception ex)
        {
            FirebaseLog.Error("Score", $"최고 기록 갱신 실패: {ex.Message}");
            return false;
        }
    }

    public async UniTask<int> LoadBestScoreAsync()
    {
        // 초기화 전(scoresRef==null)·비로그인 진입 시 NRE 방지
        if (!AuthManager.Instance.IsLoggedIn || scoresRef == null)
        {
            return 0;
        }

        string userId = AuthManager.Instance.UserId;

        try
        {
            DataSnapshot scoreSnap = await scoresRef.Child(userId).Child("bestscore").GetValueAsync();
            DataSnapshot roundSnap = await scoresRef.Child(userId).Child("bestround").GetValueAsync();

            cachedBestScore = scoreSnap.Exists ? FirebaseValue.ToInt(scoreSnap.Value) : 0;
            cachedBestRound = roundSnap.Exists ? FirebaseValue.ToInt(roundSnap.Value) : 0;
            FirebaseLog.Log("Score", $"최고 점수 로드 score={cachedBestScore} round={cachedBestRound}");
            return cachedBestScore;
        }
        catch (Exception ex)
        {
            FirebaseLog.Warn("Score", $"최고 점수 로드 실패: {ex.Message}");
            return 0;
        }
    }

    public async UniTask<List<ScoreData>> LoadHistoryAsync(int limit = 10)
    {
        if (!AuthManager.Instance.IsLoggedIn || scoresRef == null)
        {
            FirebaseLog.Log("Score", "히스토리 로드 — 로그인 X");
            return new List<ScoreData>();
        }

        string userId = AuthManager.Instance.UserId;

        try
        {
            Query query = scoresRef.Child(userId).Child("history").OrderByChild("timestamp").LimitToLast(limit);
            DataSnapshot snapshot = await query.GetValueAsync();

            List<ScoreData> historyList = ParseHistory(snapshot);
            FirebaseLog.Log("Score", $"히스토리 로드: {historyList.Count}");
            return historyList;
        }
        catch (Exception ex)
        {
            FirebaseLog.Error("Score", $"히스토리 로드 실패 {ex.Message}");
            return new List<ScoreData>();
        }
    }

    // 히스토리 실시간 구독 (HistoryPanel의 자동갱신 토글용)
    public void StartHistoryListener(int limit, Action<List<ScoreData>> onUpdate)
    {
        if (!AuthManager.Instance.IsLoggedIn || scoresRef == null || onUpdate == null)
        {
            return;
        }

        StopHistoryListener();

        string userId = AuthManager.Instance.UserId;
        historyQuery = scoresRef.Child(userId).Child("history").LimitToLast(limit);
        historyHandler = (sender, args) =>
        {
            if (args.DatabaseError != null)
            {
                FirebaseLog.Error("Score", $"히스토리 실시간 오류: {args.DatabaseError.Message}");
                return;
            }
            onUpdate(ParseHistory(args.Snapshot));
        };
        historyQuery.ValueChanged += historyHandler;
        FirebaseLog.Log("Score", "히스토리 실시간 구독 시작");
    }

    public void StopHistoryListener()
    {
        if (historyQuery != null && historyHandler != null)
        {
            historyQuery.ValueChanged -= historyHandler;
        }
        historyQuery = null;
        historyHandler = null;
    }

    private List<ScoreData> ParseHistory(DataSnapshot snapshot)
    {
        List<ScoreData> list = new List<ScoreData>();
        if (snapshot != null && snapshot.Exists)
        {
            foreach (DataSnapshot child in snapshot.Children)
            {
                list.Add(JsonUtility.FromJson<ScoreData>(child.GetRawJsonValue()));
            }
            list.Reverse(); // 키 오름차순 → 최신이 위로
        }
        return list;
    }
}
