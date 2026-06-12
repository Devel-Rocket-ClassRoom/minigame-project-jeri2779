using UnityEngine;

// 풀에서 재사용되는 일회성 VFX. 수명이 끝나면 스스로 풀에 반환한다.
// 수명은 자식 ParticleSystem의 (duration + startLifetime) 최댓값으로 산정 — 없으면 fallback.
[DisallowMultipleComponent]
public class PooledVfx : MonoBehaviour
{
    [SerializeField] private float fallbackLifetime = 5f; // ParticleSystem이 없을 때 적용할 수명

    private VfxPool pool;
    private ParticleSystem[] particleSystems;
    private float lifetime;
    private float timer;
    private bool released;

    private void Awake()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        lifetime = ComputeLifetime();
    }

    public void AssignPool(VfxPool pool) => this.pool = pool;

    // Get() 직후 호출 — 파티클을 처음부터 재생(재사용 시 잔여 파티클 이어붙음 방지).
    public void Play()
    {
        timer = 0f;
        released = false;
        foreach (var ps in particleSystems)
        {
            ps.Clear(true);
            ps.Play(true);
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime)
            Despawn();
    }

    private void Despawn()
    {
        if (released)
            return;

        released = true;
        foreach (var ps in particleSystems)
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        if (pool != null)
            pool.Release(this);
        else
            Destroy(gameObject); // 풀 밖에서 만들어진 인스턴스 폴백
    }

    private float ComputeLifetime()
    {
        if (particleSystems == null || particleSystems.Length == 0)
            return fallbackLifetime;

        float max = 0f;
        foreach (var ps in particleSystems)
        {
            var main = ps.main;
            float dur = main.duration + main.startLifetime.constantMax;
            if (dur > max) max = dur;
        }
        return max > 0f ? max : fallbackLifetime;
    }
}
