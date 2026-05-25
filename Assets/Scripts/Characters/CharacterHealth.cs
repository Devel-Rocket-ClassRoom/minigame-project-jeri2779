using UnityEngine;

public class CharacterHealth : MonoBehaviour, IDamageable
{
    public enum CharacterState
    {
        Alive,
        Dead
    }
    public CharacterState State { get; private set; } = CharacterState.Alive;
    public float CurrentHealth => currentHealth;
    private CharacterStats characterStats;
    private float currentHealth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        characterStats = GetComponent<CharacterStats>();
        currentHealth = characterStats.MaxHealth;
    }

 
    public void ResetHealth()
    {
        State = CharacterState.Alive;
        currentHealth = characterStats.MaxHealth;
    }

    public void AddHealth(float amount)
    {
        
        characterStats = GetComponent<CharacterStats>();
        currentHealth = Mathf.Min(currentHealth + amount, characterStats.MaxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (State == CharacterState.Dead) return;
        currentHealth -= damage;
        Debug.Log($"{gameObject.name}{damage} damage. health: {currentHealth}");

        if (currentHealth <= 0)
        {
             
            Die();
        }
    }
    private void Die()
    {
        State = CharacterState.Dead;
        
        Debug.Log($"{gameObject.name} died.");
        //캐릭터 컨트롤러 비활성화 처리
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
    }
}
