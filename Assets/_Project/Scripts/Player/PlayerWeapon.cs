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
    public WeaponData Data => Equipped.data;

    public int CurrentLevel => Equipped.CurrentLevel;
    public int CurrentDamage => Equipped.data.damageByLevel[Equipped.CurrentLevel - 1];
    public bool IsMaxLevel => Equipped.CurrentLevel >= Equipped.data.MaxLevel;
    public int NextUpgradeCost => Equipped.data.upgradeCostByLevel[Equipped.CurrentLevel - 1];

    public int MagazineAmmo => Equipped.MagazineAmmo;
    public int ReserveAmmo => Equipped.ReserveAmmo;
    public int MaxReserveAmmo => Equipped.data.maxReserveAmmo;
    public bool CanReload => Equipped.MagazineAmmo < Equipped.data.magazineSize && Equipped.ReserveAmmo > 0;

    private void Awake()
    {
        Instance = this;
        foreach(WeaponSlot slot in weaponSlots)
        {
            slot.MagazineAmmo = slot.data.magazineSize;
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
        int needed = Equipped.data.magazineSize - Equipped.MagazineAmmo;
        int actual = Mathf.Min(needed, Equipped.ReserveAmmo);

        Equipped.MagazineAmmo += actual;
        Equipped.ReserveAmmo -= actual;
    }

    public void AddReserveAmmo(int amount)
    {
        Equipped.ReserveAmmo += amount;
    }

    public int GetAmmoBundleCost(int bundleCount) => Equipped.data.ammoBundleCost * bundleCount;

    public int GetMaxAffordableBundles()
    {
        int needed = Equipped.data.maxReserveAmmo - ReserveAmmo;
        if (needed <= 0) return 0;
        return Mathf.CeilToInt((float)needed / Equipped.data.magazineSize);
    }

    public bool TryBuyAmmo(int bundleCount)
    {
        int cost = GetAmmoBundleCost(bundleCount);
        if (!CurrencyManager.Instance.TrySpend(cost)) return false;

        int amount = Mathf.Min(bundleCount * Equipped.data.magazineSize, Equipped.data.maxReserveAmmo - Equipped.ReserveAmmo);
        Equipped.ReserveAmmo += amount;
        Debug.Log($"탄약 {amount}발 구매 완료. 현재 예비탄약: {Equipped.ReserveAmmo}/{Equipped.data.maxReserveAmmo}");
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
            slot.MagazineAmmo = slot.data.magazineSize;
            slot.ReserveAmmo = slot.data.startingReserveAmmo;
        }
        equippedIndex = 0;
    }
}
