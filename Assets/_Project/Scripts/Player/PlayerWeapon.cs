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
        public float NextFireTime { get; set; }   // 이 무기를 다시 쏠 수 있는 시각 - 무기별로 따로 두어야 교체로 쿨다운을 우회하지 못함

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
    public float CurrentHitSlowMultiplier => Stats.hitSlowMultiplier;
    public float CurrentHitSlowDuration => Stats.hitSlowDuration;
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
    public bool CanFire => Time.time >= Equipped.NextFireTime;
    public void StartFireCooldown() => Equipped.NextFireTime = Time.time + FireInterval;

    private void Awake()
    {
        Instance = this;
        ResetWeapon();
    }

    // 상점은 장착 무기와 무관하게 아무 슬롯이나 조회·강화하므로 슬롯을 통째로 내줌
    public WeaponSlot GetSlot(int index) => weaponSlots[index];

    public bool EquipWeapon(int index)
    {
        if (index < 0 || index >= weaponSlots.Length) return false;
        if (!weaponSlots[index].IsUnlocked) return false;
        if (index == equippedIndex) return false;   // 이미 든 무기

        equippedIndex = index;
        return true;
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
        if (!CurrencyManager.Instance.TrySpend(slot.NextUpgradeCost, CurrencyManager.Instance.UpgradeSound)) return false;

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

    public void LoadFrom(WeaponSaveEntry[] entries, int savedEquippedIndex)
    {
        // 저장에 없는 값이 메모리에 남지 않도록 먼저 전부 기본 상태로 되돌림
        ResetWeapon();
        if (entries == null) return;   // 이 필드가 생기기 전에 만든 세이브 파일

        int count = Mathf.Min(entries.Length, weaponSlots.Length);
        for (int i = 0; i < count; i++)
        {
            WeaponSlot slot = weaponSlots[i];
            WeaponSaveEntry entry = entries[i];

            // 밸런스를 수정한 뒤에도 옛 세이브가 범위를 넘는 값을 만들지 못하게 자름
            slot.CurrentLevel = Mathf.Clamp(entry.level, 1, slot.data.MaxLevel);
            slot.IsUnlocked = entry.isUnlocked || slot.data.unlockCost <= 0;

            WeaponData.LevelStats stats = slot.Stats;
            slot.MagazineAmmo = Mathf.Clamp(entry.magazineAmmo, 0, stats.magazineSize);
            slot.ReserveAmmo = Mathf.Clamp(entry.reserveAmmo, 0, stats.maxReserveAmmo);
        }

        // 범위를 벗어나거나 잠긴 슬롯이면 기본 무기로
        bool validIndex = savedEquippedIndex >= 0 && savedEquippedIndex < weaponSlots.Length;
        equippedIndex = validIndex && weaponSlots[savedEquippedIndex].IsUnlocked ? savedEquippedIndex : 0;
    }

    public void ResetWeapon()
    {
        foreach(WeaponSlot slot in weaponSlots)
        {
            slot.CurrentLevel = 1;
            slot.MagazineAmmo = slot.data.GetStats(1).magazineSize;
            slot.ReserveAmmo = slot.data.startingReserveAmmo;
            slot.IsUnlocked = slot.data.unlockCost <= 0;
            slot.NextFireTime = 0f;
        }
        equippedIndex = 0;
    }
}
