using UnityEngine;
using UnityEngine.Pool;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance { get; private set; }

    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int defaultCapacity = 20;
    [SerializeField] private int maxSize = 100;

    private ObjectPool<Projectile> pool;

    private void Awake()
    {
        Instance = this;

        pool = new ObjectPool<Projectile>(
            createFunc: CreateProjectile,
            actionOnGet: OnGetProjectile,
            actionOnRelease: OnReleaseProjectile,
            actionOnDestroy: OnDestroyProjectile,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
            );
    }

    public Projectile Get(Vector3 position, Quaternion rotation)
    {
        Projectile projectile = pool.Get();
        projectile.transform.SetPositionAndRotation(position, rotation);
        return projectile;
    }

    public void Release(Projectile projectile)
    {
        pool.Release(projectile);
    }

    private Projectile CreateProjectile()
    {
        GameObject obj = Instantiate(projectilePrefab, transform);
        Projectile projectile = obj.GetComponent<Projectile>();
        projectile.SetPool(this);
        return projectile;
    }

    private void OnGetProjectile(Projectile projectile)
    {
        projectile.gameObject.SetActive(true);
    }

    private void OnReleaseProjectile(Projectile projectile)
    {
        projectile.gameObject.SetActive(false);
    }

    private void OnDestroyProjectile(Projectile projectile)
    {
        Destroy(projectile.gameObject);
    }
}
