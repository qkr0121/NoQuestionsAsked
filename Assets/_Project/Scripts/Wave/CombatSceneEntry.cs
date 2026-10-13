using UnityEngine;

public class CombatSceneEntry : MonoBehaviour
{
    [SerializeField] private WaveSpawner waveSpawner;

    private void Start()
    {
        // 소환기에서 웨이브를 선택하지 않고 지상에 온 경우 - 좀비 없이 그냥 둘러볼 수 있는 상태
        // 이 경우 나가도 시간이 흐르지 않음(CombatExitPoint)
        if(RunState.Instance.SelectedWave == null)
        {
            return;
        }

        waveSpawner.BeginWave(RunState.Instance.SelectedWave, RunState.Instance.SelectedTimeOfDay);
    }
}
