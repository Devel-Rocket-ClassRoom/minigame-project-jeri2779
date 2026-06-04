using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour, IDamageable, IHealthInfo
{
    //enemy는 enemydata값 받아옴
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private Renderer hitRenderer;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private float hitFlashDuration = 0.1f;

    private float currentHealth;
    private bool isDead;

    // 라운드별 체력 배율 (스폰 시 EnemySpawner가 Start 전에 주입)
    private float healthMultiplier = 1f;
    public void SetHealthMultiplier(float multiplier) => healthMultiplier = multiplier;
    private float EffectiveMaxHealth => enemyData.maxHealth * healthMultiplier;

    public float HealthRatio => EffectiveMaxHealth > 0f ? currentHealth / EffectiveMaxHealth : 1f;

    // 이 적 타입의 기준색(식별용). EnemyController가 prepare/사망 후 복원 기준으로 읽는다.
    public Color NormalColor => normalColor;

    private float hitFlashTimer;
    private Renderer[] renderers;

    private void Start()
    {
        currentHealth = EffectiveMaxHealth;
        renderers = GetComponentsInChildren<Renderer>();
        ApplyColor(normalColor);
    }

    // 모든 몸통 렌더러에 틴트 적용 (타입색/히트색 공통)
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

    private void Update()
    {
        if (hitFlashTimer > 0f)
        {
            hitFlashTimer -= Time.deltaTime;
            if (hitFlashTimer <= 0f)
                ApplyColor(normalColor);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        ApplyColor(hitColor);
        hitFlashTimer = hitFlashDuration;

        if (currentHealth <= 0)
        {
            isDead = true;
            GetComponent<Collider>().enabled = false; 
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
        Destroy(gameObject);
    }
     
}
