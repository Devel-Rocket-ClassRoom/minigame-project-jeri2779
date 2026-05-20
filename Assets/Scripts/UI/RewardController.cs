using UnityEngine;

public class RewardController : MonoBehaviour
{
    public int Money => money;
    public int Score => score;

    private int money;
    private int score;

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
}
