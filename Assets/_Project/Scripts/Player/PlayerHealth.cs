using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float invincibleTime = 0.2f;   // 피격 후 무적 시간 - 여러 좀비가 동시에 때려도 피해량의 상한이 됨
    [SerializeField] private EnemyHitFeedback hitFeedback;   // 피격 번쩍임, 비워둬도 동작
    [SerializeField] private SoundData hitSound;     // 피격 때마다 항상 나는 소리(신음), 비워둬도 동작
    [SerializeField] private SoundData biteSound;    // 피격 때 함께 나는 물리는 소리(여러 개 중 하나), 비워둬도 동작
    [SerializeField] private SoundData deathSound;   // 사망 소리, 비워둬도 동작
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
            AudioManager.PlaySfx(deathSound);   // 죽는 타격은 피격 소리 대신 사망 소리만
            GameManager.Instance.GameOver();
        }
        else
        {
            AudioManager.PlaySfx(hitSound);
            AudioManager.PlaySfx(biteSound);
            if (hitFeedback != null) hitFeedback.Play();   // 죽는 타격은 게임오버 화면이 대신함
        }
    }

    public void Heal(int amount)
    {
        int newHealth = PlayerManager.Instance.CurrentHealth + amount;
        PlayerManager.Instance.SetCurrentHealth(newHealth);
    }
}
