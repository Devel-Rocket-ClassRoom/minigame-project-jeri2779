using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public enum EnemyState { Idle, Chasing, Attacking, Prepare, Action, Recover, Dead }

    [SerializeField] private EnemyData enemyData;
    [SerializeField] private Transform character;
    [SerializeField] private Animator animator;
    [SerializeField] private ProjectilePool projectilePool;

    [Header("추적 간격")]
    [SerializeField] private float repathInterval = 0.2f; // 추격 시 길찾기 재계산 간격(초). 낮을수록 정밀·무거움, 높을수록 가벼움

    [Header("준비 지연 분산")]
    [SerializeField] private float prepareDelayJitter = 0.6f; // 0~이 값만큼 개체별 prepare 시간 랜덤 추가 → 동시 행동(투척/돌진) 분산

    private static readonly int IsWalkingHash = Animator.StringToHash("isWalking");
    private static readonly int IsRunningHash = Animator.StringToHash("isRunning");
    private static readonly int IsDead1Hash = Animator.StringToHash("isDead1");
    private static readonly int IsPunchingHash = Animator.StringToHash("isPunching_Left");

    private EnemyState currentState = EnemyState.Idle;
    private float attackTimer;
    private float prepareTimer;
    private float recoverTimer;
    private float chargeTimer;
    private float repathTimer;
    private float prepareDurationThisCycle; // prepare 진입 시 지터 포함해 1회 계산
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
        playerDamageable = player.GetComponent<IDamageable>();
        playerHealth = player.GetComponent<CharacterHealth>();
    }

    void Awake()
    {
        // agent는 의존성 없으므로 Awake에서 캐시 → ResetForSpawn이 Start보다 먼저 불려도 안전
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        agent.speed = enemyData.moveSpeed;
        playerDamageable = character.GetComponent<IDamageable>();
        playerHealth = character.GetComponent<CharacterHealth>();
        // 수동 배치 적도 자체 초기화 (스포너 없이 동작 보존). 풀 재사용 시엔 스포너가 직접 호출.
        ResetForSpawn();
    }

    // 최초 스폰/풀 재사용 공통 상태 초기화. 죽으면서 바꿔둔 FSM/타이머/Animator/Agent를 되돌린다.
    public void ResetForSpawn()
    {
        currentState = EnemyState.Idle;
        attackTimer = 0f;
        prepareTimer = 0f;
        recoverTimer = 0f;
        chargeTimer = 0f;
        repathTimer = 0f; // 재사용 시 추격 진입 즉시 1회 길찾기
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(transform.position); // 재배치는 호출 전 transform로. Warp가 에이전트를 navmesh에 정착시킴
            agent.isStopped = false;
        }
    }

    void Update()
    {
        if (currentState == EnemyState.Dead) return;
        UpdateState();

        switch (currentState)
        {
            case EnemyState.Chasing:
                if (agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                    // 매 프레임 대신 repathInterval마다 재계산 (다수 적 동시 추격 시 길찾기 부하 절감)
                    repathTimer -= Time.deltaTime;
                    if (repathTimer <= 0f)
                    {
                        agent.SetDestination(character.position);
                        repathTimer = repathInterval;
                    }
                }
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
        // 개체마다 prepare 길이를 살짝 랜덤화 → 동시 행동 분산 (진입 시 1회만)
        prepareDurationThisCycle = enemyData.prepareDuration + Random.Range(0f, prepareDelayJitter);
        if (agent.isOnNavMesh) agent.isStopped = true;
    }

    private void HandlePrepare()
    {
        prepareTimer += Time.deltaTime;
        if (prepareTimer >= prepareDurationThisCycle)
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
        if (projectilePool == null && enemyData.projectilePrefab == null) return;

        Vector3 toTarget = lockedTargetPos - transform.position;
        Vector3 flatDir = new Vector3(toTarget.x, 0f, toTarget.z);
        float horizDist = flatDir.magnitude;

        float t = Mathf.Clamp01((horizDist - enemyData.attackRange) / (enemyData.prepareDistance - enemyData.attackRange));
        float angle = Mathf.Lerp(enemyData.minThrowAngle, enemyData.maxThrowAngle, t);

        Vector3 dir = (flatDir.normalized + Vector3.up * Mathf.Tan(angle * Mathf.Deg2Rad)).normalized;
        Vector3 spawnPos = transform.position + Vector3.up * 1.5f;

        EnemyProjectile projectile = SpawnProjectile(spawnPos, Quaternion.LookRotation(dir));
        if (projectile == null)
            return;

        projectile.Init(enemyData.projectileDamage * damageMultiplier, enemyData.fuseTime, enemyData.explosionRadius, GetComponentsInChildren<Collider>());

        var rb = projectile.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = dir * enemyData.throwForce;
    }

    private EnemyProjectile SpawnProjectile(Vector3 position, Quaternion rotation)
    {
        if (projectilePool != null)
            return projectilePool.Spawn(position, rotation);

        GameObject go = Instantiate(enemyData.projectilePrefab, position, rotation);
        return go.GetComponent<EnemyProjectile>();
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
