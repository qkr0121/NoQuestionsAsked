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

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (attackTimer > 0f) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
        attackTimer = attackInterval;
    }
}
