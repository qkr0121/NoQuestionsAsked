using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(InteractableStructure))]
public class CombatExitPoint : MonoBehaviour
{
    [SerializeField] private WaveSpawner waveSpawner;

    public void TryExit()
    {
        if (!waveSpawner.CanExit) return;

        RunState.Instance.ClearSelectedWave();
        RunState.Instance.AdvanceSlot();
        SceneManager.LoadScene("Base");
    }
}
