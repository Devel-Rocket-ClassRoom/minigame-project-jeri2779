using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseInitializer : MonoBehaviour
{
    private static FirebaseInitializer instance;
    public static FirebaseInitializer Instance => instance;

    public enum InitState
    {
        Pending,
        Ready,
        Failed,
    }

    public InitState State { get; private set; } = InitState.Pending;
    public bool IsReady => State == InitState.Ready;
    public string LastError { get; private set; }

    public FirebaseApp App { get; private set; }
    public FirebaseDatabase Database { get; private set; }
    public FirebaseAuth Auth { get; private set; }

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

        InitializeFirebaseAsync().Forget();
    }

    private async UniTaskVoid InitializeFirebaseAsync()
    {
        FirebaseLog.Log("Init", "초기화 시작 — 의존성 검사 중...");

        try
        {
            DependencyStatus status = await FirebaseApp.CheckAndFixDependenciesAsync().AsUniTask();
            FirebaseLog.Log("Init", $"의존성 검사 결과: {status}");

            if (status != DependencyStatus.Available)
            {
                Fail($"의존성 오류: {status}");
                return;
            }

            App = FirebaseApp.DefaultInstance;
            // DB URL이 찍히면 google-services(json)가 제대로 읽힌 결정적 증거.
            FirebaseLog.Log("Init", $"DefaultInstance 획득 (app={App.Name}, dbUrl={App.Options.DatabaseUrl})");

            Database = FirebaseDatabase.GetInstance(App);
            FirebaseLog.Log("Init", "Database 인스턴스 획득");

            Auth = FirebaseAuth.GetAuth(App);
            FirebaseLog.Log("Init", $"Auth 인스턴스 획득 (현재 로그인 유저={(Auth.CurrentUser != null ? Auth.CurrentUser.UserId : "없음")})");

            State = InitState.Ready;
            FirebaseLog.Log("Init", "✅ 초기화 성공 — Ready");
        }
        catch (System.Exception ex)
        {
            Fail(ex.Message);
        }
    }

    private void Fail(string error)
    {
        LastError = error;
        State = InitState.Failed;
        FirebaseLog.Error("Init", $"❌ 초기화 실패: {error}");
    }

    // 초기화 완료까지 대기. true=성공, false=실패. (Auth/Profile 등 다른 매니저의 진입 게이트)
    public async UniTask<bool> WaitForInitializationAsync()
    {
        await UniTask.WaitUntil(() => State != InitState.Pending);
        return State == InitState.Ready;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
