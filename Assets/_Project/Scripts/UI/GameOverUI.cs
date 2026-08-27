using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text finalScoreText;

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
        gameOverPanel.SetActive(true);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
