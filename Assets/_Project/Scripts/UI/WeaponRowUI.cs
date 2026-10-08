using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponRowUI : MonoBehaviour
{
    [SerializeField] private Button selectButton;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text statusText;       // "Lv 3 / 5" 또는 "잠김"
    [SerializeField] private GameObject selectedMark;
    [SerializeField] private GameObject lockedMark;     // 잠긴 무기를 회색으로 덮는 반투명 오브젝트

    public void Init(Action onClick)
    {
        selectButton.onClick.AddListener(() => onClick());
    }

    public void Refresh(PlayerWeapon.WeaponSlot slot, bool selected)
    {
        nameText.text = slot.data.displayName;
        statusText.text = slot.IsUnlocked ? $"Lv {slot.CurrentLevel} / {slot.data.MaxLevel}" : "잠김";
        selectedMark.SetActive(selected);
        lockedMark.SetActive(!slot.IsUnlocked);
    }
}
