using UnityEngine;
using UnityEngine.Pool;

public class ProjectilePool : MonoBehaviour
{
    [SerializeField] private EnemyProjectile prefab;
    [SerializeField] private int defaultCapacity = 16;
    [SerializeField] private int maxSize = 64;
    [SerializeField] private int prewarmCount = 0;

    private ObjectPool<EnemyProjectile> pool;

    private void Awake()
    {
        EnsurePool();
        Prewarm();
    }

    public EnemyProjectile Spawn(Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
            return null;

        EnsurePool();
        EnemyProjectile projectile = pool.Get();
        projectile.transform.SetPositionAndRotation(position, rotation);
        projectile.AssignPool(this);
        return projectile;
    }

    public void Release(EnemyProjectile projectile)
    {
        if (projectile == null)
            return;

        EnsurePool();
        pool.Release(projectile);
    }

    private EnemyProjectile CreateProjectile()
    {
        EnemyProjectile projectile = Instantiate(prefab);
        if (gameObject.scene.IsValid())
            projectile.transform.SetParent(transform, false);
        projectile.AssignPool(this);
        projectile.gameObject.SetActive(false);
        return projectile;
    }

    private void OnGetProjectile(EnemyProjectile projectile)
    {
        projectile.gameObject.SetActive(true);
    }

    private void OnReleaseProjectile(EnemyProjectile projectile)
    {
        projectile.ResetForPool();
        projectile.gameObject.SetActive(false);
    }

    private void OnDestroyProjectile(EnemyProjectile projectile)
    {
        if (projectile != null)
            Destroy(projectile.gameObject);
    }

    private void Prewarm()
    {
        if (prefab == null || prewarmCount <= 0)
            return;

        EnsurePool();
        int count = Mathf.Min(prewarmCount, maxSize);
        EnemyProjectile[] projectiles = new EnemyProjectile[count];

        for (int i = 0; i < count; i++)
            projectiles[i] = pool.Get();

        for (int i = 0; i < count; i++)
            pool.Release(projectiles[i]);
    }

    private void EnsurePool()
    {
        if (pool != null)
            return;

        pool = new ObjectPool<EnemyProjectile>(
            CreateProjectile,
            OnGetProjectile,
            OnReleaseProjectile,
            OnDestroyProjectile,
            false,
            defaultCapacity,
            maxSize
        );
    }
}
