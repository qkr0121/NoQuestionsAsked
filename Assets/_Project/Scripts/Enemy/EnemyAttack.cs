using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackInterval = 2f;

    [Header("방어벽 공격")]
    [SerializeField] private int barricadeDamage = 10;
    [SerializeField] private float barricadeAttackInterval = 1f;
    [SerializeField] private float sphereCastRadius = 0.5f; // 좀비 몸통 반경 정도 - 대각선으로 접근해도 벽을 감지하기 위함
    [SerializeField] private float sphereCastDistance = 1.5f;
    [SerializeField] private LayerMask obstacleLayer;

    private float attackTimer;
    private float barricadeAttackTimer;
    private bool nearWall;
    private EnemyMovement enemyMovement;

    private void Awake()
    {
        enemyMovement = GetComponent<EnemyMovement>();
    }

    private void OnEnable()
    {
        attackTimer = 0f;
        barricadeAttackTimer = 0f;
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

        barricadeAttackTimer -= Time.deltaTime;
        if (barricadeAttackTimer > 0f) return;

        CheckWallAndAttack();
    }

    private void CheckWallAndAttack()
    {
        if (Physics.SphereCast(transform.position, sphereCastRadius, transform.forward, out RaycastHit hit, sphereCastDistance, obstacleLayer))
        {
            enemyMovement.IsBlocked = true;

            BarricadeSlot slot = hit.collider.GetComponent<BarricadeSlot>();
            if (slot != null)
            {
                slot.TakeDamage(barricadeDamage);
            }

            barricadeAttackTimer = barricadeAttackInterval;
        }
        else
        {
            enemyMovement.IsBlocked = false;
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
