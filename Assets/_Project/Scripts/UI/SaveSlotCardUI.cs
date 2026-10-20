using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotCardUI : MonoBehaviour
{
    [SerializeField] private Button selectButton;   // 카드 전체
    [SerializeField] private Button deleteButton;   // 채워진 칸에서만 보임
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
        selectButton.interactable = newGameMode ? !filled : filled;

        if (!filled)
        {
            infoText.text = "비어 있음";
            return;
        }

        string difficulty = summary.difficulty == GameDifficulty.Survival ? "서바이벌" : "노말";
        infoText.text = $"{summary.day}일차 · {difficulty}\n동기: {summary.motiveName}\n" +
                        $"크리스탈 {CurrencyManager.Format(summary.crystal)}\n{summary.savedAt:yyyy-MM-dd HH:mm}";
    }
}
