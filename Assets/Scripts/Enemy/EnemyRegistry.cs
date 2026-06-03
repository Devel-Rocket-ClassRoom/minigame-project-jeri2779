using System;
using System.Collections.Generic;

// 생존 적 목록·처치 통계의 단일 보관소(정적). 적이 스스로 등록/해제하고, 죽을 때 처치를 보고한다.
// MonoBehaviour/싱글톤/Find 없는 순수 정적 — 보상·흡혈·통계·투지는 구독/읽기만 한다.
// EnemySpawner가 씬에 없어도(수동 배치) 적이 직접 등록하므로 모든 계산이 정상 동작한다.
public static class EnemyRegistry
{
    private static readonly HashSet<EnemyHealth> alive = new HashSet<EnemyHealth>();

    public static int AliveCount => alive.Count;
    public static int KilledCount { get; private set; }

    // 적 처치 시 1회 발행 (죽은 적의 EnemyData). 보상/흡혈/통계가 구독.
    public static event Action<EnemyData> OnEnemyKilled;

    public static void Register(EnemyHealth enemy)
    {
        if (enemy != null)
            alive.Add(enemy);
    }

    public static void Unregister(EnemyHealth enemy)
    {
        if (enemy != null)
            alive.Remove(enemy);
    }

    // 적 사망 보고: 생존 목록에서 제거 + 처치수 증가 + 구독자에게 알림.
    public static void ReportKilled(EnemyHealth enemy, EnemyData data)
    {
        Unregister(enemy);
        KilledCount++;
        OnEnemyKilled?.Invoke(data);
    }

    // 씬(라운드) 재시작 시 정적 상태 초기화. RoundManager.Awake가 호출한다.
    // 구독 이벤트도 비워, 씬 언로드 때 해제 못 된 잔여 구독을 정리한다(새 구독은 이후 OnEnable에서).
    public static void Reset()
    {
        alive.Clear();
        KilledCount = 0;
        OnEnemyKilled = null;
    }
}
