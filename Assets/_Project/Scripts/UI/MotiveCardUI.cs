using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MotiveCardUI : MonoBehaviour
{
    [SerializeField] private Button selectButton;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;   // 주제 오른쪽 칸에 설명 표시
    [SerializeField] private GameObject selectedMark;

    public void Init(Action onClick)
    {
        selectButton.onClick.AddListener(() => onClick());
    }

    public void Show(string motiveName, string description, bool selected)
    {
        nameText.text = motiveName;
        descriptionText.text = description;
        selectedMark.SetActive(selected);
    }
}
