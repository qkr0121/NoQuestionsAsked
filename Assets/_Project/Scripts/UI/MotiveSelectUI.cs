using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MotiveSelectUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;           // 처음에 꺼 둠, 전체 화면 + 배경 Image
    [SerializeField] private MotiveCardUI[] cards;       // MotiveData.OfferCount(4) 이상의 칸
    [SerializeField] private Button confirmButton;
    [SerializeField] private TMP_Text confirmLabel;      // 확인 버튼의 글자
    [SerializeField] private Button backButton;

    private int[] offered;          // 이번에 제시한 동기 번호
    private int selectedSlot = -1;  // 선택한 카드 칸
    private Action<int> onConfirmed;
    private Action onCancelled;

    private void Awake()
    {
        for (int i = 0; i < cards.Length; i++)
        {
            int slot = i;  // 람다가 루프 변수를 잡지 않도록 복사
            cards[i].Init(() => Select(slot));
        }
        confirmButton.onClick.AddListener(OnConfirmClicked);
        backButton.onClick.AddListener(() => onCancelled?.Invoke());
    }

    public void Open(Action<int> confirmed, Action cancelled)
    {
        onConfirmed = confirmed;
        onCancelled = cancelled;

        MotiveData data = RunState.Instance.Motives;
        offered = DrawRandom(data.Count, data.OfferCount);
        selectedSlot = -1;

        panel.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    private void Select(int slot)
    {
        selectedSlot = selectedSlot == slot ? -1 : slot;   // 선택한 줄을 다시 누르면 해제
        Refresh();
    }

    private void OnConfirmClicked()
    {
        // 고르지 않고 확인하면 "답하지 않음"(-1)으로 시작
        int motiveIndex = selectedSlot >= 0 ? offered[selectedSlot] : -1;
        onConfirmed?.Invoke(motiveIndex);
    }

    private void Refresh()
    {
        MotiveData data = RunState.Instance.Motives;

        for (int i = 0; i < cards.Length; i++)
        {
            bool used = i < offered.Length;
            cards[i].gameObject.SetActive(used);
            if (used)
            {
                MotiveData.Motive motive = data.Get(offered[i]);
                cards[i].Show(motive.displayName, motive.description, i == selectedSlot);
            }
        }

        confirmLabel.text = selectedSlot >= 0 ? "확인" : "답하지 않는다";
    }

    // 앞에서부터 하나씩 확정하는 부분 셔플 - 중복 없이 count개를 뽑음
    private static int[] DrawRandom(int total, int count)
    {
        int[] pool = new int[total];
        for (int i = 0; i < total; i++) pool[i] = i;

        for (int i = 0; i < count; i++)
        {
            int j = UnityEngine.Random.Range(i, total);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        int[] result = new int[count];
        Array.Copy(pool, result, count);
        return result;
    }
}
