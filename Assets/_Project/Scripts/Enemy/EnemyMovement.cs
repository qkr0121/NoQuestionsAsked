using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 8f;

    private Transform target;
    private EnemyPool pool;
    private bool isReleased;

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    public void SetPool(EnemyPool pool)
    {
        this.pool = pool;
    }

    private void OnEnable()
    {
        isReleased = false;
    }

    public void ReturnToPool()
    {
        if (isReleased) return;
        isReleased = true;
        pool.Release(this);
    }

    private void Update()
    {
        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1 - Mathf.Exp(-rotationSpeed * Time.deltaTime));

        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }
}
