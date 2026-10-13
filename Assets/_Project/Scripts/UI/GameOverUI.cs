using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text survivedDaysText;

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
        finalScoreText.text = $"Final Crystal: {CurrencyManager.Instance.Crystal}";
        // 날짜가 이미 넘어간 뒤 판정되는 마감 실패까지 포함해 "끝까지 보낸 날"로 통일
        survivedDaysText.text = $"Survived: {RunState.Instance.CurrentDay - 1} Day";
        gameOverPanel.SetActive(true);
    }

    public void OnReturnToMainMenuClicked()
    {
        Time.timeScale = 1f; // GameManager.GameOver() 가 멈춘 시간을 여기서 되돌림

        PendingRunReset.MarkForReset();
        SaveManager.Instance.DeleteSave();

        SceneTransitioner.GoTo("MainMenu");
    }
}
