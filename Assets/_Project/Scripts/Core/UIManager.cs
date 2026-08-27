using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private GameOverUI gameOverUI;
    private GameUI gameUI;

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterGameOverUI(GameOverUI ui) => gameOverUI = ui;
    public void RegisterGameUI(GameUI ui) => gameUI = ui;

    public void ShowGameOver()
    {
        gameOverUI?.Show();
    }
}
