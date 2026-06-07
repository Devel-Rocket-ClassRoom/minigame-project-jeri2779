using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour, IDamageable, IHealthInfo
{
    //enemy는 enemydata값 받아옴
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private Renderer hitRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private GameObject bloodVFX; // 피격 시 몸통에 스폰되는 출혈 이펙트(프리팹)

    private float currentHealth;
    private bool isDead;
    private Collider bodyCollider;

    // 라운드별 체력 배율 (스폰 시 EnemySpawner가 Start 전에 주입)
    private float healthMultiplier = 1f;
    public void SetHealthMultiplier(float multiplier) => healthMultiplier = multiplier;
    private float EffectiveMaxHealth => enemyData.maxHealth * healthMultiplier;

    public float HealthRatio => EffectiveMaxHealth > 0f ? currentHealth / EffectiveMaxHealth : 1f;

    private Renderer[] renderers;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        bodyCollider = GetComponent<Collider>();
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
        if (bodyCollider != null) bodyCollider.enabled = true;
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
            if (bodyCollider != null) bodyCollider.enabled = false;
            Die();
        }
    }
    private void Die()
    {
        EnemyRegistry.ReportKilled(this, enemyData);

        var controller = GetComponent<EnemyController>();
        if (controller != null)
            controller.SetDead();

        StartCoroutine(DeactivateAfterDelay(enemyData.deathDelay));
    }

    private IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // 풀 반환: Destroy 대신 비활성화. 비활성=free → 다음 스폰에서 재사용.
        // (수동 배치 적은 풀이 없어 그대로 비활성 상태로 남는다 — 게임 동작엔 무영향)
        gameObject.SetActive(false);
    }
     
}
