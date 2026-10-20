using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotCardUI : MonoBehaviour
{
    [SerializeField] private Button selectButton;        // 줄 전체를 누를 수 있는 투명 버튼
    [SerializeField] private Button deleteButton;        // "파기" 도장 버튼, 채워진 칸에서만 보임. 줄 버튼보다 앞에 그려야 클릭을 받음
    [SerializeField] private CanvasGroup contentGroup;   // 라벨·정보·ㅁ칸을 묶은 자식 (파기 버튼은 흐려지지 않게 밖에 둠)
    [SerializeField] private PaperCheckBox checkBox;
    [SerializeField] private TMP_Text slotLabel;
    [SerializeField] private TMP_Text infoText;

    public void Init(Action onSelect, Action onDelete)
    {
        selectButton.onClick.AddListener(() => onSelect());
        deleteButton.onClick.AddListener(() => onDelete());
    }

    public void Show(int slot, SaveSlotSummary summary, bool newGameMode)
    {
        slotLabel.text = $"세이브 {slot + 1}";
        bool filled = summary != null;

        deleteButton.gameObject.SetActive(filled);

        // 새 게임은 빈 칸만, 이어하기는 채워진 칸만 선택할 수 있음
        bool selectable = newGameMode ? !filled : filled;
        selectButton.interactable = selectable;
        contentGroup.alpha = selectable ? 1f : 0.4f;
        checkBox.gameObject.SetActive(selectable);
        checkBox.SetChecked(false, false);

        if (!filled)
        {
            infoText.text = "비어 있음";
            return;
        }

        string difficulty = summary.difficulty == GameDifficulty.Survival ? "서바이벌" : "노말";
        infoText.text = $"{summary.day}일차 · {difficulty}\n동기: {summary.motiveName}\n" +
                        $"크리스탈 {CurrencyManager.Format(summary.crystal)}\n{summary.savedAt:yyyy-MM-dd HH:mm}";
    }

    // 상태가 바뀐 칸만 다시 그려서, 이미 체크된 칸의 V가 매번 새로 그어지지 않게 함
    public void SetSelected(bool selected)
    {
        if (checkBox.IsChecked != selected) checkBox.SetChecked(selected);
    }
}
