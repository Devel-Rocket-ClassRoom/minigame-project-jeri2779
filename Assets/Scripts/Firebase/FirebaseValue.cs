// RTDB가 숫자를 long/double로 돌려줄 때 int로 안전 변환.
public static class FirebaseValue
{
    public static int ToInt(object value)
    {
        if (value == null)
        {
            return 0;
        }

        switch (value)
        {
            case long l:
                return (int)l;
            case double d:
                return (int)d;
            case int i:
                return i;
            default:
                if (int.TryParse(value.ToString(), out int parsed))
                {
                    return parsed;
                }
                return 0;
        }
    }
}
