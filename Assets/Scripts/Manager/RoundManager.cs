using System;
using System.Collections;
using TMPro;
using UnityEngine;

// 라운드 진행 조율자. 라운드/상점 페이즈 상태기계를 소유하고,
// 실제 적 스폰은 EnemySpawner에 명령한다. (RoundManager → EnemySpawner 단방향)
public class RoundManager : MonoBehaviour
{
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private WeaponInventoryNew weaponInventory;
    [SerializeField] private RewardController rewardController;
    [SerializeField] private GameClearUI gameClearUI;
    [SerializeField] private GameObject roundClearPanel;
    [SerializeField] private TextMeshProUGUI finalClearText;

    [Header("라운드 설정")]
    [SerializeField] private int totalRounds = 10;
    [SerializeField] private float roundDuration = 60f;
    [SerializeField] private float roundEndDelay = 3f;
    [SerializeField] private float ShoppingTimer = 5f;

    public event Action<int, int> OnRoundChanged;   // (currentRound, totalRounds)

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
    private CharacterMoves characterMoves;

    private void Start()
    {
        characterMoves = player.GetComponent<CharacterMoves>();
        characterMoves.SetMovable(false);
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
        characterMoves.Teleport(playerSpawnPoint.position);
        characterMoves.SetMovable(true);
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
        characterMoves.SetMovable(false);
        characterMoves.Teleport(playerSpawnPoint.position);
        player.GetComponent<CharacterHealth>().ResetHealth();
        weaponInventory?.ResetAmmo();
        characterMoves.ResetStamina();
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

        characterMoves.SetMovable(true);
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

        if (roundClearPanel != null) roundClearPanel.SetActive(true);
        StartCoroutine(NextRoundRoutine());
    }

    private IEnumerator FinalClearSequence()
    {
        if (roundClearPanel != null) roundClearPanel.SetActive(true);
        if (finalClearText != null) finalClearText.gameObject.SetActive(true);
        yield return new WaitForSeconds(roundEndDelay);
        rewardController.AddRoundClearReward();
        if (roundClearPanel != null) roundClearPanel.SetActive(false);
        if (finalClearText != null) finalClearText.gameObject.SetActive(false);
        characterMoves.SetMovable(false);
        if (gameClearUI != null) gameClearUI.Show();
    }

    private IEnumerator NextRoundRoutine()
    {
        yield return new WaitForSeconds(roundEndDelay);
        if (roundClearPanel != null) roundClearPanel.SetActive(false);
        rewardController.AddRoundClearReward();
        characterMoves.SetMovable(false);
        characterMoves.Teleport(playerSpawnPoint.position);
        player.GetComponent<CharacterHealth>().ResetHealth();
        weaponInventory?.ResetAmmo();
        characterMoves.ResetStamina();
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
