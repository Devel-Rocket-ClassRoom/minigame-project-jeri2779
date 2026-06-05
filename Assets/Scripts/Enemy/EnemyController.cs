using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public enum EnemyState { Idle, Chasing, Attacking, Prepare, Action, Recover, Dead }

    [SerializeField] private EnemyData enemyData;
    [SerializeField] private Transform character;
    [SerializeField] private Animator animator;

    private static readonly int IsWalkingHash = Animator.StringToHash("isWalking");
    private static readonly int IsRunningHash = Animator.StringToHash("isRunning");
    private static readonly int IsDead1Hash = Animator.StringToHash("isDead1");
    private static readonly int IsPunchingHash = Animator.StringToHash("isPunching_Left");

    private EnemyState currentState = EnemyState.Idle;
    private float attackTimer;
    private float prepareTimer;
    private float recoverTimer;
    private float chargeTimer;
    private Vector3 lockedTargetPos;

    private IDamageable playerDamageable;
    private CharacterHealth playerHealth;
    private NavMeshAgent agent;

    // 라운드별 공격력 배율 (스폰 시 EnemySpawner가 주입)
    private float damageMultiplier = 1f;
    public void SetDamageMultiplier(float multiplier) => damageMultiplier = multiplier;

    public void Initialize(Transform player)
    {
        character = player;
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = enemyData.moveSpeed;
        playerDamageable = character.GetComponent<IDamageable>();
        playerHealth = character.GetComponent<CharacterHealth>();
    }

    void Update()
    {
        if (currentState == EnemyState.Dead) return;
        UpdateState();

        switch (currentState)
        {
            case EnemyState.Chasing:
                if (agent.isOnNavMesh) { agent.isStopped = false; agent.SetDestination(character.position); }
                animator.SetBool(IsWalkingHash, true);
                animator.SetBool(IsRunningHash, false);
                animator.SetBool(IsPunchingHash, false);
                break;
            case EnemyState.Attacking:
                if (agent.isOnNavMesh) agent.isStopped = true;
                animator.SetBool(IsWalkingHash, false);
                animator.SetBool(IsRunningHash, false);
                animator.SetBool(IsPunchingHash, true);
                HandleAttack();
                break;
            case EnemyState.Prepare:
                animator.SetBool(IsWalkingHash, false);
                animator.SetBool(IsRunningHash, false);
                animator.SetBool(IsPunchingHash, false);
                HandlePrepare();
                break;
            case EnemyState.Action:
                if (enemyData.behaviorType == EnemyBehaviorType.Charger)
                {
                    animator.SetBool(IsRunningHash, false);
                    animator.SetBool(IsWalkingHash, false);
                    animator.SetBool(IsPunchingHash, true);
                    HandleChargeAction();
                }
                break;
            case EnemyState.Recover:
                animator.SetBool(IsWalkingHash, false);
                animator.SetBool(IsRunningHash, false);
                animator.SetBool(IsPunchingHash, false);
                HandleRecover();
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

        if (currentState == EnemyState.Prepare ||
            currentState == EnemyState.Action ||
            currentState == EnemyState.Recover)
            return;

        float sqrDist = GetSqrFlatDist();
        float yDiff = Mathf.Abs(character.position.y - transform.position.y);

        switch (enemyData.behaviorType)
        {
            case EnemyBehaviorType.Default:
                bool inRange = sqrDist <= enemyData.attackRange * enemyData.attackRange
                    && yDiff <= enemyData.attackHeightRange;
                currentState = inRange ? EnemyState.Attacking : EnemyState.Chasing;
                break;
            case EnemyBehaviorType.Charger:
            case EnemyBehaviorType.Thrower:
                if (sqrDist <= enemyData.prepareDistance * enemyData.prepareDistance
                    && yDiff <= enemyData.attackHeightRange)
                {
                    if (currentState == EnemyState.Chasing)
                        EnterPrepare();
                }
                else
                    currentState = EnemyState.Chasing;
                break;
        }
    }

    private void EnterPrepare()
    {
        currentState = EnemyState.Prepare;
        lockedTargetPos = character.position;
        prepareTimer = 0f;
        if (agent.isOnNavMesh) agent.isStopped = true;
    }

    private void HandlePrepare()
    {
        prepareTimer += Time.deltaTime;
        if (prepareTimer >= enemyData.prepareDuration)
            EnterAction();
    }

    private void EnterAction()
    {
        currentState = EnemyState.Action;
        chargeTimer = 0f;

        if (enemyData.behaviorType == EnemyBehaviorType.Thrower)
        {
            ThrowProjectile();
            EnterRecover();
        }
        else if (enemyData.behaviorType == EnemyBehaviorType.Charger)
        {
            agent.enabled = false;
        }
    }

    private void HandleChargeAction()
    {
        chargeTimer += Time.deltaTime;
        Vector3 dir = lockedTargetPos - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.2f || chargeTimer >= enemyData.chargeMaxDuration)
        {
            EnterRecover();
            return;
        }

        // 초반 0.2초간 가속 커브 적용
        float accel = Mathf.Clamp01(chargeTimer / 0.2f);
        accel = accel * accel;
        transform.Translate(dir.normalized * enemyData.chargeSpeed * accel * Time.deltaTime, Space.World);

        float contactYDiff = Mathf.Abs(character.position.y - transform.position.y);
        if (GetSqrFlatDist() <= enemyData.attackRange * enemyData.attackRange
            && contactYDiff <= enemyData.attackHeightRange)
        {
            playerDamageable.TakeDamage(enemyData.chargeContactDamage * damageMultiplier);
            EnterRecover();
        }
    }

    private void ThrowProjectile()
    {
        if (enemyData.projectilePrefab == null) return;

        Vector3 toTarget = lockedTargetPos - transform.position;
        Vector3 flatDir = new Vector3(toTarget.x, 0f, toTarget.z);
        float horizDist = flatDir.magnitude;

        float t = Mathf.Clamp01((horizDist - enemyData.attackRange) / (enemyData.prepareDistance - enemyData.attackRange));
        float angle = Mathf.Lerp(enemyData.minThrowAngle, enemyData.maxThrowAngle, t);

        Vector3 dir = (flatDir.normalized + Vector3.up * Mathf.Tan(angle * Mathf.Deg2Rad)).normalized;
        Vector3 spawnPos = transform.position + Vector3.up * 1.5f;

        var go = Instantiate(enemyData.projectilePrefab, spawnPos, Quaternion.LookRotation(dir));
        go.GetComponent<EnemyProjectile>().Init(enemyData.projectileDamage * damageMultiplier, enemyData.fuseTime, enemyData.explosionRadius, GetComponentsInChildren<Collider>());

        var rb = go.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = dir * enemyData.throwForce;
    }

    private void EnterRecover()
    {
        currentState = EnemyState.Recover;
        recoverTimer = 0f;
        if (!agent.enabled) agent.enabled = true;
        if (agent.isOnNavMesh) agent.isStopped = true;
    }

    private void HandleRecover()
    {
        recoverTimer += Time.deltaTime;
        if (recoverTimer >= enemyData.recoverDuration)
            currentState = EnemyState.Chasing;
    }

    private void HandleAttack()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= enemyData.attackInterval)
        {
            if (playerDamageable != null)
                playerDamageable.TakeDamage(enemyData.attackDamage * damageMultiplier);
            attackTimer = 0f;
        }
    }

    public void SetDead()
    {
        currentState = EnemyState.Dead;
        if (!agent.enabled) agent.enabled = true;
        if (agent.isOnNavMesh) agent.isStopped = true;
        animator.SetBool(IsDead1Hash, true);
        animator.SetBool(IsWalkingHash, false);
        animator.SetBool(IsRunningHash, false);
        animator.SetBool(IsPunchingHash, false);
    }

    private float GetSqrFlatDist()
    {
        Vector3 d = character.position - transform.position;
        d.y = 0f;
        return d.sqrMagnitude;
    }
}
