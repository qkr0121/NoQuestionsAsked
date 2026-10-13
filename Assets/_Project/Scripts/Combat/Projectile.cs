using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private LayerMask hitMask;             // Enemy 레이어
    [SerializeField] private TrailRenderer trail;           // 잔상, 비워둬도 동작
    [SerializeField] private float trailLingerTime = 0.1f;  // 명중 후 잔상이 사라질 때까지 반납을 미루는 시간
    [SerializeField] private float muzzleCheckRadius = 0.1f;
    [SerializeField] private PooledEffect hitEffect;        // 피격 이펙트, 비워둬도 동작

    private ProjectilePool pool;
    private int damage;
    private float slowMultiplier;
    private float slowDuration;
    private float speed;
    private float remainingRange;
    private bool isFinished;
    private float lingerTimer;

    public void SetPool(ProjectilePool pool)
    {
        this.pool = pool;
    }

    public void Launch(int damage, float speed, float range, float slowMultiplier, float slowDuration)
    {
        this.damage = damage;
        this.slowMultiplier = slowMultiplier;
        this.slowDuration = slowDuration;
        this.speed = speed;
        remainingRange = range;
        isFinished = false;

        // 풀에서 꺼내 위치를 옮긴 직후 이전 위치와 이어지는 잔상이 그려지지 않게 함
        if (trail != null) trail.Clear();

        // 총구가 이미 좀비 콜라이더 안이면 Raycast가 그 좀비를 못 맞히므로(시작점이 내부면 무시됨) 발사 즉시 명중 처리
        Collider[] overlaps = Physics.OverlapSphere(transform.position, muzzleCheckRadius, hitMask, QueryTriggerInteraction.Collide);
        if(overlaps.Length > 0)
        {
            ApplyHit(overlaps[0], transform.position, -transform.forward);
            Finish();
        }
    }

    private void Update()
    {
        if(isFinished)
        {
            // 바로 비활성화하면 잔상이 뚝 끊기므로 잔상이 사라질 때까지 기다렸다가 반납
            lingerTimer -= Time.deltaTime;
            if (lingerTimer <= 0f) pool.Release(this);
            return;
        }

        float stepLength = Mathf.Min(speed * Time.deltaTime, remainingRange);

        // 이번 프레임에 지나갈 구간을 Raycast로 먼저 검사해서, 탄속이 아무리 빨라도 좀비를 건너뛰지 않게 함
        if(Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, stepLength, hitMask, QueryTriggerInteraction.Collide))
        {
            transform.position = hit.point;
            ApplyHit(hit.collider, hit.point, hit.normal);
            Finish();
            return;
        }

        transform.position += transform.forward * stepLength;
        remainingRange -= stepLength;
        if (remainingRange <= 0f) Finish();
    }

    private void ApplyHit(Collider other, Vector3 point, Vector3 noraml)
    {
        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy == null) return;

        // 사망해 풀로 반납되기 전에 적용 - 반납된 오브젝트에 슬로우 상태가 남지 않게 함
        enemy.GetComponent<EnemyMovement>().ApplySlow(slowMultiplier, slowDuration);
        enemy.TakeDamage(damage);

        // 맞은 표면 바깥쪽(총을 쏜 쪽)으로 터지게 함
        if (hitEffect != null) EffectPool.Instance.Play(hitEffect, point, Quaternion.LookRotation(noraml));
    }

    private void Finish()
    {
        isFinished = true;
        lingerTimer = trailLingerTime;
    }
}
