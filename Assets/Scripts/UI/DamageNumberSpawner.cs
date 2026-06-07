using System.Collections.Generic;
using UnityEngine;

 
public class DamageNumberSpawner : MonoBehaviour
{
    [SerializeField] private PlayerDamageCalculator damageCalculator;
    [SerializeField] private RectTransform container;   // 중앙 기준 컨테이너(보통 화면 중앙 anchor)
    [SerializeField] private DamageNumberPopup popupPrefab;

    [Header("설정")]
    [SerializeField] private int poolSize = 12;
    [SerializeField] private Vector2 randomRange = new Vector2(120f, 80f); // 중앙 기준 ± 픽셀
    [SerializeField] private float floatDistance = 50f;
    [SerializeField] private float duration = 0.7f; // 기존 1.5초보다 빠르게
    [SerializeField] private Color headshotColor = new Color(1f, 0.85f, 0.2f, 1f); // 헤드샷 데미지 색 (일반은 팝업 프리팹 기본색)

    private readonly List<DamageNumberPopup> pool = new();
    private int next;

    private void Awake()
    {
        if (popupPrefab == null || container == null) return;
        for (int i = 0; i < poolSize; i++)
        {
            var p = Instantiate(popupPrefab, container, false);
            p.gameObject.SetActive(false);
            pool.Add(p);
        }
    }

    private void OnEnable()
    {
        if (damageCalculator != null) damageCalculator.OnDamageDealt += HandleDamage;
    }

    private void OnDisable()
    {
        if (damageCalculator != null) damageCalculator.OnDamageDealt -= HandleDamage;
    }

    private void HandleDamage(float damage, bool isHeadshot)
    {
        if (pool.Count == 0) return;
        var popup = GetPooled();
        Vector2 pos = new Vector2(
            Random.Range(-randomRange.x, randomRange.x),
            Random.Range(-randomRange.y, randomRange.y));
        popup.Show(((int)damage).ToString(), pos, duration, floatDistance,
            isHeadshot ? headshotColor : (Color?)null);
    }

    private DamageNumberPopup GetPooled()
    {
        foreach (var p in pool)
        {
            if (!p.IsActive) return p;
        }   
        var recycled = pool[next];
        next = (next + 1) % pool.Count;
        return recycled;
    }
}
