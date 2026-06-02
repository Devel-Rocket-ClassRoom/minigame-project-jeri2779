using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;  
 
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private WaveData waveData;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private WeaponInventoryNew weaponInventory;
    [SerializeField] private GameClearUI gameClearUI;
    [SerializeField] private GameObject roundClearPanel;
    [SerializeField] private TextMeshProUGUI finalClearText;
    [SerializeField] private RewardController rewardController;

    [Header("라운드 설정")]
    [SerializeField] private int totalRounds = 10;
    [SerializeField] private float roundDuration = 60f;
    [SerializeField] private float roundEndDelay = 3f;
    [SerializeField] private float ShoppingTimer = 5f;

    [Header("스폰 설정")]
    [SerializeField] private int maxAliveEnemies = 15;
    [SerializeField] private int spawnBatchSize = 3;
    [SerializeField] private float spawnStartDelay = 3f;
    [SerializeField] private float spawnInterval = 3f;

    public event Action<int, int> OnRoundChanged;   // (currentRound, totalRounds)

    public int CurrentRound => currentRound;
    public int TotalRounds => totalRounds;
    public float RoundTimer => roundTimer;
    public float ShopTimer => shopTimer;
    public bool IsRoundActive => isRoundActive;
    public bool IsShopPhase => isShopPhase;
    public int KilledCount => totalKilledCount;
    private int currentRound = 0;
    private float roundTimer;
    private float shopTimer;
    private float spawnTimer;
    private int totalEnemiesThisRound;
    private int killedEnemiesThisRound;
    private int totalKilledCount;
    private readonly List<GameObject> aliveEnemies = new List<GameObject>();
    private List<EnemySpawnEntry> pendingSpawns = new List<EnemySpawnEntry>();
    private bool isRoundActive = false;
    private bool isShopPhase = false;
    private CharacterMoves characterMoves;
    private CharacterStats characterStats;

    private void Start()
    {
        characterMoves = player.GetComponent<CharacterMoves>();
        characterStats = player.GetComponent<CharacterStats>();
        characterMoves.SetMovable(false);

    }

    private void Update()
    {
        if (!isRoundActive) return;

        aliveEnemies.RemoveAll(enemy => enemy == null || !enemy.activeInHierarchy);

        roundTimer -= Time.deltaTime;

        bool timerExpired = roundTimer <= 0f;
        bool allCleared = pendingSpawns.Count == 0 && aliveEnemies.Count == 0;

        if (timerExpired || allCleared)
        {
            EndRound();
            return;
        }

        if (pendingSpawns.Count > 0 && aliveEnemies.Count < maxAliveEnemies)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnBatch();
                spawnTimer = spawnInterval;
            }
        }
    }
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

    // GameManager가 호출 — 진행 중인 라운드/코루틴 정지
    public void HaltRounds()
    {
        isRoundActive = false;
        isShopPhase = false;
        StopAllCoroutines();
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
        spawnTimer = spawnStartDelay;
        aliveEnemies.Clear();

        killedEnemiesThisRound = 0;
        pendingSpawns = GetEntriesForRound(currentRound);
        totalEnemiesThisRound = 0;
        foreach (var e in pendingSpawns) totalEnemiesThisRound += e.count;

        Debug.Log($"Round {currentRound}/{totalRounds} 시작 ");
        Debug.Log($"  적 {totalEnemiesThisRound}마리 예정 | 제한시간 {roundDuration}초");
    }

    private void SpawnBatch()
    {
        int remaining = maxAliveEnemies - aliveEnemies.Count;
        int count = Mathf.Min(spawnBatchSize, remaining);

        for (int i = pendingSpawns.Count - 1; i >= 0 && count > 0; i--)
        {
            
            Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemy = Instantiate(pendingSpawns[i].prefab, sp.position, sp.rotation);
            enemy.GetComponent<EnemyController>().Initialize(player);
            enemy.GetComponent<EnemyHealth>().onKilled += OnEnemyKilled;
            aliveEnemies.Add(enemy);

            pendingSpawns[i].count--;
            if (pendingSpawns[i].count <= 0)
                pendingSpawns.RemoveAt(i);

            count--;
        }
    }

    private void OnEnemyKilled(EnemyData data)
    {
        killedEnemiesThisRound++;
        totalKilledCount++;
        rewardController.AddMoney(data.moneyReward);
        rewardController.AddScore(data.scoreReward);
        characterStats?.RegisterKill();
    }

    private void EndRound()
    {
        isRoundActive = false;

        float elapsed = roundDuration - roundTimer;
        string clearType = pendingSpawns.Count == 0 && aliveEnemies.Count == 0 ? "적 전부 처치" : "시간 만료";

        foreach (var enemy in aliveEnemies)
        {
            if (enemy == null || !enemy.activeInHierarchy) continue;
            var ctrl = enemy.GetComponent<EnemyController>();
            if (ctrl != null) ctrl.SetDead();
            enemy.SetActive(false);
        }
        aliveEnemies.Clear();
        pendingSpawns.Clear();
        Debug.Log($" Round {currentRound}/{totalRounds} 클리어  ");
        Debug.Log($"  조건: {clearType} | 처치: {killedEnemiesThisRound}/{totalEnemiesThisRound} | 경과: {elapsed:F1}초");

        if (currentRound >= totalRounds)
        {
            StartCoroutine(FinalClearSequence());
            return;
        }

        if (roundClearPanel != null) roundClearPanel.SetActive(true);
        StartCoroutine(NextRoundRoutine());
    }

    private List<EnemySpawnEntry> GetEntriesForRound(int round)
    {
        foreach (var obj in waveData.overrides)
        {
            if (obj.round == round)
                return new List<EnemySpawnEntry>(obj.enemies);
        }

        var entries = new List<EnemySpawnEntry>();
        foreach (var config in waveData.enemyTypes)
        {
            if (round < config.startRound) continue;
            int count = config.baseCount
                + Mathf.FloorToInt((round - config.startRound) * config.countPerRound);
            if (count <= 0) continue;
            entries.Add(new EnemySpawnEntry { prefab = config.prefab, count = count });
        }
        return entries;
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
