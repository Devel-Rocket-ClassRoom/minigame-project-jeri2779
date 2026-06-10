using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

// 프리팹 종류별 적 인스턴스 풀. Unity ObjectPool<T> 기반으로 통일.
// 반환은 Release()로만 이뤄진다 (SetActive(false)만으로는 풀에 돌아오지 않음).
public class EnemyPool
{
    private readonly Dictionary<GameObject, ObjectPool<GameObject>> pools = new();
    private readonly Dictionary<GameObject, GameObject> instanceToPrefab = new();
    private readonly Transform parent;
    private readonly int defaultCapacity;
    private readonly int maxSize;

    public EnemyPool(Transform parent = null, int defaultCapacity = 16, int maxSize = 64)
    {
        this.parent = parent;
        this.defaultCapacity = defaultCapacity;
        this.maxSize = maxSize;
    }

    // 해당 프리팹의 풀에서 인스턴스를 대여한다. 위치 확정 후 활성화한다.
    // (위치를 먼저 잡아야 NavMeshAgent가 올바른 지점에서 navmesh에 매핑됨)
    // (활성화 시점에 적이 스스로 EnemyRegistry에 재등록)
    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        ObjectPool<GameObject> pool = GetOrCreatePool(prefab);
        GameObject instance = pool.Get();
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);
        return instance;
    }

    // 인스턴스를 풀로 반환한다. 이미 비활성(=반환됨)이면 무시해 중복 반환을 막는다.
    public void Release(GameObject instance)
    {
        if (instance == null || !instance.activeSelf) return;

        if (instanceToPrefab.TryGetValue(instance, out var prefab))
            pools[prefab].Release(instance);
        else
            instance.SetActive(false); // 풀 밖에서 만들어진 인스턴스 폴백
    }

    private ObjectPool<GameObject> GetOrCreatePool(GameObject prefab)
    {
        if (pools.TryGetValue(prefab, out var pool))
            return pool;

        pool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject inst = Object.Instantiate(prefab);
                if (parent != null) inst.transform.SetParent(parent, false);
                instanceToPrefab[inst] = prefab;
                inst.SetActive(false);
                return inst;
            },
            actionOnGet: null, // 활성화는 위치 확정 후 Get()에서 직접 처리
            actionOnRelease: inst => inst.SetActive(false),
            actionOnDestroy: inst => Object.Destroy(inst),
            collectionCheck: false,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
        pools[prefab] = pool;
        return pool;
    }
}
