using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance { get; private set; }

    [System.Serializable]
    public class EnemyPrefabEntry
    {
        public EnemyType type;
        public GameObject prefab;
    }

    [SerializeField] private EnemyPrefabEntry[] enemyPrefabs;
    [SerializeField] private int defaultCapacity = 20;
    [SerializeField] private int maxSize = 200;

    private Dictionary<EnemyType, ObjectPool<EnemyMovement>> pools;

    private void Awake()
    {
        Instance = this;

        pools = new Dictionary<EnemyType, ObjectPool<EnemyMovement>>();
        foreach (EnemyPrefabEntry entry in enemyPrefabs)
        {
            EnemyType type = entry.type;
            GameObject prefab = entry.prefab;

            pools[type] = new ObjectPool<EnemyMovement>(
                createFunc: () => CreateEnemy(prefab, type),
                actionOnGet: OnGetEnemy,
                actionOnRelease: OnReleaseEnemy,
                actionOnDestroy: OnDestroyEnemy,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize);
        }
    }

    public EnemyMovement Get(EnemyType type, Vector3 position, Quaternion rotation)
    {
        EnemyMovement enemy = pools[type].Get();
        enemy.transform.SetPositionAndRotation(position, rotation);
        return enemy;
    }

    public void Release(EnemyMovement enemy, EnemyType type)
    {
        pools[type].Release(enemy);
    }

    private EnemyMovement CreateEnemy(GameObject prefab, EnemyType type)
    {
        GameObject obj = Instantiate(prefab, transform); // EnemyPool 오브젝트의 자식으로 생성(Hierarchy 정리 목적)
        EnemyMovement enemy = obj.GetComponent<EnemyMovement>();
        enemy.SetPool(this, type);
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
