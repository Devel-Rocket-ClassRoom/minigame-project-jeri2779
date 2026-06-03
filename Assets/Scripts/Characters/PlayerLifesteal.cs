using UnityEngine;

// 흡혈 행동 전담. 처치 누적이 임계에 도달하면 회복한다.
// 처치 이벤트는 EnemyRegistry를 "구독"해 받는다(직접 호출 안 받음).
// 수치(회복량/임계)는 CharacterStats가 보관하고, 여기서는 읽어서 행동만 한다.
[RequireComponent(typeof(CharacterStats))]
[RequireComponent(typeof(CharacterHealth))]
public class PlayerLifesteal : MonoBehaviour
{
    private CharacterStats stats;
    private CharacterHealth health;
    private int killCounter;

    private void Awake()
    {
        stats = GetComponent<CharacterStats>();
        health = GetComponent<CharacterHealth>();
    }

    private void OnEnable()
    {
        EnemyRegistry.OnEnemyKilled += HandleEnemyKilled;
    }

    private void OnDisable()
    {
        EnemyRegistry.OnEnemyKilled -= HandleEnemyKilled;
    }

    // 적 처치 시 (EnemyRegistry 구독) — 누적이 임계 도달하면 회복
    private void HandleEnemyKilled(EnemyData data)
    {
        if (stats.LifestealHealAmount <= 0f)
            return;

        killCounter++;
        if (killCounter >= stats.LifestealKillThreshold)
        {
            health.AddHealth(stats.LifestealHealAmount);
            killCounter = 0;
        }
    }
}
