using UnityEngine;

public enum TimeSlot { Morning, Day, Night }

public class RunState : MonoBehaviour
{
    public static RunState Instance { get; private set; }

    [SerializeField] private int currentDay = 1;
    [SerializeField] private int segmentQuota = 100;
    [SerializeField] private int segmentProgress = 0;

    public int CurrentDay => currentDay;
    public int SegmentQuota => SegmentQuota;
    public int SegmentProgress => SegmentProgress;

    public WaveDefinition SelectedWave { get; private set; }
    public TimeOfDayModifier SelectedTimeOfDay { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void SelectWave(WaveDefinition wave, TimeOfDayModifier timeOfDay)
    {
        SelectedWave = wave;
        SelectedTimeOfDay = timeOfDay;
    }

    public void AddSegmentProgress(int amount)
    {
        segmentProgress += amount;
    }

    public void AdvanceDay()
    {
        currentDay++;
    }

    public bool IsSegmentDeadline()
    {
        return currentDay % 10 == 0;
    }

    public bool IsSegmentQuotaMet()
    {
        return segmentProgress >= segmentQuota;
    }

    public void ClearSelectedWave()
    {
        SelectedWave = null;
        SelectedTimeOfDay = null;
    }
}
