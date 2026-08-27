using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void GameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        Time.timeScale = 0f;
        UIManager.Instance.ShowGameOver();
    }
}
