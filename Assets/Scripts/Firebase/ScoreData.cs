using System;

 
[Serializable]
public class ScoreData
{
    public int score;
    public int round;
    public long timestamp;
    public float playTimeSec; //일시정지 제외

    public ScoreData() { }

    public ScoreData(int score, int round, long timestamp, float playTimeSec)
    {
        this.score = score;
        this.round = round;
        this.timestamp = timestamp;
        this.playTimeSec = playTimeSec;
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
