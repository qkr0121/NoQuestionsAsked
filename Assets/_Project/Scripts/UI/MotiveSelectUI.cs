using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;

public class MotiveSelectUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;           // 처음에 꺼 둠
    [SerializeField] private MotiveCardUI[] cards;       // MotiveData.OfferCount(4) 이상의 칸
    [FormerlySerializedAs("confirmButton")]
    [SerializeField] private Button signButton;
    [FormerlySerializedAs("confirmLabel")]
    [SerializeField] private TMP_Text signLabel;         // 서명 버튼의 글자
    [SerializeField] private PaperStamp stamp;
    [SerializeField] private Button backButton;

    private int[] offered;          // 이번에 제시한 동기 번호
    private int selectedSlot = -1;  // 선택한 카드 칸
    private bool signing;
    private Action<int> onConfirmed;
    private Action onCancelled;

    private void Awake()
    {
        for (int i = 0; i < cards.Length; i++)
        {
            int slot = i;  // 람다가 루프 변수를 잡지 않도록 복사
            cards[i].Init(() => Select(slot));
        }
        signButton.onClick.AddListener(OnSignClicked);
        backButton.onClick.AddListener(() => { if (!signing) onCancelled?.Invoke(); });
    }

    public void Open(Action<int> confirmed, Action cancelled)
    {
        onConfirmed = confirmed;
        onCancelled = cancelled;

        MotiveData data = RunState.Instance.Motives;
        offered = DrawRandom(data.Count, data.OfferCount);
        selectedSlot = -1;
        signing = false;

        for (int i = 0; i < cards.Length; i++)
        {
            bool used = i < offered.Length;
            cards[i].gameObject.SetActive(used);
            if (used)
            {
                MotiveData.Motive motive = data.Get(offered[i]);
                cards[i].Show(motive.displayName, motive.description);
            }
        }

        stamp.Hide();
        signButton.interactable = true;   // 동기는 비워 둔 채 서명할 수 있음
        UpdateSignLabel();
        panel.SetActive(true);
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    private void Select(int slot)
    {
        if (signing) return;

        selectedSlot = selectedSlot == slot ? -1 : slot;   // 선택한 줄을 다시 누르면 해제
        for (int i = 0; i < offered.Length; i++)
            cards[i].SetSelected(i == selectedSlot);
        UpdateSignLabel();
    }

    private void OnSignClicked()
    {
        if (signing) return;

        signing = true;
        signButton.interactable = false;
        // 고르지 않고 서명하면 "답하지 않음"(-1)으로 시작
        int motiveIndex = selectedSlot >= 0 ? offered[selectedSlot] : -1;
        stamp.Play(() =>
        {
            signing = false;
            onConfirmed?.Invoke(motiveIndex);
        });
    }

    private void UpdateSignLabel()
    {
        signLabel.text = selectedSlot >= 0 ? "제출" : "답하지 않고 제출";
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
