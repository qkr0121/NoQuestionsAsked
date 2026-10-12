using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHotbar : MonoBehaviour
{
    [SerializeField] private PlayerShoot playerShoot;
    [SerializeField] private PlayerGrenade playerGrenade;
    [SerializeField] private PlayerPotion playerPotion;
    [SerializeField] private PlayerAim playerAim;

    private void Update()
    {
        if (PlayerInputLock.IsLocked) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.digit1Key.wasPressedThisFrame) SelectWeapon(0);
        if (keyboard.digit2Key.wasPressedThisFrame) SelectWeapon(1);
        if (keyboard.digit3Key.wasPressedThisFrame) SelectWeapon(2);
        if (keyboard.digit4Key.wasPressedThisFrame) playerGrenade.OnGrenadeKey();
        if (keyboard.qKey.wasPressedThisFrame) playerPotion.TryUse();
    }

    private void SelectWeapon(int index)
    {
        PlayerWeapon weapon = PlayerWeapon.Instance;
        // 잠겼거나 없는 슬롯이면 아무 일도 하지 않음 - 들고 있던 수류탄도 그대로 둠
        if (index >= weapon.SlotCount || !weapon.GetSlot(index).IsUnlocked) return;

        // 수류탄을 든 채 지금 무기의 번호를 눌러도 내려놓음
        playerGrenade.PutDown();

        if (!weapon.EquipWeapon(index)) return;   // 이미 든 무기면 재장전을 취소하지 않음
        playerShoot.CancelReload();
        playerAim.ResetBurst();
    }
}
