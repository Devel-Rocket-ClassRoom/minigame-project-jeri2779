using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Chasing,
        Attacking,
        Dead
    }
    private EnemyState currentState = EnemyState.Idle;
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private Transform character;

    private float attackTimer = 0f;
    private IDamageable playerDamageable;
    private CharacterHealth playerHealth;
    private NavMeshAgent agent;
    


    public void Initialize(Transform player)
    {
        character = player;
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        playerDamageable = character.GetComponent<IDamageable>();
        playerHealth = character.GetComponent<CharacterHealth>();
    }

    // Update is called once per frame
    void Update()
    {
        if(currentState == EnemyState.Dead) return;
        UpdateState();
            
        switch (currentState)
        { 
            case EnemyState.Idle:
                // Handle idle behavior
                break;
            case EnemyState.Chasing:
                agent.isStopped = false;
                agent.SetDestination(character.position);
                break;
            case EnemyState.Attacking:
                agent.isStopped = true;
                HandleAttack();
                break;
            case EnemyState.Dead:
                // Handle dead behavior
                break;
        }
    }

    private void UpdateState()
    {
        if (playerHealth.State == CharacterHealth.CharacterState.Dead)
        {
            currentState = EnemyState.Idle;
            return;
        }
        Vector3 flatDist = character.position - transform.position;
        flatDist.y = 0f;
        float sqrDist = flatDist.sqrMagnitude;
        float sqrRange = enemyData.attackRange * enemyData.attackRange;

        if (sqrDist <= sqrRange)
        {
            currentState = EnemyState.Attacking;
        }
        else
        {
            currentState = EnemyState.Chasing;
        }
        
    }
    private void HandleAttack()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= enemyData.attackInterval)
        {
            Debug.Log($"{gameObject.name} is attacking the player!");
            if (playerDamageable != null)
            {
                playerDamageable.TakeDamage(enemyData.attackDamage);
            }
            attackTimer = 0f; // Reset the attack timer
        }
    }
    public void SetDead()
    {
        currentState = EnemyState.Dead;
        agent.isStopped = true;
    }
    

   
}
