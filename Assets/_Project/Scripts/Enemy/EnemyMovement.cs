using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float stoppingDistance = 1.2f;

    private Transform target;
    private EnemyPool pool;
    private EnemyType enemyType;
    private bool isReleased;

    public bool IsBlocked { get; set; } // 방어벽에 막혀 공격 중일 때 EnemyAttack이 세팅

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    public void SetPool(EnemyPool pool, EnemyType type)
    {
        this.pool = pool;
        this.enemyType = type;
    }

    private void OnEnable()
    {
        isReleased = false;
    }

    public void ReturnToPool()
    {
        if (isReleased) return; // 중복 반납 방지 - 여러 경로로 반납이 중복 호출될 수 있는 것을 방지
        isReleased = true;
        pool.Release(this, enemyType);
    }

    private void Update()
    {
        if (target == null) return;
        if (IsBlocked) return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;
        if (distance < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1 - Mathf.Exp(-rotationSpeed * Time.deltaTime));
        // 지수 감쇠 기반 부드러운 회전 : 프레임레이트에 무관하게 일정한 감쇠속도를 내기 위한 공식

        if(distance > stoppingDistance)
        {
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
    }
}
