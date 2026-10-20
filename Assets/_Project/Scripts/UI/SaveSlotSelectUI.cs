using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 새 게임(빈 칸 선택)과 이어하기(채워진 칸 선택)가 같이 쓰는 세이브 칸 선택 패널
public class SaveSlotSelectUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;           // 처음에 꺼 둠, 전체 화면 + 배경 Image
    [SerializeField] private SaveSlotCardUI[] cards;     // SaveManager.SlotCount 개
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button backButton;

    private bool newGameMode;
    private Action<int> onSelected;
    private Action onCancelled;

    private void Awake()
    {
        for (int i = 0; i < cards.Length; i++)
        {
            int slot = i;   // 람다가 루프 변수를 잡지 않도록 복사
            cards[i].Init(() => OnCardClicked(slot), () => OnDeleteClicked(slot));
        }
        backButton.onClick.AddListener(() => onCancelled?.Invoke());
    }

    public void Open(bool newGame, Action<int> selected, Action cancelled)
    {
        newGameMode = newGame;
        onSelected = selected;
        onCancelled = cancelled;

        titleText.text = newGame ? "새로 시작할 세이브 칸" : "이어할 세이브 칸";
        panel.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    private void Refresh()
    {
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].Show(i, SaveManager.Instance.GetSummary(i), newGameMode);
        }
    }

    private void OnCardClicked(int slot)
    {
        // 카드가 선택 불가(interactable false)면 클릭이 오지 않지만 이중으로 막음
        if (SaveManager.Instance.HasSave(slot) == newGameMode) return;
        onSelected?.Invoke(slot);
    }

    private void OnDeleteClicked(int slot)
    {
        ConfirmationDialog.Instance.Show("이 세이브를 삭제하시겠습니까?", () =>
        {
            SaveManager.Instance.DeleteSlot(slot);
            Refresh();
        });
    }
}
