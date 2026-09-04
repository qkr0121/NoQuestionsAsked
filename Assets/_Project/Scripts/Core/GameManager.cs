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
        if (IsGameOver) return; // 여러 경로(플레이어 사망 이벤트 등)에서 동시에 호출돼도 한 번만 처리되도록 하는 가드

        IsGameOver = true;
        Time.timeScale = 0f;
        UIManager.Instance.ShowGameOver();
    }
}
