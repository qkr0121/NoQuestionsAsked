using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class StoreUI : MonoBehaviour
{
    public static StoreUI Instance { get; private set; }

    [Header("패널")]
    [SerializeField] private GameObject panel;

    [Header("화면 그룹")]
    [SerializeField] private GameObject weaponUpgradeGroup;
    [SerializeField] private GameObject barricadeMenuGroup;
    [SerializeField] private GameObject consumableGroup;

    [Header("무기 탭")]
    [SerializeField] private WeaponTabUI weaponTab;

    [Header("방어벽 행")]
    [SerializeField] private BarricadeRowUI[] barricadeRows;

    [Header("소모품 행")]
    [SerializeField] private AmmoRowUI[] ammoRows;   // 무기 슬롯마다 한 행
    [SerializeField] private PotionRowUI potionRow;
    [SerializeField] private GrenadeRowUI grenadeRow;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Open()
    {
        panel.SetActive(true);
        ShowWeaponUpgrade();
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    public void ShowWeaponUpgrade()
    {
        weaponUpgradeGroup.SetActive(true);
        barricadeMenuGroup.SetActive(false);
        consumableGroup.SetActive(false);

        weaponTab.Show();
    }

    public void ShowBarricadeMenu()
    {
        weaponUpgradeGroup.SetActive(false);
        barricadeMenuGroup.SetActive(true);
        consumableGroup.SetActive(false);

        foreach (BarricadeRowUI row in barricadeRows)
        {
            row.Refresh();
        }
    }

    public void ShowConsumables()
    {
        weaponUpgradeGroup.SetActive(false);
        barricadeMenuGroup.SetActive(false);
        consumableGroup.SetActive(true);

        foreach (AmmoRowUI row in ammoRows)
        {
            // 사지 않은 무기의 탄약은 살 수 없으므로 행 자체를 숨김
            bool unlocked = PlayerWeapon.Instance.GetSlot(row.WeaponIndex).IsUnlocked;
            row.gameObject.SetActive(unlocked);
            if (unlocked) row.Refresh();
        }
        potionRow.Refresh();
        grenadeRow.Refresh();
    }
}
