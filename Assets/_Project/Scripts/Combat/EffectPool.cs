using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EffectPool : MonoBehaviour
{
    public static EffectPool Instance { get; private set; }

    // 이펙트 종류(프리팹)마다 풀을 따로 둠
    private readonly Dictionary<PooledEffect, ObjectPool<PooledEffect>> pools = new Dictionary<PooledEffect, ObjectPool<PooledEffect>>();

    private void Awake()
    {
        Instance = this;
    }

    public void Play(PooledEffect prefab, Vector3 position, Quaternion rotation)
    {
        if(!pools.TryGetValue(prefab, out ObjectPool<PooledEffect> pool))
        {
            pool = CreatePool(prefab);
            pools.Add(prefab, pool);
        }

        PooledEffect effect = pool.Get();
        effect.transform.SetPositionAndRotation(position, rotation);
        effect.GetComponent<ParticleSystem>().Play();
    }

    private ObjectPool<PooledEffect> CreatePool(PooledEffect prefab)
    {
        ObjectPool<PooledEffect> pool = null;
        pool = new ObjectPool<PooledEffect>(
            createFunc: () =>
            {
                PooledEffect effect = Instantiate(prefab, transform);
                effect.SetPool(pool);
                return effect;
            },
            actionOnGet: effect => effect.gameObject.SetActive(true),
            actionOnRelease: effect => effect.gameObject.SetActive(false),
            actionOnDestroy: effect => Destroy(effect.gameObject),
            defaultCapacity: 10,
            maxSize: 50);
        return pool;
    }
}
