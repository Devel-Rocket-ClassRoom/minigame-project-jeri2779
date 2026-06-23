using System;

// scores/{uid}/history/{pushId} 한 항목. Round+Score 기록.
[Serializable]
public class ScoreData
{
    public int score;
    public int round;
    public long timestamp;

    public ScoreData() { }

    public ScoreData(int score, int round, long timestamp)
    {
        this.score = score;
        this.round = round;
        this.timestamp = timestamp;
    }

    public DateTime GetDateTime()
    {
        return TimeUtil.FromUnixMillis(timestamp);
    }

    public string GetDateString()
    {
        return TimeUtil.ToDateString(timestamp);
    }
}
