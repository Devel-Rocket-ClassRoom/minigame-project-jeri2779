using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

// 적 투사체 풀. EnemyPool과 동일한 순수 C# 클래스 패턴으로 통일.
// (prefab 키, 위치 확정 후 활성화, 이중 릴리즈 가드 — Unity ObjectPool<T> 기반)
// 투사체 프리팹의 출처는 EnemyData.projectilePrefab — 적이 Get 시 자기 프리팹을 넘긴다.
public class ProjectilePool
{
    private readonly Dictionary<GameObject, ObjectPool<EnemyProjectile>> pools = new();
    private readonly Dictionary<EnemyProjectile, GameObject> instanceToPrefab = new();
    private readonly Transform parent;
    private readonly int defaultCapacity;
    private readonly int maxSize;

    public ProjectilePool(Transform parent = null, int defaultCapacity = 16, int maxSize = 64)
    {
        this.parent = parent;
        this.defaultCapacity = defaultCapacity;
        this.maxSize = maxSize;
    }

    // 해당 프리팹 풀에서 투사체를 대여. 위치 확정 후 활성화 (직전 폭발 위치에서 튀어나오는 현상 방지).
    public EnemyProjectile Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
            return null;

        ObjectPool<EnemyProjectile> pool = GetOrCreatePool(prefab);
        EnemyProjectile instance = pool.Get();
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.gameObject.SetActive(true);
        return instance;
    }

    // 투사체를 풀로 반환. 이미 비활성(=반환됨)이면 무시해 이중 반환을 막는다.
    public void Release(EnemyProjectile instance)
    {
        if (instance == null || !instance.gameObject.activeSelf)
            return;

        if (instanceToPrefab.TryGetValue(instance, out var prefab))
            pools[prefab].Release(instance);
        else
            instance.gameObject.SetActive(false); // 풀 밖에서 만들어진 인스턴스 폴백
    }

    private ObjectPool<EnemyProjectile> GetOrCreatePool(GameObject prefab)
    {
        if (pools.TryGetValue(prefab, out var pool))
            return pool;

        pool = new ObjectPool<EnemyProjectile>(
            createFunc: () =>
            {
                EnemyProjectile inst = Object.Instantiate(prefab).GetComponent<EnemyProjectile>();
                if (parent != null) inst.transform.SetParent(parent, false);
                instanceToPrefab[inst] = prefab;
                inst.AssignPool(this);
                inst.gameObject.SetActive(false);
                return inst;
            },
            actionOnGet: null, // 활성화는 위치 확정 후 Get()에서 직접 처리
            actionOnRelease: inst =>
            {
                inst.ResetForPool();
                inst.gameObject.SetActive(false);
            },
            actionOnDestroy: inst => Object.Destroy(inst.gameObject),
            collectionCheck: false,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
        pools[prefab] = pool;
        return pool;
    }
}
