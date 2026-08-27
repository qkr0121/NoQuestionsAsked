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
