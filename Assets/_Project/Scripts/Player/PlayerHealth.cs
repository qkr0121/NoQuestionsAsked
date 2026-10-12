using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float invincibleTime = 0.2f;   // 피격 후 무적 시간 - 여러 좀비가 동시에 때려도 피해량의 상한이 됨
    private float invincibleUntil;

    public void TakeDamage(int amount)
    {
        if (PlayerManager.Instance.CurrentHealth <= 0) return;
        if (Time.time < invincibleUntil) return;   // 무적 중에는 피해를 받지 않음

        invincibleUntil = Time.time + invincibleTime;

        int newHealth = PlayerManager.Instance.CurrentHealth - amount;
        PlayerManager.Instance.SetCurrentHealth(newHealth);
        if (PlayerManager.Instance.CurrentHealth <= 0)
        {
            GameManager.Instance.GameOver();
        }
    }

    public void Heal(int amount)
    {
        int newHealth = PlayerManager.Instance.CurrentHealth + amount;
        PlayerManager.Instance.SetCurrentHealth(newHealth);
    }
}
