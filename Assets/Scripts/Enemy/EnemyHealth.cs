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
    private float hitFlashTimer;
    private Material hitMaterial;

    private void Start()
    {
        currentHealth = EffectiveMaxHealth;
        hitMaterial = hitRenderer.material;
        hitMaterial.color = normalColor;
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
                hitMaterial.color = normalColor;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        hitMaterial.color = hitColor;
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
