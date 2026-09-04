using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackInterval = 2f;

    private float attackTimer;

    private void OnEnable()
    {
        attackTimer = 0f;
    }

    private void Update()
    {
        attackTimer -= Time.deltaTime;
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
}
