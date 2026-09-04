using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private int damage = 10;

    private ProjectilePool pool;
    private float timer;
    private bool isReleased;

    public void SetPool(ProjectilePool pool)
    {
        this.pool = pool;
    }

    private void OnEnable()
    {
        timer = lifeTime;
        isReleased = false;
    }

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;

        timer -= Time.deltaTime;
        if(timer <= 0f)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Update() 의 시간 만료 반납과 OnTriggerEnter() 의 충돌 반납이 동시에 발생할 수 있기 때문에 중복 방지
        if (isReleased) return;

        if(other.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if(enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }

            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        if (isReleased) return;
        isReleased = true;
        pool.Release(this);
    }
}
