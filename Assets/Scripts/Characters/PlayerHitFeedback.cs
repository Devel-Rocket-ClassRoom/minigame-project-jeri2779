using UnityEngine;
using UnityEngine.UI;

// 플레이어 피격 시각 피드백 전담. CharacterHealth.OnDamaged 구독 → 화면 비네트 Alpha 페이드.
// HUD(상태 표시)와 분리된 역할. 비네트 Image는 playingHUD 밑에 두어 사망 시 함께 숨겨진다.
[RequireComponent(typeof(CharacterHealth))]
public class PlayerHitFeedback : MonoBehaviour
{
    [SerializeField] private Image vignette;          // 풀스크린 피격 이미지
    [SerializeField] private float flashAlpha = 0.6f; // 피격 순간 최대 불투명도
    [SerializeField] private float fadeDuration = 0.5f;

    private CharacterHealth health;
    private float timer;

    private void Awake()
    {
        health = GetComponent<CharacterHealth>();
        SetAlpha(0f);
    }

    private void OnEnable()
    {
        if (health != null) health.OnDamaged += HandleDamaged;
    }

    private void OnDisable()
    {
        if (health != null) health.OnDamaged -= HandleDamaged;
    }

    private void HandleDamaged()
    {
        timer = fadeDuration;
        SetAlpha(flashAlpha);
    }

    private void Update()
    {
        if (timer <= 0f) return;
        timer -= Time.deltaTime;
        float t = Mathf.Clamp01(timer / fadeDuration);
        SetAlpha(flashAlpha * t);
    }

    private void SetAlpha(float a)
    {
        if (vignette == null) return;
        Color c = vignette.color;
        vignette.color = new Color(c.r, c.g, c.b, a);
    }
}
