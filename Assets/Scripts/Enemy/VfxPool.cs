using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

// 일회성 VFX(폭발 등) 풀. ProjectilePool과 동일 패턴:
// prefab 키, 위치 확정 후 활성화, 이중 릴리즈 가드 (Unity ObjectPool<T> 기반).
// VFX 프리팹에 PooledVfx가 없으면 생성 시 자동 부착 → 프리팹 수동 편집 불필요.
public class VfxPool
{
    private readonly Dictionary<GameObject, ObjectPool<PooledVfx>> pools = new();
    private readonly Dictionary<PooledVfx, GameObject> instanceToPrefab = new();
    private readonly Transform parent;
    private readonly int defaultCapacity;
    private readonly int maxSize;

    public VfxPool(Transform parent = null, int defaultCapacity = 8, int maxSize = 32)
    {
        this.parent = parent;
        this.defaultCapacity = defaultCapacity;
        this.maxSize = maxSize;
    }

    // VFX를 대여해 지정 위치/회전에서 재생. 위치 확정 후 활성화.
    public PooledVfx Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
            return null;

        ObjectPool<PooledVfx> pool = GetOrCreatePool(prefab);
        PooledVfx instance = pool.Get();
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.gameObject.SetActive(true);
        instance.Play();
        return instance;
    }

    // 수명이 끝난 VFX를 풀로 반환. 이미 비활성이면 무시(이중 반환 방지).
    public void Release(PooledVfx instance)
    {
        if (instance == null || !instance.gameObject.activeSelf)
            return;

        if (instanceToPrefab.TryGetValue(instance, out var prefab))
            pools[prefab].Release(instance);
        else
            instance.gameObject.SetActive(false);
    }

    private ObjectPool<PooledVfx> GetOrCreatePool(GameObject prefab)
    {
        if (pools.TryGetValue(prefab, out var pool))
            return pool;

        pool = new ObjectPool<PooledVfx>(
            createFunc: () =>
            {
                GameObject go = Object.Instantiate(prefab);
                PooledVfx inst = go.GetComponent<PooledVfx>();
                if (inst == null) inst = go.AddComponent<PooledVfx>(); // 프리팹에 없으면 런타임 부착
                if (parent != null) inst.transform.SetParent(parent, false);
                instanceToPrefab[inst] = prefab;
                inst.AssignPool(this);
                inst.gameObject.SetActive(false);
                return inst;
            },
            actionOnGet: null, // 활성화/재생은 위치 확정 후 Get()에서 직접 처리
            actionOnRelease: inst => inst.gameObject.SetActive(false),
            actionOnDestroy: inst => Object.Destroy(inst.gameObject),
            collectionCheck: false,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
        pools[prefab] = pool;
        return pool;
    }
}
