using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPotion : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;

    private const int PotionIndex = 0;   // 지금은 회복약 1종만 사용 (PotionRowUI와 동일)
    private float nextUseTime;

    // 핫바(Issue 6)가 쿨타임 표시에 쓸 값
    public float CooldownRemaining => Mathf.Max(0f, nextUseTime - Time.time);

    private void Update()
    {
        if (PlayerInputLock.IsLocked) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.qKey.wasPressedThisFrame) TryUse();
    }

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
