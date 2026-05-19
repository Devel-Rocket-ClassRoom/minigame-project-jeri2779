using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    //enemy는 enemydata값 받아옴
    [SerializeField]private EnemyData enemyData;

    private float currentHealth;
   

    private void Start()
    { 
        currentHealth = enemyData.maxHealth;
    }
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private void Die()
    {
        // Handle enemy death (e.g., play animation, disable controls, etc.)
        Debug.Log($"{gameObject.name} died.");
        //적 컨트롤러 비활성화 처리
        EnemyController controller = GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.SetDead();
            gameObject.SetActive(false);
        }
    }
     
}
