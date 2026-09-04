using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // [SerializeField]로 미리 연결하지 않고, 씬 전용 UI 가 스스로 Start()에서 RegisterXxx를 호출해 등록하는 방식
    // UIManager(영속 오브젝트)가 씬 전용 오브젝트를 직접 몰라도 되도록 결합도를 낮추기 위함
    private GameOverUI gameOverUI;

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterGameOverUI(GameOverUI ui) => gameOverUI = ui;

    public void ShowGameOver()
    {
        gameOverUI?.Show(); // Base 씬처럼 GameOverUI 자체가 없는 씬이 있으므로 null이면 조용히 무시
    }
}
