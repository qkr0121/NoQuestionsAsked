using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] private PlayerAim playerAim;
    [SerializeField] private PlayerGrenade playerGrenade;
    [SerializeField] private ParticleSystem muzzleFlash;    // FirePoint 자식, 비워둬도 동작

    private float cooldownTimer;
    private float reloadTimer;
    private bool isReloading;

    public bool IsReloading => isReloading;
    public float LastFireTime { get; private set; } = -999f; // 시작하자마자 감속 상태로 판정되지 않도록 아주 과거 값 

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
        //수류탄을 든 동안에는 좌클릭이 투척이므로 사격,재장전 입력을 받지 않음
        if (playerGrenade.IsHolding) return;

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
            cooldownTimer = PlayerWeapon.Instance.FireInterval;
        }
    }

    public void CancelReload()
    {
        isReloading = false;
    }

    private void TryStartReload()
    {
        if (!PlayerWeapon.Instance.CanReload) return;

        isReloading = true;
        reloadTimer = PlayerWeapon.Instance.ReloadTime;
    }

    private void Fire()
    {
        if (!InfiniteAmmoZone.IsActive && !PlayerWeapon.Instance.TryConsumeShot()) return;
        LastFireTime = Time.time;

        Transform fp = playerAim.FirePoint;
        // 캐릭터 회전은 보간돼서 늦게 따라오므로, 탄 방향은 조준점으로 직접 계산
        Vector3 dir = playerAim.AimPoint - fp.position;
        dir.y = 0f;
        Quaternion rot = dir.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(dir) : fp.rotation;

        Projectile projectile = ProjectilePool.Instance.Get(fp.position, rot);
        WeaponData data = PlayerWeapon.Instance.Data;
        projectile.Launch(PlayerWeapon.Instance.CurrentDamage, data.bulletSpeed, data.range);

        if(muzzleFlash != null)
        {
            muzzleFlash.transform.rotation = rot; // 캐릭터 회전이 늦게 따라와도 화염이 탄 방향을 향하게 함
            muzzleFlash.Play();
        }

        playerAim.ApplyRecoil(data);
    }
}
