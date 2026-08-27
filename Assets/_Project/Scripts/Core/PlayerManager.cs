using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [SerializeField] private int maxHealth = 100;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }

    private void Awake()
    {
        Instance = this;
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (CurrentHealth <= 0) return;

        CurrentHealth -= amount;
        if(CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            GameManager.Instance.GameOver();
        }
    }

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
    }
}
