using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    public static PlayerWeapon Instance { get; private set; }

    [SerializeField] private WeaponData weaponData;
    public WeaponData Data => weaponData;

    public int CurrentLevel { get; private set; } = 1;
    public int CurrentDamage => weaponData.damageByLevel[CurrentLevel - 1];
    public bool IsMaxLevel => CurrentLevel >= weaponData.MaxLevel;
    public int NextUpgradeCost => weaponData.upgradeCostByLevel[CurrentLevel - 1];

    public int MagazineAmmo { get; private set; }
    public int ReserveAmmo { get; private set; }
    public int MaxReserveAmmo => weaponData.maxReserveAmmo;
    public bool CanReload => MagazineAmmo < weaponData.magazineSize && ReserveAmmo > 0;

    private void Awake()
    {
        Instance = this;
        MagazineAmmo = weaponData.magazineSize;
        ReserveAmmo = weaponData.startingReserveAmmo;
    }

    public bool TryUpgrade()
    {
        if (IsMaxLevel) return false;
        if (!CurrencyManager.Instance.TrySpend(NextUpgradeCost)) return false;

        CurrentLevel++;
        return true;
    }

    public bool TryConsumeShot()
    {
        if (MagazineAmmo <= 0) return false;

        MagazineAmmo--;
        return true;
    }

    public void Reload()
    {
        int needed = weaponData.magazineSize - MagazineAmmo;
        int actual = Mathf.Min(needed, ReserveAmmo);

        MagazineAmmo += actual;
        ReserveAmmo -= actual;
    }

    public void AddReserveAmmo(int amount)
    {
        ReserveAmmo += amount;
    }


    public int GetAmmoBundleCost(int bundleCount) => weaponData.ammoBundleCost * bundleCount;

    public int GetMaxAffordableBundles()
    {
        int needed = weaponData.maxReserveAmmo - ReserveAmmo;
        if (needed <= 0) return 0;
        return Mathf.CeilToInt((float)needed / weaponData.magazineSize);
    }

    public bool TryBuyAmmo(int bundleCount)
    {
        int cost = GetAmmoBundleCost(bundleCount);
        if (!CurrencyManager.Instance.TrySpend(cost)) return false;

        int amount = Mathf.Min(bundleCount * weaponData.magazineSize, weaponData.maxReserveAmmo - ReserveAmmo);
        ReserveAmmo += amount;
        Debug.Log($"탄약 {amount}발 구매 완료. 현재 예비탄약: {ReserveAmmo}/{weaponData.maxReserveAmmo}");
        return true;
    }

    public void LoadFrom(int level, int magazin, int reserve)
    {
        CurrentLevel = level;
        MagazineAmmo = magazin;
        ReserveAmmo = reserve;
    }

    public void ResetWeapon()
    {
        CurrentLevel = 1;
        MagazineAmmo = weaponData.magazineSize;
        ReserveAmmo = weaponData.startingReserveAmmo;
    }
}
