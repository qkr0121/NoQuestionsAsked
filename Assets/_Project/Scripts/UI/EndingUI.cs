using UnityEngine;
using TMPro;

public class EndingUI : MonoBehaviour
{
    [SerializeField] private GameObject endingPanel;   // 처음에 꺼진 자식 패널 + LocksInputWhileActive
    [SerializeField] private TMP_Text motiveEndingText;
    [SerializeField] private TMP_Text finalCrystalText;

    private void Awake()
    {
        endingPanel.SetActive(false);
    }

    public void Show()
    {
        MotiveData.Motive motive = RunState.Instance.CurrentMotive;
        // 지원 동기에 답하지 않은 런은 전용 텍스트를 보여줌
        motiveEndingText.text = motive != null ? motive.endingText : RunState.Instance.Motives.NoAnswerEndingText;
        finalCrystalText.text = $"최종 크리스탈 {CurrencyManager.Instance.Crystal}";
        endingPanel.SetActive(true);
    }

    public void OnReturnToMainMenuClicked()
    {
        // 세이브는 로켓 탑승 때 이미 지웠고, 런 리셋은 로딩 화면이 가린 뒤에 하도록 표시만 함
        PendingRunReset.MarkForReset();
        SceneTransitioner.GoTo("MainMenu");
    }
}
