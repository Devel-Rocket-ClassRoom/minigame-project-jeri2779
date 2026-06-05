using System;
using UnityEngine;

public class CharacterHealth : MonoBehaviour, IDamageable, IHealthInfo
{
    public enum CharacterState
    {
        Alive,
        Dead
    }
    public CharacterState State { get; private set; } = CharacterState.Alive;
    public float CurrentHealth => currentHealth;
    public float HealthRatio => characterStats.MaxHealth > 0f ? currentHealth / characterStats.MaxHealth : 1f;

    // 체력/최대체력 변경 시  (current, max)
    public event Action<float, float> OnHealthChanged;
    // 실제 피해를 입었을 때 1회 발행(회복 제외). 피격 시각 피드백이 구독.
    public event Action OnDamaged;
    // 사망 시 1회 발행
    public event Action OnDied;

    private CharacterStats characterStats;
    private float currentHealth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        characterStats = GetComponent<CharacterStats>();
        currentHealth = characterStats.MaxHealth;
    }

    private void RaiseHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHealth, characterStats.MaxHealth);
    }

    public void ResetHealth()
    {
        State = CharacterState.Alive;
        currentHealth = characterStats.MaxHealth;
        RaiseHealthChanged();
    }

    public void AddHealth(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, characterStats.MaxHealth);
        RaiseHealthChanged();
    }

    public void TakeDamage(float damage)
    {
        if (State == CharacterState.Dead) return;
        float applied = damage * (1f - characterStats.DamageReduction);
        currentHealth -= applied;
        RaiseHealthChanged();
        if (applied > 0f) OnDamaged?.Invoke();

        if (currentHealth <= 0)
        {

            Die();
        }
    }
    private void Die()
    {
        State = CharacterState.Dead;
        OnDied?.Invoke();
        //캐릭터 컨트롤러 비활성화 처리
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
    }
}
