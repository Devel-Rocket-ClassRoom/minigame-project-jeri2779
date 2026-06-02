using UnityEngine;

 
public class PassiveHealthRegen : MonoBehaviour
{
    [SerializeField] private StatUpgradeData data;
    [SerializeField] private float triggerDelay = 3f;   // 조건 유지 필요 시간(초)
    [SerializeField] private float healInterval = 1f;   // 회복 주기

    private UpgradeManager upgradeManager;
    private CharacterHealth health;
    private CharacterMoves moves;
    private PlayerShooter shooter;

    private float idleTimer;
    private float healTimer;

    private void Awake()
    {
        health = GetComponent<CharacterHealth>();
        moves = GetComponent<CharacterMoves>();
        shooter = GetComponent<PlayerShooter>();
        upgradeManager = GetComponent<UpgradeManager>();
    }

    private void Update()
    {
        if (data == null || upgradeManager == null) return;
        if (health.State == CharacterHealth.CharacterState.Dead) return;

        int level = upgradeManager.GetLevel(data.statType);
        if (level == 0)
        {
            idleTimer = 0f;
            healTimer = 0f;
            return;
        }

        bool isIdle = !shooter.IsFirePressed && !moves.IsSprinting;

        if (isIdle)
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= triggerDelay)
            {
                healTimer += Time.deltaTime;
                if (healTimer >= healInterval)
                {
                    health.AddHealth(data.bonusPerLevel * level);
                    healTimer = 0f;
                }
            }
        }
        else
        {
            idleTimer = 0f;
            healTimer = 0f;
        }
    }
}
