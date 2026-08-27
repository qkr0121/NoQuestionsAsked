using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(InteractableStructure))]
public class CombatExitPoint : MonoBehaviour
{
    [SerializeField] private WaveSpawner waveSpawner;

    public void TryExit()
    {
        if (!waveSpawner.CanExit) return;

        if (waveSpawner.IsCleared)
        {
            RunState.Instance.AddSegmentProgress(CurrencyManager.Instance.Crystal);
        }

        RunState.Instance.ClearSelectedWave();
        SceneManager.LoadScene("Base");
    }
}
