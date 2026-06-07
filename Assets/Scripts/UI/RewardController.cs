using UnityEngine;

public class RewardController : MonoBehaviour
{
    [SerializeField] private int startingMoney = 7500;
    [SerializeField] private int roundClearReward = 500;
    [SerializeField] private CharacterStats characterStats;

    public int Money => money;
    public int Score => score;
    // 이번 판 누적 획득 재화 (씬 리로드 시 0). 끝화면이 읽음.
    public int TotalEarned { get; private set; }

    private int money;
    private int score;

    private void Awake()
    {
        money = startingMoney;
    }

    private void OnEnable()
    {
        EnemyRegistry.OnEnemyKilled += HandleEnemyKilled;
    }

    private void OnDisable()
    {
        EnemyRegistry.OnEnemyKilled -= HandleEnemyKilled;
    }

    // 적 처치 보상 (EnemyRegistry 구독). 적은 보상을 모른다.
    private void HandleEnemyKilled(EnemyData data)
    {
        AddMoney(data.moneyReward);
        AddScore(data.scoreReward);
    }

    public void AddMoney(int amount)
    {
        float multiplier = characterStats != null ? characterStats.MoneyGainMultiplier : 1f;
        int gained = Mathf.RoundToInt(amount * multiplier);
        money += gained;
        TotalEarned += gained;
    }

    public bool SpendMoney(int amount)
    {
        if (Money < amount) return false;
        money -= amount;
        return true;
    }

    public void AddScore(int amount)
    {
        score += amount;
    }

    public void AddRoundClearReward()
    {
        AddMoney(roundClearReward);
    }
}
