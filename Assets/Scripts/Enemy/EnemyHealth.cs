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

    // 라운드별 체력 배율 (스폰 시 EnemySpawner가 Start 전에 주입)
    private float healthMultiplier = 1f;
    public void SetHealthMultiplier(float multiplier) => healthMultiplier = multiplier;
    private float EffectiveMaxHealth => enemyData.maxHealth * healthMultiplier;

    public float HealthRatio => EffectiveMaxHealth > 0f ? currentHealth / EffectiveMaxHealth : 1f;

    private Renderer[] renderers;

    private void Start()
    {
        currentHealth = EffectiveMaxHealth;
        renderers = GetComponentsInChildren<Renderer>();
        ApplyColor(normalColor);
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
