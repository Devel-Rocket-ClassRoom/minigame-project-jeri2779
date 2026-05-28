using UnityEngine;

public class RewardController : MonoBehaviour
{
    [SerializeField] private int startingMoney = 7500;
    [SerializeField] private int roundClearReward = 500;
    [SerializeField] private CharacterStats characterStats;

    public int Money => money;
    public int Score => score;

    private int money;
    private int score;

    private void Awake()
    {
        money = startingMoney;
    }

    public void AddMoney(int amount)
    {
        float multiplier = characterStats != null ? characterStats.MoneyGainMultiplier : 1f;
        money += Mathf.RoundToInt(amount * multiplier);
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
