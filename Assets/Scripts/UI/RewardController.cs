using UnityEngine;

public class RewardController : MonoBehaviour
{
    [SerializeField] private int startingMoney = 7500;
    [SerializeField] private int roundClearReward = 500;

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
        money += amount;
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
        money += roundClearReward;
    }
}
