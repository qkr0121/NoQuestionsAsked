using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(ParticleSystem))]
public class PooledEffect : MonoBehaviour
{
    private IObjectPool<PooledEffect> pool;

    public void SetPool(IObjectPool<PooledEffect> pool)
    {
        this.pool = pool;
    }

    // 파티클의 Stop Action을 Callback으로 두면, 재생이 끝날 때 유니티가 이 메서드를 호출해 줌
    private void OnParticleSystemStopped()
    {
        pool.Release(this);
    }
}
