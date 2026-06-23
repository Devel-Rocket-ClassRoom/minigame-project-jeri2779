using System;

[Serializable]
public class LeaderboardEntry
{
    public string userId;
    public string nickname;
    public int score;
    public int round;
    public long timestamp;

    public LeaderboardEntry()
    {
    }

    public LeaderboardEntry(string userId, string nickname, int score, int round, long timestamp)
    {
        this.userId = userId;
        this.nickname = nickname;
        this.score = score;
        this.round = round;
        this.timestamp = timestamp;
    }

    public string ToJson()
    {
        return UnityEngine.JsonUtility.ToJson(this);
    }

    public static LeaderboardEntry FromJson(string json)
    {
        return UnityEngine.JsonUtility.FromJson<LeaderboardEntry>(json);
    }
}
