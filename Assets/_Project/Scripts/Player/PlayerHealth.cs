using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public void TakeDamage(int amount)
    {
        if (PlayerManager.Instance.CurrentHealth <= 0) return;

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
