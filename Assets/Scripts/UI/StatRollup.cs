using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 결과 스탯 숫자 롤업 연출(공용). 값을 0→target으로 굴리고, 표시 형식만 fmt로 주입받는다.
// 굴리는 코드엔 문자열 리터럴이 없다(숫자만). "MM:SS"·"X / Y" 같은 표기 리터럴은 호출부 fmt 람다에만 한정된다.
public static class StatRollup
{
    public struct Item
    {
        public TextMeshProUGUI text;
        public int target;
        public Func<int, string> fmt;
    }

    // 항목을 순서대로 하나씩 롤업. 한 개가 끝나야 다음이 시작된다.
    public static IEnumerator Cascade(IList<Item> items, float rollDuration, float gap)
    {
        foreach (var item in items)
        {
            if (item.text == null) continue;
            yield return RollUp(item.text, item.target, rollDuration, item.fmt);
            yield return new WaitForSecondsRealtime(gap);
        }
    }

    public static IEnumerator RollUp(TextMeshProUGUI text, int target, float duration, Func<int, string> fmt)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            text.text = fmt(Mathf.FloorToInt(Mathf.Lerp(0f, target, t)));
            yield return null;
        }
        text.text = fmt(target);
    }
}
