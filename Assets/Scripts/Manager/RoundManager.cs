using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

// 실제 적 스폰은 EnemySpawner에 명령한다. (RoundManager → EnemySpawner 단방향)
// 클리어 UI는 직접 만지지 않고 이벤트로 알린다 (RoundUIPresenter/GameManager가 소비).
public class RoundManager : MonoBehaviour
{
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private RewardController rewardController;

    [Header("라운드 설정")]
    [SerializeField] private int totalRounds = 10;
    [SerializeField] private float roundDuration = 60f;
    [SerializeField] private float roundEndDelay = 3f;
    [SerializeField] private float ShoppingTimer = 5f;

    public event Action<int, int> OnRoundChanged;   // (currentRound, totalRounds)
    public event Action<bool> OnRoundCleared;       // 클리어 배너 표시 (isFinal=최종 여부)
    public event Action OnRoundClearHidden;         // 클리어 배너 숨김
    public event Action OnAllRoundsCleared;         // 최종 클리어 (게임 전체 상태로 승격)

    public int CurrentRound => currentRound;
    public int TotalRounds => totalRounds;
    public float RunTime => runTime; // 이번 판 진행 시간(라운드+상점, 일시정지 제외). 끝화면이 읽음.
    public float RoundTimer => roundTimer;
    public float ShopTimer => shopTimer;
    public bool IsRoundActive => isRoundActive;
    public bool IsShopPhase => isShopPhase;

    private int currentRound = 0;
    private float roundTimer;
    private float shopTimer;
    private bool isRoundActive = false;
    private bool isShopPhase = false;
    private float runTime;
    private CancellationTokenSource roundCts; // 라운드 루프 취소원 (HaltRounds/파괴 시 취소)

    private void Awake()
    {
        // 씬(라운드) 재시작 시 정적 처치/생존 상태 초기화
        EnemyRegistry.Reset();
    }

    private void Start()
    {
        playerSpawner.Freeze();
    }

    private void Update()
    {
        if (isRoundActive || isShopPhase) runTime += Time.deltaTime; // 일시정지 시 timeScale=0 → deltaTime 0

        if (!isRoundActive) return;

        roundTimer -= Time.deltaTime;

        bool timerExpired = roundTimer <= 0f;

        if (timerExpired || IsRoundCleared())
            EndRound();
    }

    // 라운드 클리어 = 스폰 완료(스포너 없으면 무조건 완료) + 생존 적 0.
    private bool IsRoundCleared()
    {
        bool spawnComplete = enemySpawner == null || enemySpawner.IsSpawningComplete;
        return spawnComplete && EnemyRegistry.AliveCount == 0;
    }

    //인트로 진입 (플레이어 배치)
    public void BeginIntro()
    {
        playerSpawner.PlaceForIntro();
    }

    //첫 라운드 루프 시작
    public void BeginFirstRound()
    {
        roundCts = CancellationTokenSource.CreateLinkedTokenSource(
            this.GetCancellationTokenOnDestroy()
        );
        FirstRoundRoutine(roundCts.Token).Forget();
    }

    //진행 중인 라운드/태스크/스폰 정지
    public void HaltRounds()
    {
        isRoundActive = false;
        isShopPhase = false;
        roundCts?.Cancel();
        if (enemySpawner != null) enemySpawner.StopSpawning();
    }

    private void OnDestroy()
    {
        roundCts?.Cancel();
        roundCts?.Dispose();
    }

    private async UniTaskVoid FirstRoundRoutine(CancellationToken token)
    {
        playerSpawner.ResetForRound();
        isShopPhase = true;
        currentRound++;
        OnRoundChanged?.Invoke(currentRound, totalRounds);
        shopTimer = ShoppingTimer;
        while (shopTimer > 0f)
        {
            shopTimer -= Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        StartRound();
    }

    private void StartRound()
    {
        isShopPhase = false;
        if (currentRound > totalRounds)
            return;

        playerSpawner.Unfreeze();
        isRoundActive = true;
        roundTimer = roundDuration;

        if (enemySpawner != null) enemySpawner.StartSpawning(currentRound);
    }

    private void EndRound()
    {
        isRoundActive = false;

        if (enemySpawner != null) enemySpawner.StopSpawning();

        if (currentRound >= totalRounds)
        {
            FinalClearSequence(roundCts.Token).Forget();
            return;
        }

        OnRoundCleared?.Invoke(false);
        NextRoundRoutine(roundCts.Token).Forget();
    }

    private async UniTaskVoid FinalClearSequence(CancellationToken token)
    {
        OnRoundCleared?.Invoke(true);
        await UniTask.Delay(TimeSpan.FromSeconds(roundEndDelay), cancellationToken: token);
        rewardController.AddRoundClearReward();
        OnRoundClearHidden?.Invoke();
        playerSpawner.Freeze();
        OnAllRoundsCleared?.Invoke();
    }

    private async UniTaskVoid NextRoundRoutine(CancellationToken token)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(roundEndDelay), cancellationToken: token);
        OnRoundClearHidden?.Invoke();
        rewardController.AddRoundClearReward();
        playerSpawner.ResetForRound();
        isShopPhase = true;
        currentRound++;
        OnRoundChanged?.Invoke(currentRound, totalRounds);
        shopTimer = ShoppingTimer;
        while (shopTimer > 0f)
        {
            shopTimer -= Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        StartRound();
    }
}
