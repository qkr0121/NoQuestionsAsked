using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] private PlayerAim playerAim;

    private float cooldownTimer;
    private float reloadTimer;
    private bool isReloading;

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        // 테스트용 무기 교체 로직
        if (keyboard != null)
        {
            if (keyboard.digit1Key.wasPressedThisFrame) PlayerWeapon.Instance.EquipWeapon(0);
            if (keyboard.digit2Key.wasPressedThisFrame) PlayerWeapon.Instance.EquipWeapon(1);
        }

        if (PlayerInputLock.IsLocked) return;

        if(isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if(reloadTimer <= 0f)
            {
                PlayerWeapon.Instance.Reload();
                isReloading = false;
            }
            return;
        }

        cooldownTimer -= Time.deltaTime;

        //Keyboard keyboard = Keyboard.current;
        if(keyboard != null && keyboard.rKey.wasPressedThisFrame)
        {
            TryStartReload();
            return;
        }

        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        if(mouse.leftButton.isPressed && cooldownTimer <= 0f)
        {
            Fire();
            cooldownTimer = PlayerWeapon.Instance.Data.fireCooldown;
        }
    }

    private void TryStartReload()
    {
        if (!PlayerWeapon.Instance.CanReload) return;

        isReloading = true;
        reloadTimer = PlayerWeapon.Instance.Data.reloadTime;
    }

    private void Fire()
    {
        if (!InfiniteAmmoZone.IsActive && !PlayerWeapon.Instance.TryConsumeShot()) return;

        Transform fp = playerAim.FirePoint;
        Projectile projectile = ProjectilePool.Instance.Get(fp.position, fp.rotation);
        projectile.SetDamage(PlayerWeapon.Instance.CurrentDamage);
    }
}
