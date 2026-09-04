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
        // 오브젝트 풀에서 다시 꺼내질 때마다(재활성화) 사망 플래그 리셋
        // (이전에 죽었던 상태가 남아있으면 재사용 시 즉시 죽은 것으로 처리되는 버그 방지)
        isDead = false;
    }

    public void Initialize(float strengthMultiplier, Action onDeath)
    {
        // strengthMultiplier : 시간대별 강도 배율
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
        CurrencyManager.Instance.AddKillReward(); // 좀비 종류 추가 시 매개변수화 및 메서드를 EnemyHealth 로 이동
        isDead = true;
        onDeathCallback?.Invoke();
        enemyMovement.ReturnToPool();
    }
}
