using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MotiveCardUI : MonoBehaviour
{
    [SerializeField] private Button selectButton;         // 줄 전체를 누를 수 있는 투명 버튼
    [SerializeField] private TMP_Text nameText;           // 굵은 제목
    [SerializeField] private TMP_Text descriptionText;    // 제목 아래 작은 설명
    [SerializeField] private PaperCheckBox checkBox;      // 줄 오른쪽 끝 ㅁ 칸

    public void Init(Action onClick)
    {
        selectButton.onClick.AddListener(() => onClick());
    }

    // 문구만 채우고 칸은 비운 채로 시작
    public void Show(string motiveName, string description)
    {
        nameText.text = motiveName;
        descriptionText.text = description;
        checkBox.SetChecked(false, false);
    }

    // 상태가 바뀐 칸만 다시 그려서, 이미 체크된 칸의 V가 매번 새로 그어지지 않게 함
    public void SetSelected(bool selected)
    {
        if (checkBox.IsChecked != selected) checkBox.SetChecked(selected);
    }
}
