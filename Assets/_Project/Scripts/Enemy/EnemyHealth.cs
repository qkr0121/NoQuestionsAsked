using System;
using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 30;

    private int currentHealth;
    private bool isDead;
    private EnemyMovement enemyMovement;
    private Action onDeathCallback;

    private void Awake()
    {
        enemyMovement = GetComponent<EnemyMovement>();
    }

    private void OnEnable()
    {
        isDead = false;
    }

    public void Initialize(float strengthMultiplier, Action onDeath)
    {
        currentHealth = Mathf.RoundToInt(maxHealth * strengthMultiplier);
        onDeathCallback = onDeath;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if(currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        CurrencyManager.Instance.AddKillReward();
        isDead = true;
        onDeathCallback?.Invoke();
        enemyMovement.ReturnToPool();
    }
}
