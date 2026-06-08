using UnityEngine;
using UnityEngine.UI;

 
[RequireComponent(typeof(CharacterHealth))]
public class PlayerHitFeedback : MonoBehaviour
{
    [SerializeField] private Image vignette;          // 피격 이미지
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
