 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//추후 스크립트 역할 분리 리팩토링 필요
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private WaveData waveData;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerSpawnPoint;

    [Header("라운드 설정")]
    [SerializeField] private int totalRounds = 10;
    [SerializeField] private float roundDuration = 60f;
    [SerializeField] private float roundEndDelay = 3f;
    [SerializeField] private float timeBetweenRounds = 5f;

    [Header("스폰 설정")]
    [SerializeField] private int maxAliveEnemies = 15;
    [SerializeField] private int spawnBatchSize = 3;
    [SerializeField] private float spawnStartDelay = 3f;
    [SerializeField] private float spawnInterval = 3f;

    public int CurrentRound => currentRound;
    public int TotalRounds => totalRounds;
    public float RoundTimer => roundTimer;
    public bool IsRoundActive => isRoundActive;
    public int KilledCount => totalKilledCount;
    private int currentRound = 0;
    private float roundTimer;
    private float spawnTimer;
    private int totalEnemiesThisRound;
    private int killedEnemiesThisRound;
    private int totalKilledCount;
    private readonly List<GameObject> aliveEnemies = new List<GameObject>();
    private List<EnemySpawnEntry> pendingSpawns = new List<EnemySpawnEntry>();
    private bool isRoundActive = false;
    private CharacterMoves characterMoves;

    private void Start()
    {
        characterMoves = player.GetComponent<CharacterMoves>();
        characterMoves.SetMovable(false);
        StartCoroutine(NextRoundRoutine());
    }

    private void Update()
    {
        if (!isRoundActive) return;

        int beforeCount = aliveEnemies.Count;
        aliveEnemies.RemoveAll(enemy => enemy == null || !enemy.activeInHierarchy);
        int killed = beforeCount - aliveEnemies.Count;
        killedEnemiesThisRound += killed;
        totalKilledCount += killed;

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

    public void StopGame()
    {
        isRoundActive = false;
        StopAllCoroutines();
    }

    private void StartRound()
    {
        currentRound++;
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
            aliveEnemies.Add(enemy);

            pendingSpawns[i].count--;
            if (pendingSpawns[i].count <= 0)
                pendingSpawns.RemoveAt(i);

            count--;
        }
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
             
            Debug.Log("전체 라운드 클리어");
          
            return;
        }

 
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

    private IEnumerator NextRoundRoutine()
    {
        yield return new WaitForSeconds(roundEndDelay);
        characterMoves.SetMovable(false);
        characterMoves.Teleport(playerSpawnPoint.position);
        player.GetComponent<CharacterHealth>().ResetHealth();
        characterMoves.ResetStamina();
        yield return new WaitForSeconds(timeBetweenRounds);
        StartRound();
    }
}
