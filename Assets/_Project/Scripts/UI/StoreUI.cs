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
    [SerializeField] private GameObject mainMenuGroup;
    [SerializeField] private GameObject weaponUpgradeGroup;
    [SerializeField] private GameObject barricadeMenuGroup;

    [Header("권총 강화")]
    [SerializeField] private TMP_Text weaponLevelText;
    [SerializeField] private TMP_Text weaponUpgradeCostText;
    [SerializeField] private Button weaponUpgradeButton;

    [Header("방어벽 행")]
    [SerializeField] private BarricadeRowUI[] barricadeRows;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Open()
    {
        panel.SetActive(true);
        ShowMainMenu();
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    public void ShowMainMenu()
    {
        mainMenuGroup.SetActive(true);
        weaponUpgradeGroup.SetActive(false);
        barricadeMenuGroup.SetActive(false);
    }

    public void ShowWeaponUpgrade()
    {
        mainMenuGroup.SetActive(false);
        weaponUpgradeGroup.SetActive(true);
        barricadeMenuGroup.SetActive(false);
    }

    public void ShowBarricadeMenu()
    {
        mainMenuGroup.SetActive(false);
        weaponUpgradeGroup.SetActive(false);
        barricadeMenuGroup.SetActive(true);

        foreach(BarricadeRowUI row in barricadeRows)
        {
            row.Refresh();
        }
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
