using UnityEngine;

public class CombatSceneEntry : MonoBehaviour
{
    [SerializeField] private WaveSpawner waveSpawner;

    private void Start()
    {
        if(RunState.Instance.SelectedWave == null)
        {
            return;
        }

        waveSpawner.BeginWave(RunState.Instance.SelectedWave, RunState.Instance.SelectedTimeOfDay);
    }
}
