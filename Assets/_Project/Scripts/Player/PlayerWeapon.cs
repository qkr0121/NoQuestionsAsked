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
