using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(InteractableStructure))]
public class CombatExitPoint : MonoBehaviour
{
    [SerializeField] private WaveSpawner waveSpawner;

    public void TryExit()
    {
        // 모든 좀비를 처치하지 않으면 나갈 수 없음
        if (!waveSpawner.CanExit) return;

        ScreenFader.Instance.FadeAction(() =>
        {
            RunState.Instance.ClearSelectedWave();
            RunState.Instance.AdvanceSlot();
            // 마감 실패로 게임오버가 되었으면 이 씬에서 게임오버 화면을 보여주고 Base로 넘어가지 않음
            if (GameManager.Instance.IsGameOver) return;
            SceneManager.LoadScene("Base");
        });
    }
}
