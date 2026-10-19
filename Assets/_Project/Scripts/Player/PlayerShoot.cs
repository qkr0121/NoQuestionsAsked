using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] private PlayerAim playerAim;
    [SerializeField] private PlayerGrenade playerGrenade;
    [SerializeField] private ParticleSystem muzzleFlash;    // 무기에 화염 프리팹이 없을 때의 대체, FirePoint 자식, 비워둬도 동작

    private readonly Dictionary<WeaponData, MuzzleFlashEffect> flashCache = new Dictionary<WeaponData, MuzzleFlashEffect>();

    private PlayerWeaponVisual weaponVisual;

    private float reloadTimer;
    private float reloadDuration;   // 재장전 시작 때 저장 - 진행률 계산에 필요
    private bool isReloading;

    public bool IsReloading => isReloading;
    // 재장전 게이지용 진행률 (0~1)
    public float ReloadProgress => isReloading && reloadDuration > 0f ? 1f - reloadTimer / reloadDuration : 0f;
    public float LastFireTime { get; private set; } = -999f; // 시작하자마자 감속 상태로 판정되지 않도록 아주 과거 값

    private void Awake()
    {
        weaponVisual = GetComponent<PlayerWeaponVisual>();   // 같은 Player 루트에 있음
    }

    private void Update()
    {
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

        Keyboard keyboard = Keyboard.current;
        if(keyboard != null && keyboard.rKey.wasPressedThisFrame)
        {
            TryStartReload();
            return;
        }

        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        if(mouse.leftButton.isPressed && PlayerWeapon.Instance.CanFire)
        {
            Fire();
            PlayerWeapon.Instance.StartFireCooldown();
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
        reloadDuration = PlayerWeapon.Instance.ReloadTime;
        reloadTimer = reloadDuration;
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
        projectile.Launch(PlayerWeapon.Instance.CurrentDamage, data.bulletSpeed, data.range,
            PlayerWeapon.Instance.CurrentHitSlowMultiplier, PlayerWeapon.Instance.CurrentHitSlowDuration);

        Transform muzzle = weaponVisual != null ? weaponVisual.Muzzle : null;
        Vector3 flashPosition = muzzle != null ? muzzle.position : fp.position;   // 총구 표식이 없는 무기는 기존 FirePoint

        MuzzleFlashEffect flash = GetFlash(data, fp);
        if (flash != null)
        {
            flash.Play(flashPosition, rot);
        }
        else if (muzzleFlash != null)
        {
            muzzleFlash.transform.rotation = rot; // 캐릭터 회전이 늦게 따라와도 화염이 탄 방향을 향하게 함
            muzzleFlash.Play();
        }

        playerAim.ApplyRecoil(data);
    }

    // 한 번 만든 화염은 FirePoint 아래에 두고 재사용 - 발사할 때마다 생성/삭제하지 않도록
    private MuzzleFlashEffect GetFlash(WeaponData data, Transform parent)
    {
        if (data.muzzleFlashPrefab == null) return null;

        if (!flashCache.TryGetValue(data, out MuzzleFlashEffect flash))
        {
            flash = Instantiate(data.muzzleFlashPrefab, parent);   // FirePoint 자식이라 플레이어를 따라다님
            flash.transform.localPosition = Vector3.zero;
            flashCache[data] = flash;
        }
        return flash;
    }
}
