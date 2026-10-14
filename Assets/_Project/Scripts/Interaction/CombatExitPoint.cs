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
            // 웨이브 없이 둘러보기만 한 경우엔 시간이 흐르지 않음 - 웨이브 선택 여부가 시간 소모의 기준
            if (RunState.Instance.SelectedWave != null)
            {
                RunState.Instance.MarkWaveCleared(RunState.Instance.SelectedWave);   // EndDay 가 선택 웨이브를 지우기 전에 클리어 기록
                RunState.Instance.EndDay();
                // 마감 실패로 게임오버가 되었으면 이 씬에서 게임오버 화면을 보여주고 Base로 넘어가지 않음
                if (GameManager.Instance.IsGameOver) return;
            }
            SceneManager.LoadScene("Base");
        });
    }
}
