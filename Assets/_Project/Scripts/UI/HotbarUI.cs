using UnityEngine;

public class HotbarUI : MonoBehaviour
{
    [SerializeField] private HotbarSlotUI[] weaponSlots;   // 1, 2, 3번 칸 순서
    [SerializeField] private HotbarSlotUI grenadeSlot;     // 4번 칸
    [SerializeField] private HotbarSlotUI potionSlot;      // Q 칸

    private PlayerGrenade playerGrenade;
    private PlayerPotion playerPotion;

    private const int GrenadeIndex = 0;   // 지금은 수류탄·회복약 1종만 사용 (PlayerPotion, 상점 행과 동일)
    private const int PotionIndex = 0;

    // 플레이어는 씬 시작 후에 생성되므로 ScenePlayerSpawner가 코드로 넘겨줌
    public void SetPlayer(PlayerGrenade grenade, PlayerPotion potion)
    {
        playerGrenade = grenade;
        playerPotion = potion;
    }

    private void Update()
    {
        if (playerGrenade == null) return;   // 플레이어가 생성되기 전

        PlayerWeapon weapon = PlayerWeapon.Instance;
        bool holdingGrenade = playerGrenade.IsHolding;

        for (int i = 0; i < weaponSlots.Length; i++)
        {
            // 무기 슬롯 수보다 뒤의 칸(3번)은 영구 잠금
            if (i >= weapon.SlotCount || !weapon.GetSlot(i).IsUnlocked)
            {
                weaponSlots[i].ShowLocked();
                continue;
            }

            // 수류탄을 든 동안에는 무기 칸이 아니라 수류탄 칸이 선택 상태
            weaponSlots[i].ShowWeapon(weapon.GetSlot(i), !holdingGrenade && i == weapon.EquippedIndex);
        }

        grenadeSlot.ShowCount(PlayerConsumables.Instance.GetGrenadeCount(GrenadeIndex), holdingGrenade);
        potionSlot.ShowPotion(PlayerConsumables.Instance.GetPotionCount(PotionIndex), playerPotion.CooldownRemaining);
    }
}
