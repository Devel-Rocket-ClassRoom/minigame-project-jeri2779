using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    //enemy는 enemydata값 받아옴
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private Renderer hitRenderer;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private float hitFlashDuration = 0.1f;

    public System.Action onKilled;

    private float currentHealth;
    private bool isDead;
    private float hitFlashTimer;
    private Material hitMaterial;
    private RewardController rewardController;

    private void Start()
    {
        currentHealth = enemyData.maxHealth;
        rewardController = FindFirstObjectByType<RewardController>();
        hitMaterial = hitRenderer.material;
        hitMaterial.color = normalColor;
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
        Debug.Log($"{damage} damage.health: {currentHealth}");
        hitMaterial.color = hitColor;
        hitFlashTimer = hitFlashDuration;

        if (currentHealth <= 0)
        {
            isDead = true;
            Die();
        }
    }
    private void Die()
    {
        Debug.Log($"{gameObject.name} died.");
        onKilled?.Invoke();
        if (rewardController != null)
        {
            rewardController.AddMoney(enemyData.moneyReward);
            rewardController.AddScore(enemyData.scoreReward);
        }

        var controller = GetComponent<EnemyController>();
        if (controller != null)
            controller.SetDead();

        StartCoroutine(DeactivateAfterDelay(enemyData.deathDelay));
    }

    private IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
     
}
