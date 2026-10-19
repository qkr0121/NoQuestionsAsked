using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponVisual : MonoBehaviour
{
    [SerializeField] private Transform weaponSocket;     // RightHand 아래 WeaponSocket
    [SerializeField] private GameObject grenadeModel;    // 수류탄을 든 동안만 표시 (WeaponSocket 아래, 종류가 늘면 GrenadeData 로 이동)
    [SerializeField] private float fireHoldTime = 0.8f;  // 쏜 뒤 조준 자세를 유지하는 시간 - 저격총은 발사 간격이 길어 1발마다 자세가 풀리지 않게

    private PlayerGrenade playerGrenade;
    private PlayerShoot playerShoot;
    private Animator animator;
    private WeaponData shownData;
    private GameObject shownModel;
    private readonly Dictionary<WeaponData, GameObject> cache = new Dictionary<WeaponData, GameObject>();

    public Transform Muzzle { get; private set; }   // 지금 손에 든 총의 총구, 없으면 null (수류탄을 든 동안 등)

    private void Awake()
    {
        playerGrenade = GetComponent<PlayerGrenade>();
        playerShoot = GetComponent<PlayerShoot>();   // 같은 Player 루트에 있음
        animator = GetComponentInChildren<Animator>();   // 모델이 자식 FBX라 GetComponentInChildren 필요
    }

    private void Update()
    {
        PlayerWeapon weapon = PlayerWeapon.Instance;
        if (weapon == null) return;

        bool holdingGrenade = playerGrenade.IsHolding;

        // 수류탄은 소총 자세로 들면 어색해서 항상 권총 자세로 듦
        if (animator != null)
        {
            animator.SetBool("IsRifle", !holdingGrenade && weapon.Data.useRifleAnimation);
            animator.SetBool("IsFiring", Time.time - playerShoot.LastFireTime < fireHoldTime);
        }

        // 수류탄을 든 동안에는 총을 숨김
        WeaponData wanted = holdingGrenade ? null : weapon.Data;
        if (wanted != shownData)
        {
            if (shownModel != null) shownModel.SetActive(false);
            shownModel = wanted != null ? GetOrCreate(wanted) : null;
            if (shownModel != null) shownModel.SetActive(true);
            shownData = wanted;

            WeaponMuzzle marker = shownModel != null ? shownModel.GetComponentInChildren<WeaponMuzzle>() : null;
            Muzzle = marker != null ? marker.transform : null;
        }

        // 던져서 0개가 된 뒤에는 빈 손으로 계속 들고 있으므로, 남은 수류탄이 있을 때만 모델을 보임
        bool showGrenade = holdingGrenade && playerGrenade.HasGrenade();
        if (grenadeModel != null && grenadeModel.activeSelf != showGrenade) grenadeModel.SetActive(showGrenade);
    }

    // 한 번 만든 모델은 꺼 두고 재사용 - 무기를 바꿀 때마다 생성/삭제하지 않도록
    private GameObject GetOrCreate(WeaponData data)
    {
        if (data.heldModel == null) return null;
        if (!cache.TryGetValue(data, out GameObject model))
        {
            model = Instantiate(data.heldModel, weaponSocket);
            model.SetActive(false);
            cache[data] = model;
        }
        return model;
    }
}
