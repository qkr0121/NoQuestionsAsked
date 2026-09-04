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
        if (isReleased) return; // 중복 반납 방지 - 여러 경로로 반납이 중복 호출될 수 있는 것을 방지
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
        // 지수 감쇠 기반 부드러운 회전 : 프레임레이트에 무관하게 일정한 감쇠속도를 내기 위한 공식

        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }
}
