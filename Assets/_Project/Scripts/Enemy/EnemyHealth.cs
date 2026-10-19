using System;
using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 30;
    [SerializeField] private float damageNumberHeight = 1.5f;   // 좀비 위치에서 머리 위까지 높이 - 프리팹마다 조정
    [SerializeField] private int crystalReward = 10;   // 킬 보상 - 프리팹마다 조정 (일반/헤비 등 종류별로 다르게)
    [SerializeField] private PooledEffect deathEffect;      // 죽을 때 터지는 이펙트, 비워둬도 동작
    [SerializeField] private float deathEffectHeight = 1f;  // 좀비 위치에서 몸 중심까지 높이

    private int currentHealth;
    private bool isDead;
    private EnemyMovement enemyMovement;
    private EnemyHitFeedback hitFeedback;
    private Action onDeathCallback;

    private void Awake()
    {
        enemyMovement = GetComponent<EnemyMovement>();
        hitFeedback = GetComponent<EnemyHitFeedback>();
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

        // 표시 값은 무기 데미지 그대로 (남은 체력보다 커도 줄이지 않음) - 풀이 없어도 전투가 멈추지 않게 null 검사
        if (DamageNumberPool.Instance != null)
        {
            DamageNumberPool.Instance.Show(amount, transform.position + Vector3.up * damageNumberHeight);
        }

        currentHealth -= amount;
        if (currentHealth > 0 && hitFeedback != null) hitFeedback.Play();   // 죽는 타격은 사망 이펙트가 대신함

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        CurrencyManager.Instance.AddKillReward(crystalReward);
        isDead = true;

        if (DamageNumberPool.Instance != null)
            DamageNumberPool.Instance.ShowReward(crystalReward, transform.position + Vector3.up * damageNumberHeight);

        if (deathEffect != null && EffectPool.Instance != null)
            EffectPool.Instance.Play(deathEffect, transform.position + Vector3.up * deathEffectHeight, Quaternion.identity);

        onDeathCallback?.Invoke();
        enemyMovement.ReturnToPool();
    }
}
