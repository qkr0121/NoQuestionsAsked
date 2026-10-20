using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text survivedDaysText;
    [SerializeField] private TMP_Text saveNoticeText;   // 난이도별 세이브 안내, 비워둬도 동작

    private void Awake()
    {
        gameOverPanel.SetActive(false);
    }

    private void Start()
    {
        UIManager.Instance.RegisterGameOverUI(this);
    }

    public void Show()
    {
        finalScoreText.text = $"Final Crystal: {CurrencyManager.Format(CurrencyManager.Instance.Crystal)}";
        // 날짜가 이미 넘어간 뒤 판정되는 마감 실패까지 포함해 "끝까지 보낸 날"로 통일
        survivedDaysText.text = $"Survived: {RunState.Instance.CurrentDay - 1} Day";

        if (saveNoticeText != null)
        {
            saveNoticeText.text = RunState.Instance.Difficulty == GameDifficulty.Survival
                ? "서바이벌 - 세이브가 삭제됩니다."
                : "마지막 저장 시점부터 이어하기로 다시 시작할 수 있습니다.";
        }
        gameOverPanel.SetActive(true);
    }

    public void OnReturnToMainMenuClicked()
    {
        Time.timeScale = 1f; // GameManager.GameOver() 가 멈춘 시간을 여기서 되돌림

        PendingRunReset.MarkForReset();

        // 서바이벌은 사망하면 세이브를 지우고, 노말은 마지막 저장을 남겨 이어하기로 재개함
        // 난이도는 LoadingScreen 이 런을 리셋하기 전인 지금 읽어야 함
        if (RunState.Instance.Difficulty == GameDifficulty.Survival)
        {
            SaveManager.Instance.DeleteSave();
        }

        SceneTransitioner.GoTo("MainMenu");
    }
}
