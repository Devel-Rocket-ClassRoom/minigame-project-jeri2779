using UnityEngine;
using TMPro;

// 개별 데미지 숫자(풀에서 재사용). Show 호출 시 위치/값 세팅 후 위로 떠오르며 알파 페이드.
public class DamageNumberPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private RectTransform rt;
    private Color baseColor;
    private Vector2 startPos;
    private float timer;
    private float duration;
    private float floatDistance;

    private void Awake()
    {
        rt = (RectTransform)transform;
        if (text == null) text = GetComponent<TextMeshProUGUI>();
        baseColor = text.color;
        gameObject.SetActive(false);
    }

    public bool IsActive => gameObject.activeSelf;

    public void Show(string value, Vector2 anchoredPos, float duration, float floatDistance)
    {
        this.duration = duration;
        this.floatDistance = floatDistance;
        startPos = anchoredPos;
        timer = duration;
        text.text = value;
        rt.anchoredPosition = anchoredPos;
        SetAlpha(1f);
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (timer <= 0f) return;
        timer -= Time.deltaTime;
        float p = 1f - Mathf.Clamp01(timer / duration); // 진행도 0→1
        rt.anchoredPosition = startPos + Vector2.up * (floatDistance * p);
        SetAlpha(1f - p);
        if (timer <= 0f) gameObject.SetActive(false);
    }

    private void SetAlpha(float a)
    {
        text.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
    }
}
