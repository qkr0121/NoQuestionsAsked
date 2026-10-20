using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;

// 새 게임(빈 칸 선택)과 이어하기(채워진 칸 선택)가 같이 쓰는 세이브 칸 선택 패널
public class SaveSlotSelectUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;           // 처음에 꺼 둠
    [SerializeField] private SaveSlotCardUI[] cards;     // SaveManager.SlotCount 개
    [FormerlySerializedAs("titleText")]
    [SerializeField] private TMP_Text sectionText;       // 서류의 섹션 줄 (모드별 안내 문구)
    [SerializeField] private Button signButton;
    [SerializeField] private PaperStamp stamp;
    [SerializeField] private Button backButton;

    private bool newGameMode;
    private int selectedSlot = -1;
    private bool signing;
    private Action<int> onSelected;
    private Action onCancelled;

    private void Awake()
    {
        for (int i = 0; i < cards.Length; i++)
        {
            int slot = i;   // 람다가 루프 변수를 잡지 않도록 복사
            cards[i].Init(() => OnCardClicked(slot), () => OnDeleteClicked(slot));
        }
        signButton.onClick.AddListener(OnSignClicked);
        backButton.onClick.AddListener(() => { if (!signing) onCancelled?.Invoke(); });
    }

    public void Open(bool newGame, Action<int> selected, Action cancelled)
    {
        newGameMode = newGame;
        onSelected = selected;
        onCancelled = cancelled;
        signing = false;

        sectionText.text = newGame ? "새로 시작할 세이브 칸" : "이어할 세이브 칸";
        stamp.Hide();
        panel.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    // 목록을 다시 그릴 때마다 선택도 비움 (파기한 칸이 선택돼 있을 수 있음)
    private void Refresh()
    {
        selectedSlot = -1;
        signButton.interactable = false;
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].Show(i, SaveManager.Instance.GetSummary(i), newGameMode);
        }
    }

    private void OnCardClicked(int slot)
    {
        if (signing) return;
        // 카드가 선택 불가(interactable false)면 클릭이 오지 않지만 이중으로 막음
        if (SaveManager.Instance.HasSave(slot) == newGameMode) return;

        selectedSlot = selectedSlot == slot ? -1 : slot;   // 선택한 줄을 다시 누르면 해제
        for (int i = 0; i < cards.Length; i++)
            cards[i].SetSelected(i == selectedSlot);
        signButton.interactable = selectedSlot >= 0;
    }

    private void OnSignClicked()
    {
        if (selectedSlot < 0 || signing) return;

        signing = true;
        signButton.interactable = false;
        int slot = selectedSlot;
        stamp.Play(() =>
        {
            signing = false;
            onSelected?.Invoke(slot);
        });
    }

    private void OnDeleteClicked(int slot)
    {
        if (signing) return;

        ConfirmationDialog.Instance.Show("이 세이브를 삭제하시겠습니까?", () =>
        {
            SaveManager.Instance.DeleteSlot(slot);
            Refresh();
        });
    }
}
