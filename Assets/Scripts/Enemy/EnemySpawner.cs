using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// 적 스폰 전담. 라운드 진행은 RoundManager가 명령한다.
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private WaveData waveData;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform player;
    [SerializeField] private RewardController rewardController;

    [Header("스폰 설정")]
    [SerializeField] private int maxAliveEnemies = 15;
    [SerializeField] private int spawnBatchSize = 3;
    [SerializeField] private float spawnStartDelay = 3f;
    [SerializeField] private float spawnInterval = 3f;

    public int KilledCount => totalKilledCount;

    // 대기 스폰 없고 생존 적도 없으면 true
    public bool IsAllCleared => pendingSpawns.Count == 0 && aliveEnemies.Count == 0;

    private float spawnTimer;
    private int totalKilledCount;
    private readonly List<GameObject> aliveEnemies = new List<GameObject>();
    private List<EnemySpawnEntry> pendingSpawns = new List<EnemySpawnEntry>();
    private bool isSpawning = false;
    private CharacterStats characterStats;

    private void Start()
    {
        characterStats = player.GetComponent<CharacterStats>();
    }

    private void Update()
    {
        if (!isSpawning) return;

        aliveEnemies.RemoveAll(enemy => enemy == null || !enemy.activeInHierarchy);

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

    // RoundManager가 호출 — 해당 라운드의 스폰 준비 후 시작
    public void StartSpawning(int round)
    {
        spawnTimer = spawnStartDelay;
        aliveEnemies.Clear();
        pendingSpawns = GetEntriesForRound(round);
        isSpawning = true;
    }

    // RoundManager가 호출 — 진행 중인 스폰 정지 + 잔여 적 비활성화
    public void StopSpawning()
    {
        isSpawning = false;
        foreach (var enemy in aliveEnemies)
        {
            if (enemy == null || !enemy.activeInHierarchy) continue;
            var ctrl = enemy.GetComponent<EnemyController>();
            if (ctrl != null) ctrl.SetDead();
            enemy.SetActive(false);
        }
        aliveEnemies.Clear();
        pendingSpawns.Clear();
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
        totalKilledCount++;
        rewardController.AddMoney(data.moneyReward);
        rewardController.AddScore(data.scoreReward);
        characterStats?.RegisterKill();
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
}
