using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float stoppingDistance = 1.2f;
    [SerializeField] private bool immuneToSlow;   // 헤비처럼 슬로우가 통하지 않는 좀비

    [Header("분리 조향")]
    [SerializeField] private float separationRadius = 0.5f;     // 몸집 반경 - 두 좀비의 반경 합보다 가까워지면 서로 밀어냄
    [SerializeField] private float separationMass = 1f;         // 클수록 잘 안 밀림 (헤비)
    [SerializeField] private float separationSpeed = 3f;        // 밀려나는 최대 속도 - 추격 속도보다 작아야 결국 플레이어에게 도달함
    [SerializeField] private float separationDeadZone = 0.05f;  // 이보다 약한 힘은 무시 - 떨림 방지
    [SerializeField] private float separationSmoothing = 10f;   // 밀림 방향을 섞는 속도 - 떨림 방지

    private static readonly List<EnemyMovement> active = new List<EnemyMovement>();
    private Vector3 smoothedPush;

    private float slowMultiplier = 1f;
    private float slowUntil;
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
        slowMultiplier = 1f;   // 풀에서 재사용될 때 이전 슬로우가 남지 않게 초기화
        slowUntil = 0f;
        smoothedPush = Vector3.zero;
        active.Add(this);   // 풀에서 꺼내질 때 등록
    }

    private void OnDisable()
    {
        active.Remove(this);   // 풀로 반납될 때 해제
    }

    public void ApplySlow(float multiplier, float duration)
    {
        if (immuneToSlow || duration <= 0f || multiplier >= 1f) return;

        // 이미 느려진 중이면 더 강한 배율을 유지하고 지속만 늘림 - 곱셈 중첩은 연사로 거의 멈춰 버리기 때문
        float now = Time.time;
        if (now < slowUntil)
        {
            slowMultiplier = Mathf.Min(slowMultiplier, multiplier);
            slowUntil = Mathf.Max(slowUntil, now + duration);
        }
        else
        {
            slowMultiplier = multiplier;
            slowUntil = now + duration;
        }
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

        ApplySeparation();   // 정지 거리 안에서도 밀려나야 고리 모양이 만들어지므로 추격 이동보다 먼저 호출

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;
        if (distance < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1 - Mathf.Exp(-rotationSpeed * Time.deltaTime));
        // 지수 감쇠 기반 부드러운 회전 : 프레임레이트에 무관하게 일정한 감쇠속도를 내기 위한 공식

        if(distance > stoppingDistance)
        {
            float speed = Time.time < slowUntil ? moveSpeed * slowMultiplier : moveSpeed;
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    private void ApplySeparation()
    {
        Vector3 push = Vector3.zero;
        Vector3 myPosition = transform.position;

        for (int i = 0; i < active.Count; i++)
        {
            EnemyMovement other = active[i];
            if (other == this) continue;

            Vector3 away = myPosition - other.transform.position;
            away.y = 0f;

            float minDistance = separationRadius + other.separationRadius;
            float sqrDistance = away.sqrMagnitude;
            if (sqrDistance >= minDistance * minDistance) continue;

            float distance = Mathf.Sqrt(sqrDistance);
            // 스폰 직후처럼 완전히 같은 위치면 방향을 정할 수 없으므로 개체마다 다른 고정 방향으로 밀어냄
            Vector3 direction = distance > 0.0001f ? away / distance : FallbackDirection();

            float overlap = 1f - distance / minDistance;
            float share = 2f * other.separationMass / (separationMass + other.separationMass);   // 질량이 큰 쪽이 덜 밀림
            push += direction * overlap * share;
        }

        push = Vector3.ClampMagnitude(push, 1f);
        smoothedPush = Vector3.Lerp(smoothedPush, push, 1f - Mathf.Exp(-separationSmoothing * Time.deltaTime));
        if (smoothedPush.magnitude < separationDeadZone) return;

        transform.position += smoothedPush * separationSpeed * Time.deltaTime;
    }

    private Vector3 FallbackDirection()
    {
        float angle = GetInstanceID() * 0.618f;
        return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
    }
}
