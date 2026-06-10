using System.Collections.Generic;
using UnityEngine;

// 프리팹 종류별 적 인스턴스 풀. 생성/대여만 담당한다.
public class EnemyPool
{
    private readonly Dictionary<GameObject, List<GameObject>> pools = new();

    // 해당 프리팹의 비활성 인스턴스를 재사용하거나 없으면 새로 만든다.
    // 위치/회전 적용 후 활성화해 반환한다 (활성화 시점에 적이 스스로 EnemyRegistry에 재등록).
    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!pools.TryGetValue(prefab, out var list))
        {
            list = new List<GameObject>();
            pools[prefab] = list;
        }

        foreach (var inst in list)
        {
            if (inst != null && !inst.activeSelf)
            {
                
                inst.transform.SetPositionAndRotation(position, rotation);
                inst.SetActive(true);
                return inst;
            }
        }

        var created = Object.Instantiate(prefab, position, rotation);
        list.Add(created);
        return created;
    }
}
