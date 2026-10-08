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
        public bool IsUnlocked { get; set; }

        public WeaponData.LevelStats Stats => data.GetStats(CurrentLevel);
        public bool IsMaxLevel => CurrentLevel >= data.MaxLevel;
        // 다음 레벨 줄에 적힌 비용이 곧 그 레벨로 올리는 비용 (최대 레벨에서는 호출 금지)
        public int NextUpgradeCost => data.GetStats(CurrentLevel + 1).upgradeCost;

        // 상한을 넘는 만큼은 받지 않으므로, 비용도 실제로 받는 탄약 수 기준으로 계산함
        public int GetAmmoFillAmount(int bundleCount) => Mathf.Min(bundleCount * Stats.magazineSize, Stats.maxReserveAmmo - ReserveAmmo);
        public int GetAmmoCost(int bundleCount) => GetAmmoFillAmount(bundleCount) * data.ammoCostPerRound;

        public int GetMaxAffordableBundles()
        {
            int needed = Stats.maxReserveAmmo - ReserveAmmo;
            if (needed <= 0) return 0;
            return Mathf.CeilToInt((float)needed / Stats.magazineSize);
        }
    }

    public static PlayerWeapon Instance { get; private set; }

    [SerializeField] private WeaponSlot[] weaponSlots; // 인스펙터에서 보유 무기 목록 채움
    private int equippedIndex = 0;
    private WeaponSlot Equipped => weaponSlots[equippedIndex];
    private WeaponData.LevelStats Stats => Equipped.Stats;
    public WeaponData Data => Equipped.data;

    public int SlotCount => weaponSlots.Length;
    public int EquippedIndex => equippedIndex;

    public int CurrentLevel => Equipped.CurrentLevel;

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
        ResetWeapon();
    }

    // 상점은 장착 무기와 무관하게 아무 슬롯이나 조회·강화하므로 슬롯을 통째로 내줌
    public WeaponSlot GetSlot(int index) => weaponSlots[index];

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= weaponSlots.Length) return;
        if (!weaponSlots[index].IsUnlocked) return;
        equippedIndex = index;
    }

    public bool TryUnlock(int index)
    {
        WeaponSlot slot = weaponSlots[index];
        if (slot.IsUnlocked) return false;
        if (!CurrencyManager.Instance.TrySpend(slot.data.unlockCost)) return false;

        slot.IsUnlocked = true;
        return true;
    }

    public bool TryUpgrade(int index)
    {
        WeaponSlot slot = weaponSlots[index];
        if (!slot.IsUnlocked || slot.IsMaxLevel) return false;
        if (!CurrencyManager.Instance.TrySpend(slot.NextUpgradeCost)) return false;

        // 탄창이 커져도 현재 탄창의 탄은 그대로 - 다음 장전부터 커진 크기가 적용됨
        slot.CurrentLevel++;
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

    public bool TryBuyAmmo(int index, int bundleCount)
    {
        WeaponSlot slot = weaponSlots[index];
        int amount = slot.GetAmmoFillAmount(bundleCount);
        if (amount <= 0) return false;
        if (!CurrencyManager.Instance.TrySpend(slot.GetAmmoCost(bundleCount))) return false;

        slot.ReserveAmmo += amount;
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
            slot.IsUnlocked = slot.data.unlockCost <= 0;
        }
        equippedIndex = 0;
    }
}
