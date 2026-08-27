using UnityEngine;
using UnityEngine.Pool;

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance { get; private set; }

    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int defaultCapacity = 20;
    [SerializeField] private int maxSize = 200;

    private ObjectPool<EnemyMovement> pool;

    private void Awake()
    {
        Instance = this;

        pool = new ObjectPool<EnemyMovement>(
            createFunc: CreateEnemy,
            actionOnGet: OnGetEnemy,
            actionOnRelease: OnReleaseEnemy,
            actionOnDestroy: OnDestroyEnemy,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize);
    }

    public EnemyMovement Get(Vector3 position, Quaternion rotation)
    {
        EnemyMovement enemy = pool.Get();
        enemy.transform.SetPositionAndRotation(position, rotation);
        return enemy;
    }

    public void Release(EnemyMovement enemy)
    {
        pool.Release(enemy);
    }

    private EnemyMovement CreateEnemy()
    {
        GameObject obj = Instantiate(enemyPrefab, transform);
        EnemyMovement enemy = obj.GetComponent<EnemyMovement>();
        enemy.SetPool(this);
        return enemy;
    }

    private void OnGetEnemy(EnemyMovement enemy)
    {
        enemy.gameObject.SetActive(true);
    }

    private void OnReleaseEnemy(EnemyMovement enemy)
    {
        enemy.gameObject.SetActive(false);
    }

    private void OnDestroyEnemy(EnemyMovement enemy)
    {
        Destroy(enemy.gameObject);
    }
}
