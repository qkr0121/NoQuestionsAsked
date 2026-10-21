using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackInterval = 2f;

    [Header("방어벽 공격")]
    [SerializeField] private int barricadeDamage = 10;
    [SerializeField] private float barricadeAttackInterval = 1f;   // 공격 모션 한 바퀴 시간 = 피해 주기 (EnemyAnimation 이 모션 배속을 이 값에 맞춤)
    [SerializeField] private float contactCheckInterval = 0.25f;   // 막힌 뒤 막힘이 풀렸는지 확인하는 주기
    [SerializeField] private float sphereCastRadius = 0.5f; // 좀비 몸통 반경 정도 - 대각선으로 접근해도 벽을 감지하기 위함
    [SerializeField] private float sphereCastDistance = 1.5f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private SoundData barricadeHitSound;   // 방어벽을 칠 때 나는 소리 (일반/헤비 프리팹마다 다르게), 비워도 동작

    private float attackTimer;
    private float contactCheckTimer;
    private bool nearWall;
    private EnemyMovement enemyMovement;

    public float BarricadeAttackInterval => barricadeAttackInterval;   // 공격 모션 배속(EnemyAnimation)이 피해 주기를 따라가도록 공개

    private void Awake()
    {
        enemyMovement = GetComponent<EnemyMovement>();
    }

    private void OnEnable()
    {
        attackTimer = 0f;
        contactCheckTimer = 0f;
        nearWall = false;
    }

    private void Update()
    {
        attackTimer -= Time.deltaTime;

        if (!nearWall)
        {
            enemyMovement.IsBlocked = false;
            return;
        }

        // 막히기 전에는 매 프레임 확인(벽에 파고들지 않게), 막힌 뒤에는 주기적으로만 확인(풀렸는지)
        contactCheckTimer -= Time.deltaTime;
        if (enemyMovement.IsBlocked && contactCheckTimer > 0f) return;
        contactCheckTimer = contactCheckInterval;

        enemyMovement.IsBlocked = Physics.SphereCast(transform.position, sphereCastRadius, transform.forward, out _, sphereCastDistance, obstacleLayer);
    }

    // 공격 모션의 내려찍는 프레임에서 Animation Event 로 호출됨 (EnemyAnimationEvents 가 전달)
    public void OnBarricadeHit()
    {
        if (!enemyMovement.IsBlocked) return;

        // 이벤트 순간에도 벽이 앞에 있는지 다시 확인 - 모션 도중 벽이 부서졌으면 피해 없음
        if (Physics.SphereCast(transform.position, sphereCastRadius, transform.forward, out RaycastHit hit, sphereCastDistance, obstacleLayer))
        {
            BarricadeSlot slot = hit.collider.GetComponent<BarricadeSlot>();
            if (slot != null)
            {
                slot.TakeDamage(barricadeDamage);
                AudioManager.PlaySfx(barricadeHitSound);   // 공격과 피격이 같은 순간이라 소리 하나로 통일
            }
        }
    }

    // 지속적으로 붙어있으면 주기적으로 데미지를 줘야 하기 때문에 OnTriggerStay 사용
    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (attackTimer > 0f) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            // 공격 성공 시 이펙트, 사운드 추가 예정
            playerHealth.TakeDamage(damage);
        }
        attackTimer = attackInterval;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsObstacleLayer(other)) nearWall = true;

        // 처음 닿는 순간 공격 타이머를 랜덤으로 시작해, 같은 곳에 겹쳐 도착한 좀비들이 일제히 때리지 않게 함
        // 타이머가 이미 만료된 경우에만 적용 - 경계에서 들락날락하거나 콜라이더가 여러 개여도 공격이 계속 밀리지 않도록
        if (other.CompareTag("Player") && attackTimer <= 0f)
        {
            attackTimer = Random.Range(0f, attackInterval);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsObstacleLayer(other)) nearWall = false;
    }

    private bool IsObstacleLayer(Collider other)
    {
        return (obstacleLayer.value & (1 << other.gameObject.layer)) != 0;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = nearWall ? Color.red : Color.yellow;

        Vector3 start = transform.position;
        Vector3 end = start + transform.forward * sphereCastDistance;

        Gizmos.DrawWireSphere(start, sphereCastRadius);
        Gizmos.DrawWireSphere(end, sphereCastRadius);
        Gizmos.DrawLine(start, end);
    }
}
