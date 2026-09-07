using UnityEngine;

public class CombatSceneEntry : MonoBehaviour
{
    [SerializeField] private WaveSpawner waveSpawner;

    private void Start()
    {
        // 소환기에서 웨이브를 선택하지 않고 지상에 온 경우 - 좀비 없이 그냥 둘러볼 수 있는 상태
        // GDD 미정 사항("웨이브 미선택 시 시간 흐름 여부")과 연관된 지점, 결정되면 이 분기 로직이 바뀔 수 있음
        if(RunState.Instance.SelectedWave == null)
        {
            return;
        }

        waveSpawner.BeginWave(RunState.Instance.SelectedWave, RunState.Instance.SelectedTimeOfDay);
    }
}
