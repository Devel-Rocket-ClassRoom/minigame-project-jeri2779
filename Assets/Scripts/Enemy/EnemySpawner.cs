using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// 적 스폰 전담. 라운드 진행은 RoundManager가 명령한다.
// 처치 보상/흡혈/통계/생존수는 일절 다루지 않는다 — 적이 EnemyRegistry에 스스로 등록/보고한다.
// 이 컴포넌트가 씬에 없어도(수동 배치) 게임은 정상 동작한다.
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private WaveData waveData;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform player;

    [Header("스폰 설정")]
    [SerializeField] private int maxAliveEnemies = 15;
    [SerializeField] private int spawnBatchSize = 3;
    [SerializeField] private float spawnStartDelay = 3f;
    [SerializeField] private float spawnInterval = 3f;

    [Header("라운드별 스탯 증가 (라운드당 비율, 0.15 = 라운드마다 +15%)")]
    [SerializeField] private float healthGrowthPerRound = 0.08f;
    [SerializeField] private float damageGrowthPerRound = 0.05f;

    // 대기 스폰이 모두 소진됐는가 (라운드 종료 판정에 RoundManager가 읽음)
    public bool IsSpawningComplete => pendingSpawns.Count == 0;

    private float spawnTimer;
    private readonly List<GameObject> aliveEnemies = new List<GameObject>();
    private List<EnemySpawnEntry> pendingSpawns = new List<EnemySpawnEntry>();
    private bool isSpawning = false;
    private int currentRound = 1;
    private readonly EnemyPool pool = new EnemyPool();

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
        currentRound = round;
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

        // 라운드별 스탯 배율 (라운드 1 = 1.0, 증가 없음)
        float healthMult = 1f + (currentRound - 1) * healthGrowthPerRound;
        float damageMult = 1f + (currentRound - 1) * damageGrowthPerRound;

        for (int i = pendingSpawns.Count - 1; i >= 0 && count > 0; i--)
        {
            Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject enemy = pool.Get(pendingSpawns[i].prefab, sp.position, sp.rotation);

            // 멀티플라이어 주입 → ResetForSpawn 순서 필수 (체력 리셋이 EffectiveMaxHealth 의존).
            // 풀 재사용 시 Start가 안 돌므로 스포너가 직접 ResetForSpawn 호출.
            var ctrl = enemy.GetComponent<EnemyController>();
            ctrl.Initialize(player);
            ctrl.SetDamageMultiplier(damageMult);
            ctrl.ResetForSpawn();
            var health = enemy.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.SetHealthMultiplier(healthMult);
                health.ResetForSpawn();
            }
            aliveEnemies.Add(enemy);

            pendingSpawns[i].count--;
            if (pendingSpawns[i].count <= 0)
                pendingSpawns.RemoveAt(i);

            count--;
        }
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
