using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponTabUI : MonoBehaviour
{
    [Header("목록")]
    [SerializeField] private WeaponRowUI[] weaponRows;  // 인덱스 = PlayerWeapon 슬롯 번호

    [Header("상세")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private StatRowUI[] statRows;      // StatDefs 개수(8)와 같아야 함
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text actionButtonText;

    // 탄퍼짐(spreadScale)은 ADS 퍼짐 구현 전까지 효과가 없어서 표시하지 않음
    private static readonly (string label, Func<WeaponData.LevelStats, string> format)[] StatDefs =
    {
        ("공격력",              s => s.damage.ToString()),
        ("발사 간격",           s => $"{s.fireInterval:0.0#}초"),
        ("재장전 시간",         s => $"{s.reloadTime:0.0#}초"),
        ("탄창",                s => $"{s.magazineSize}발"),
        ("예비 탄약 상한",      s => $"{s.maxReserveAmmo}발"),
        ("반동(밀림)",          s => $"{Mathf.RoundToInt(s.recoilScale * 100f)}%"),
        ("사격 중 이동 속도",   s => $"{Mathf.RoundToInt(s.fireSlowMultiplier * 100f)}%"),
        ("재장전 중 이동 속도", s => $"{Mathf.RoundToInt(s.reloadSlowMultiplier * 100f)}%"),
    };

    private int selectedIndex;

    private void Awake()
    {
        for (int i = 0; i < weaponRows.Length; i++)
        {
            int index = i;  // 람다가 루프 변수를 잡지 않도록 복사
            weaponRows[i].Init(() => Select(index));
        }
        actionButton.onClick.AddListener(OnActionClicked);
    }

    // 상점을 열 때 호출 - 전투에서 들고 있던 무기가 바로 보이도록 장착 무기로 시작
    public void Show()
    {
        selectedIndex = PlayerWeapon.Instance.EquippedIndex;
        Refresh();
    }

    private void Select(int index)
    {
        selectedIndex = index;
        Refresh();
    }

    private void Refresh()
    {
        PlayerWeapon weapon = PlayerWeapon.Instance;

        for (int i = 0; i < weaponRows.Length; i++)
        {
            weaponRows[i].Refresh(weapon.GetSlot(i), i == selectedIndex);
        }

        PlayerWeapon.WeaponSlot slot = weapon.GetSlot(selectedIndex);
        WeaponData data = slot.data;
        bool isMax = slot.IsUnlocked && slot.IsMaxLevel;

        nameText.text = data.displayName;
        levelText.text = slot.IsUnlocked ? $"Lv {slot.CurrentLevel} / {data.MaxLevel}" : "잠김";

        // 잠긴 무기는 1레벨 스탯을 미리 보여주고, 비교할 다음 레벨은 없음
        WeaponData.LevelStats current = slot.Stats;
        WeaponData.LevelStats next = (slot.IsUnlocked && !isMax) ? data.GetStats(slot.CurrentLevel + 1) : null;
        for (int i = 0; i < statRows.Length; i++)
        {
            statRows[i].Set(StatDefs[i].label, StatDefs[i].format(current), next != null ? StatDefs[i].format(next) : null);
        }

        if (!slot.IsUnlocked)
        {
            costText.text = $"{data.unlockCost} 크리스탈";
            actionButtonText.text = "구매";
            actionButton.interactable = true;
        }
        else if (isMax)
        {
            costText.text = "";
            actionButtonText.text = "강화 MAX";
            actionButton.interactable = false;
        }
        else
        {
            costText.text = $"{slot.NextUpgradeCost} 크리스탈";
            actionButtonText.text = "강화";
            actionButton.interactable = true;
        }
    }

    private void OnActionClicked()
    {
        PlayerWeapon weapon = PlayerWeapon.Instance;
        int index = selectedIndex;  // 확인창이 열려 있는 동안 선택이 바뀌어도 처음 누른 무기에 적용
        PlayerWeapon.WeaponSlot slot = weapon.GetSlot(index);

        if (!slot.IsUnlocked)
        {
            Confirm($"{slot.data.unlockCost} 크리스탈로 {slot.data.displayName}을(를) 구매하시겠습니까?", () => weapon.TryUnlock(index));
        }
        else
        {
            Confirm($"{slot.NextUpgradeCost} 크리스탈로 {slot.data.displayName}을(를) 강화하시겠습니까?", () => weapon.TryUpgrade(index));
        }
    }

    private void Confirm(string message, Func<bool> action)
    {
        ConfirmationDialog.Instance.Show(message, () =>
        {
            if (!action()) ToastMessage.Instance.Show("크리스탈이 부족합니다.");
            Refresh();
        });
    }
}
