using System;

// users/{uid} 노드에 통째로 저장되는 프로필. 비밀번호는 절대 포함하지 않는다(Auth가 전담).
[Serializable]
public class UserProfile
{
    public string nickname;
    public string email;
    public long createdAt;

    public UserProfile() { }

    public UserProfile(string nickname, string email)
    {
        this.nickname = nickname;
        this.email = email;
        this.createdAt = TimeUtil.NowUnixMillis();
    }

    public string ToJson()
    {
        return UnityEngine.JsonUtility.ToJson(this);
    }

    public static UserProfile FromJson(string json)
    {
        return UnityEngine.JsonUtility.FromJson<UserProfile>(json);
    }
}
