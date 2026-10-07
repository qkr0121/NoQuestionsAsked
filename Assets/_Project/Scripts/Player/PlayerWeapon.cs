using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [System.Serializable]
    public class WeaponSlot
    {
        public WeaponData data;

        public int CurrentLevel { get; set; } = 1;
        public int MagazineAmmo { get; set; }
        public int ReserveAmmo { get; set; }
    }

    public static PlayerWeapon Instance { get; private set; }

    [SerializeField] private WeaponSlot[] weaponSlots; // 인스펙터에서 보유 무기 목록 채움
    private int equippedIndex = 0;
    private WeaponSlot Equipped => weaponSlots[equippedIndex];
    private WeaponData.LevelStats Stats => Equipped.data.GetStats(Equipped.CurrentLevel);
    public WeaponData Data => Equipped.data;

    public int CurrentLevel => Equipped.CurrentLevel;
    public bool IsMaxLevel => Equipped.CurrentLevel >= Equipped.data.MaxLevel;
    // 다음 레벨 줄에 적힌 비용이 곧 그 레벨로 올리는 비용
    public int NextUpgradeCost => Equipped.data.GetStats(Equipped.CurrentLevel + 1).upgradeCost;

    public int CurrentDamage => Stats.damage;
    public float FireInterval => Stats.fireInterval;
    public float RecoilScale => Stats.recoilScale;
    public float SpreadScale => Stats.spreadScale;
    public float ReloadTime => Stats.reloadTime;
    public float FireSlowMultiplier => Stats.fireSlowMultiplier;
    public float ReloadSlowMultiplier => Stats.reloadSlowMultiplier;
    public int MagazineSize => Stats.magazineSize;
    public int MaxReserveAmmo => Stats.maxReserveAmmo;

    public int MagazineAmmo => Equipped.MagazineAmmo;
    public int ReserveAmmo => Equipped.ReserveAmmo;
    public bool CanReload => Equipped.MagazineAmmo < Stats.magazineSize && Equipped.ReserveAmmo > 0;

    private void Awake()
    {
        Instance = this;
        foreach(WeaponSlot slot in weaponSlots)
        {
            slot.MagazineAmmo = slot.data.GetStats(1).magazineSize;
            slot.ReserveAmmo = slot.data.startingReserveAmmo;
        }
    }

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= weaponSlots.Length) return;
        equippedIndex = index;
    }

    public bool TryUpgrade()
    {
        if (IsMaxLevel) return false;
        if (!CurrencyManager.Instance.TrySpend(NextUpgradeCost)) return false;

        // 탄창이 커져도 현재 탄창의 탄은 그대로 - 다음 장전부터 커진 크기가 적용됨
        Equipped.CurrentLevel++;
        return true;
    }

    public bool TryConsumeShot()
    {
        if (MagazineAmmo <= 0) return false;

        Equipped.MagazineAmmo--;
        return true;
    }

    public void Reload()
    {
        int needed = Stats.magazineSize - Equipped.MagazineAmmo;
        int actual = Mathf.Min(needed, Equipped.ReserveAmmo);

        Equipped.MagazineAmmo += actual;
        Equipped.ReserveAmmo -= actual;
    }

    public void AddReserveAmmo(int amount)
    {
        Equipped.ReserveAmmo += amount;
    }

    // 상한을 넘는 만큼은 받지 않으므로, 비용도 실제로 받는 탄약 수 기준으로 계산함
    public int GetAmmoFillAmount(int bundleCount) => Mathf.Min(bundleCount * MagazineSize, MaxReserveAmmo - ReserveAmmo);
    public int GetAmmoCost(int bundleCount) => GetAmmoFillAmount(bundleCount) * Equipped.data.ammoCostPerRound;

    public int GetMaxAffordableBundles()
    {
        int needed = Stats.maxReserveAmmo - ReserveAmmo;
        if (needed <= 0) return 0;
        return Mathf.CeilToInt((float)needed / Stats.magazineSize);
    }

    public bool TryBuyAmmo(int bundleCount)
    {
        int amount = GetAmmoFillAmount(bundleCount);
        if (amount <= 0) return false;
        if (!CurrencyManager.Instance.TrySpend(GetAmmoCost(bundleCount))) return false;

        Equipped.ReserveAmmo += amount;
        Debug.Log($"탄약 {amount}발 구매 완료. 현재 예비탄약: {Equipped.ReserveAmmo}/{Stats.maxReserveAmmo}");
        return true;
    }

    public void LoadFrom(int level, int magazin, int reserve)
    {
        Equipped.CurrentLevel = level;
        Equipped.MagazineAmmo = magazin;
        Equipped.ReserveAmmo = reserve;
    }

    public void ResetWeapon()
    {
        foreach(WeaponSlot slot in weaponSlots)
        {
            slot.CurrentLevel = 1;
            slot.MagazineAmmo = slot.data.GetStats(1).magazineSize;
            slot.ReserveAmmo = slot.data.startingReserveAmmo;
        }
        equippedIndex = 0;
    }
}
