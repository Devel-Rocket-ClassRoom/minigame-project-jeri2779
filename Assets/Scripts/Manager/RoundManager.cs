using System;
using System.Collections;
using UnityEngine;

// 라운드 진행 조율자. 라운드/상점 페이즈 상태기계를 소유하고,
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
    public float RoundTimer => roundTimer;
    public float ShopTimer => shopTimer;
    public bool IsRoundActive => isRoundActive;
    public bool IsShopPhase => isShopPhase;

    private int currentRound = 0;
    private float roundTimer;
    private float shopTimer;
    private bool isRoundActive = false;
    private bool isShopPhase = false;

    private void Start()
    {
        playerSpawner.Freeze();
    }

    private void Update()
    {
        if (!isRoundActive) return;

        roundTimer -= Time.deltaTime;

        bool timerExpired = roundTimer <= 0f;
        bool allCleared = enemySpawner.IsAllCleared;

        if (timerExpired || allCleared)
            EndRound();
    }

    // GameManager가 호출 — 인트로 진입 (플레이어 배치)
    public void BeginIntro()
    {
        playerSpawner.PlaceForIntro();
    }

    // GameManager가 호출 — 첫 라운드 루프 시작
    public void BeginFirstRound()
    {
        StartCoroutine(FirstRoundRoutine());
    }

    // GameManager가 호출 — 진행 중인 라운드/코루틴/스폰 정지
    public void HaltRounds()
    {
        isRoundActive = false;
        isShopPhase = false;
        StopAllCoroutines();
        enemySpawner.StopSpawning();
    }

    private IEnumerator FirstRoundRoutine()
    {
        playerSpawner.ResetForRound();
        isShopPhase = true;
        currentRound++;
        OnRoundChanged?.Invoke(currentRound, totalRounds);
        shopTimer = ShoppingTimer;
        while (shopTimer > 0f)
        {
            shopTimer -= Time.deltaTime;
            yield return null;
        }
        StartRound();
    }

    private void StartRound()
    {
        isShopPhase = false;
        if (currentRound > totalRounds)
        {
            Debug.Log("전체 라운드 클리어!");
            return;
        }

        playerSpawner.Unfreeze();
        isRoundActive = true;
        roundTimer = roundDuration;

        enemySpawner.StartSpawning(currentRound);

        Debug.Log($"Round {currentRound}/{totalRounds} 시작 ");
    }

    private void EndRound()
    {
        isRoundActive = false;

        string clearType = enemySpawner.IsAllCleared ? "적 전부 처치" : "시간 만료";
        enemySpawner.StopSpawning();
        Debug.Log($" Round {currentRound}/{totalRounds} 클리어 ({clearType})");

        if (currentRound >= totalRounds)
        {
            StartCoroutine(FinalClearSequence());
            return;
        }

        OnRoundCleared?.Invoke(false);
        StartCoroutine(NextRoundRoutine());
    }

    private IEnumerator FinalClearSequence()
    {
        OnRoundCleared?.Invoke(true);
        yield return new WaitForSeconds(roundEndDelay);
        rewardController.AddRoundClearReward();
        OnRoundClearHidden?.Invoke();
        playerSpawner.Freeze();
        OnAllRoundsCleared?.Invoke();
    }

    private IEnumerator NextRoundRoutine()
    {
        yield return new WaitForSeconds(roundEndDelay);
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
            yield return null;
        }
        StartRound();
    }
}
