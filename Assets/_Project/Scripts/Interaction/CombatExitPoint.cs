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
            SceneManager.LoadScene("Base");
        });
    }
}
