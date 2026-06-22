using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable, IHealthInfo
{
    //enemy는 enemydata값 받아옴
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private Renderer hitRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private GameObject bloodVFX; // 피격 시 몸통에 스폰되는 출혈 이펙트(프리팹)

    private float currentHealth;
    private bool isDead;
    private Collider[] colliders;
    private EnemyPool pool;

    // 사망 딜레이 취소원. 풀 반환(OnDisable)/파괴 시 취소돼 재사용된 적에서 이전 딜레이가 뒤늦게 발화하는 것을 막는다.
    private CancellationTokenSource deathCts;

    // 스폰 시 EnemySpawner가 주입. 수동 배치 적은 null로 남아 SetActive(false) 폴백.
    public void AssignPool(EnemyPool pool) => this.pool = pool;

    // 라운드별 체력 배율 (스폰 시 EnemySpawner가 Start 전에 주입)
    private float healthMultiplier = 1f;
    public void SetHealthMultiplier(float multiplier) => healthMultiplier = multiplier;
    private float EffectiveMaxHealth => enemyData.maxHealth * healthMultiplier;

    public float HealthRatio => EffectiveMaxHealth > 0f ? currentHealth / EffectiveMaxHealth : 1f;

    private Renderer[] renderers;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>(true); // 자식 히트박스 포함 — 사망 시 전부 꺼 시체 피격 차단
        ApplyColor(normalColor);
    }

    private void Start()
    {
        // 수동 배치 적도 자체 초기화 (스포너 없이 동작 보존). 풀 재사용 시엔 스포너가 직접 호출.
        ResetForSpawn();
    }

    // 최초 스폰/풀 재사용 공통 상태 초기화. 체력 배율 주입 후 호출돼야 함(EffectiveMaxHealth 의존).
    public void ResetForSpawn()
    {
        currentHealth = EffectiveMaxHealth;
        isDead = false;
        SetCollidersEnabled(true);
    }

    private void SetCollidersEnabled(bool enabled)
    {
        foreach (var c in colliders)
            if (c != null) c.enabled = enabled;
    }

    // 모든 몸통 렌더러에 타입 식별색 적용
    private void ApplyColor(Color c)
    {
        foreach (var r in renderers)
            r.material.color = c;
    }

    private void OnEnable()
    {
        EnemyRegistry.Register(this);
    }

    private void OnDisable()
    {
        EnemyRegistry.Unregister(this);
        // 코루틴은 비활성화 시 자동 정지됐지만 UniTask는 그렇지 않으므로 여기서 명시적으로 취소한다.
        deathCts?.Cancel();
        deathCts?.Dispose();
        deathCts = null;
    }

    // 무기가 명중 지점(hit.point)을 주면 그 자리에 출혈 스폰. 데미지 경로와 완전 별개.
    public void SpawnBloodAt(Vector3 point)
    {
        if (bloodVFX == null) return;
        Destroy(Instantiate(bloodVFX, point, Quaternion.identity), 2f);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            isDead = true;
            SetCollidersEnabled(false);
            Die();
        }
    }
    private void Die()
    {
        EnemyRegistry.ReportKilled(this, enemyData);

        var controller = GetComponent<EnemyController>();
        if (controller != null)
            controller.SetDead();

        deathCts = CancellationTokenSource.CreateLinkedTokenSource(
            this.GetCancellationTokenOnDestroy()
        );
        DeactivateAfterDelay(enemyData.deathDelay, deathCts.Token).Forget();
    }

    private async UniTaskVoid DeactivateAfterDelay(float delay, CancellationToken token)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);
        // 풀 반환: Release()로 ObjectPool 스택에 되돌린다.
        // (수동 배치 적은 pool==null → SetActive(false) 폴백, 동작엔 무영향)
        if (pool != null)
            pool.Release(gameObject);
        else
            gameObject.SetActive(false);
    }
}
