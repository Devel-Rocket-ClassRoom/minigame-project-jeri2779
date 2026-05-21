using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    //enemy는 enemydata값 받아옴
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private Renderer hitRenderer;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private float hitFlashDuration = 0.1f;

    private float currentHealth;
    private float hitFlashTimer;
    private RewardController rewardController;

    private void Start()
    {
        currentHealth = enemyData.maxHealth;
        rewardController = FindFirstObjectByType<RewardController>();
    }

    private void Update()
    {
        if (hitFlashTimer > 0f)
        {
            hitFlashTimer -= Time.deltaTime;
            if (hitFlashTimer <= 0f)
                hitRenderer.material.color = normalColor;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"{damage} damage.health: {currentHealth}");
        hitRenderer.material.color = hitColor;
        hitFlashTimer = hitFlashDuration;

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        // Handle enemy death (e.g., play animation, disable controls, etc.)
        Debug.Log($"{gameObject.name} died.");
        if (rewardController != null)
        {
            rewardController.AddMoney(enemyData.moneyReward);
            rewardController.AddScore(enemyData.scoreReward);
        }
        //적 컨트롤러 비활성화 처리
        EnemyController controller = GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.SetDead();
            gameObject.SetActive(false);
        }
    }
     
}
