using UnityEngine;

public class PlayerPotion : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;

    private const int PotionIndex = 0;   // 지금은 회복약 1종만 사용 (PotionRowUI와 동일)
    private float nextUseTime;

    // 핫바가 쿨타임 표시에 쓰는 값
    public float CooldownRemaining => Mathf.Max(0f, nextUseTime - Time.time);

    // 입력은 PlayerHotbar가 받아서 이 메서드를 호출함
    public bool TryUse()
    {
        PlayerManager player = PlayerManager.Instance;
        if (player.CurrentHealth <= 0) return false;                 // 게임오버 후에는 사용 불가
        if (player.CurrentHealth >= player.MaxHealth) return false;  // 가득이면 낭비하지 않도록 소모 없이 무시
        if (CooldownRemaining > 0f) return false;

        if (!PlayerConsumables.Instance.TryConsumePotion(PotionIndex)) return false;

        PotionData data = PlayerConsumables.Instance.GetPotionData(PotionIndex);
        playerHealth.Heal(data.healAmount);
        nextUseTime = Time.time + data.cooldown;
        return true;
    }
}
