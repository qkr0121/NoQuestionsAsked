using UnityEngine;

public class WaveSelector : MonoBehaviour
{
    [SerializeField] private WaveDefinition testWave;
    [SerializeField] private TimeOfDayModifier testTimeOfDay;

    public void SelectTestWave()
    {
        RunState.Instance.SelectWave(testWave, testTimeOfDay);
        Debug.Log($"Selected wave: {testWave.WaveName} / {testTimeOfDay.TimeOfDayName}");
    }
}
