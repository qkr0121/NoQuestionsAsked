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

    [Header("무기 강화")]
    [SerializeField] private TMP_Text weaponLevelText;
    [SerializeField] private TMP_Text weaponUpgradeCostText;
    [SerializeField] private Button weaponUpgradeButton;

    [Header("방어벽 행")]
    [SerializeField] private BarricadeRowUI[] barricadeRows;

    [Header("소모품 행")]
    [SerializeField] private AmmoRowUI ammoRow;
    [SerializeField] private PotionRowUI potionRow;

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

        RefreshWeaponUpgrade();
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

        ammoRow.Refresh();
        potionRow.Refresh();
    }

    private void RefreshWeaponUpgrade()
    {
        weaponLevelText.text = $"{PlayerWeapon.Instance.CurrentLevel}단계";

        if(PlayerWeapon.Instance.IsMaxLevel)
        {
            weaponUpgradeCostText.text = "최대 강화";
            weaponUpgradeButton.interactable = false;
        }
        else
        {
            weaponUpgradeCostText.text = $"{PlayerWeapon.Instance.NextUpgradeCost} 크리스탈";
            weaponUpgradeButton.interactable = true;
        }
    }

    public void OnWeaponUpgradeClicked()
    {
        int cost = PlayerWeapon.Instance.NextUpgradeCost;
        ConfirmationDialog.Instance.Show($"{cost} 크리스탈로 강화하시겠습니까?", () =>
        {
            PlayerWeapon.Instance.TryUpgrade();
            RefreshWeaponUpgrade();
        });
    }
}
